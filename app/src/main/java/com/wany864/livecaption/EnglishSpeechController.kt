package com.wany864.livecaption

import android.content.Context
import android.content.Intent
import android.os.Bundle
import android.os.Handler
import android.os.Looper
import android.speech.RecognitionListener
import android.speech.RecognizerIntent
import android.speech.SpeechRecognizer

/**
 * Foreground English speech-to-text controller.
 *
 * Interim results replace the current caption. Final results also replace it,
 * so this app intentionally does not build a transcript history.
 */
class EnglishSpeechController(
    context: Context,
    private val onCaption: (String) -> Unit,
    private val onStatus: (String) -> Unit,
    private val onListeningChanged: (Boolean) -> Unit
) {
    private val appContext = context.applicationContext
    private val handler = Handler(Looper.getMainLooper())
    private var recognizer: SpeechRecognizer? = null
    private var active = false
    private var disposed = false

    private val restartTask = Runnable {
        if (active && !disposed) startRecognitionSession()
    }

    fun start() {
        if (disposed || active) return
        if (!SpeechRecognizer.isRecognitionAvailable(appContext)) {
            onStatus("No speech recognition service available")
            onListeningChanged(false)
            return
        }
        active = true
        onListeningChanged(true)
        onStatus("Starting microphone…")
        startRecognitionSession()
    }

    fun pause() {
        active = false
        handler.removeCallbacks(restartTask)
        try { recognizer?.cancel() } catch (_: Exception) { }
        onListeningChanged(false)
        onStatus("Paused")
    }

    fun destroy() {
        if (disposed) return
        disposed = true
        active = false
        handler.removeCallbacksAndMessages(null)
        try { recognizer?.cancel() } catch (_: Exception) { }
        try { recognizer?.destroy() } catch (_: Exception) { }
        recognizer = null
        onListeningChanged(false)
    }

    private fun ensureRecognizer(): Boolean {
        if (recognizer != null) return true
        return try {
            recognizer = SpeechRecognizer.createSpeechRecognizer(appContext).also {
                it.setRecognitionListener(listener)
            }
            true
        } catch (e: Exception) {
            onStatus("Could not start speech recognition: ${e.localizedMessage ?: "unknown error"}")
            onListeningChanged(false)
            active = false
            false
        }
    }

    private fun startRecognitionSession() {
        if (!active || disposed || !ensureRecognizer()) return
        val intent = Intent(RecognizerIntent.ACTION_RECOGNIZE_SPEECH).apply {
            putExtra(RecognizerIntent.EXTRA_LANGUAGE_MODEL, RecognizerIntent.LANGUAGE_MODEL_FREE_FORM)
            putExtra(RecognizerIntent.EXTRA_LANGUAGE, "en-US")
            putExtra(RecognizerIntent.EXTRA_LANGUAGE_PREFERENCE, "en-US")
            putExtra(RecognizerIntent.EXTRA_ONLY_RETURN_LANGUAGE_PREFERENCE, true)
            putExtra(RecognizerIntent.EXTRA_PARTIAL_RESULTS, true)
            putExtra(RecognizerIntent.EXTRA_MAX_RESULTS, 1)
            putExtra(RecognizerIntent.EXTRA_CALLING_PACKAGE, appContext.packageName)
        }
        try {
            recognizer?.startListening(intent)
        } catch (e: Exception) {
            onStatus("Unable to listen: ${e.localizedMessage ?: "recognizer error"}")
            scheduleRestart(1200L)
        }
    }

    private fun scheduleRestart(delayMs: Long) {
        handler.removeCallbacks(restartTask)
        if (active && !disposed) handler.postDelayed(restartTask, delayMs)
    }

    private fun firstText(bundle: Bundle?): String? =
        bundle?.getStringArrayList(SpeechRecognizer.RESULTS_RECOGNITION)
            ?.firstOrNull()
            ?.trim()
            ?.takeIf { it.isNotEmpty() }

    private val listener = object : RecognitionListener {
        override fun onReadyForSpeech(params: Bundle?) {
            if (active) onStatus("Listening · English")
        }

        override fun onBeginningOfSpeech() {
            if (active) onStatus("Listening · English")
        }

        override fun onRmsChanged(rmsdB: Float) = Unit
        override fun onBufferReceived(buffer: ByteArray?) = Unit

        override fun onEndOfSpeech() {
            if (active) onStatus("Processing…")
        }

        override fun onError(error: Int) {
            if (!active || disposed) return
            when (error) {
                SpeechRecognizer.ERROR_NO_MATCH,
                SpeechRecognizer.ERROR_SPEECH_TIMEOUT -> {
                    onStatus("Listening · English")
                    scheduleRestart(350L)
                }
                SpeechRecognizer.ERROR_RECOGNIZER_BUSY -> scheduleRestart(700L)
                SpeechRecognizer.ERROR_INSUFFICIENT_PERMISSIONS -> {
                    active = false
                    onListeningChanged(false)
                    onStatus("Microphone permission denied")
                }
                SpeechRecognizer.ERROR_NETWORK,
                SpeechRecognizer.ERROR_NETWORK_TIMEOUT -> {
                    active = false
                    onListeningChanged(false)
                    onStatus("Network unavailable · Try again")
                }
                else -> {
                    onStatus("Recognition error ($error) · restarting")
                    scheduleRestart(900L)
                }
            }
        }

        override fun onResults(results: Bundle?) {
            firstText(results)?.let(onCaption)
            if (active && !disposed) scheduleRestart(250L)
        }

        override fun onPartialResults(partialResults: Bundle?) {
            firstText(partialResults)?.let(onCaption)
        }

        override fun onEvent(eventType: Int, params: Bundle?) = Unit
    }
}
