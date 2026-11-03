package com.wany864.livecaption;

import android.app.Activity;
import android.content.Intent;
import android.os.Bundle;
import android.os.Handler;
import android.os.Looper;
import android.speech.RecognitionListener;
import android.speech.RecognizerIntent;
import android.speech.SpeechRecognizer;
import com.unity3d.player.UnityPlayer;
import java.util.ArrayList;
import java.util.Locale;

/** Android SpeechRecognizer bridge for Unity. Foreground-only by design. */
public final class LiveCaptionSpeechBridge {
    private static Activity activity;
    private static SpeechRecognizer recognizer;
    private static Handler handler;
    private static String unityObject = "LiveCaptionApp";
    private static boolean active = false;

    private LiveCaptionSpeechBridge() {}

    public static void start(Activity host, String callbackObject) {
        activity = host;
        unityObject = callbackObject == null ? "LiveCaptionApp" : callbackObject;
        handler = new Handler(Looper.getMainLooper());
        activity.runOnUiThread(() -> {
            if (!SpeechRecognizer.isRecognitionAvailable(activity)) {
                error("No Android speech recognition service is available");
                return;
            }
            active = true;
            ensureRecognizer();
            startSession();
        });
    }

    public static void stop() {
        active = false;
        if (handler != null) handler.removeCallbacksAndMessages(null);
        if (activity != null) activity.runOnUiThread(() -> {
            if (recognizer != null) {
                try { recognizer.cancel(); } catch (Exception ignored) {}
                try { recognizer.destroy(); } catch (Exception ignored) {}
                recognizer = null;
            }
        });
    }

    private static void ensureRecognizer() {
        if (recognizer != null) return;
        recognizer = SpeechRecognizer.createSpeechRecognizer(activity);
        recognizer.setRecognitionListener(new RecognitionListener() {
            @Override public void onReadyForSpeech(Bundle params) { status("Listening · English"); }
            @Override public void onBeginningOfSpeech() { status("Listening · English"); }
            @Override public void onRmsChanged(float rmsdB) {}
            @Override public void onBufferReceived(byte[] buffer) {}
            @Override public void onEndOfSpeech() { status("Processing…"); }
            @Override public void onError(int code) {
                if (!active) return;
                if (code == SpeechRecognizer.ERROR_NO_MATCH || code == SpeechRecognizer.ERROR_SPEECH_TIMEOUT) {
                    scheduleRestart(300);
                } else if (code == SpeechRecognizer.ERROR_RECOGNIZER_BUSY) {
                    scheduleRestart(700);
                } else if (code == SpeechRecognizer.ERROR_INSUFFICIENT_PERMISSIONS) {
                    active = false;
                    error("Microphone permission denied");
                } else if (code == SpeechRecognizer.ERROR_NETWORK || code == SpeechRecognizer.ERROR_NETWORK_TIMEOUT) {
                    active = false;
                    error("Network unavailable · Try again");
                } else {
                    status("Recognition error · restarting");
                    scheduleRestart(900);
                }
            }
            @Override public void onResults(Bundle results) {
                sendFirstResult(results);
                scheduleRestart(250);
            }
            @Override public void onPartialResults(Bundle partial) { sendFirstResult(partial); }
            @Override public void onEvent(int eventType, Bundle params) {}
        });
    }

    private static void sendFirstResult(Bundle bundle) {
        if (bundle == null) return;
        ArrayList<String> values = bundle.getStringArrayList(SpeechRecognizer.RESULTS_RECOGNITION);
        if (values != null && !values.isEmpty() && values.get(0) != null) {
            String text = values.get(0).trim();
            if (!text.isEmpty()) UnityPlayer.UnitySendMessage(unityObject, "OnCaption", text);
        }
    }

    private static void startSession() {
        if (!active || activity == null) return;
        try {
            if (recognizer == null) ensureRecognizer();
            Intent intent = new Intent(RecognizerIntent.ACTION_RECOGNIZE_SPEECH);
            intent.putExtra(RecognizerIntent.EXTRA_LANGUAGE_MODEL, RecognizerIntent.LANGUAGE_MODEL_FREE_FORM);
            intent.putExtra(RecognizerIntent.EXTRA_LANGUAGE, "en-US");
            intent.putExtra(RecognizerIntent.EXTRA_LANGUAGE_PREFERENCE, "en-US");
            intent.putExtra(RecognizerIntent.EXTRA_PARTIAL_RESULTS, true);
            intent.putExtra(RecognizerIntent.EXTRA_MAX_RESULTS, 1);
            recognizer.startListening(intent);
        } catch (Exception e) {
            error("Unable to start recognition: " + e.getMessage());
            scheduleRestart(1200);
        }
    }

    private static void scheduleRestart(long delay) {
        if (!active || handler == null) return;
        handler.removeCallbacksAndMessages(null);
        handler.postDelayed(() -> {
            if (!active) return;
            try { if (recognizer != null) recognizer.cancel(); } catch (Exception ignored) {}
            startSession();
        }, delay);
    }

    private static void status(String value) {
        UnityPlayer.UnitySendMessage(unityObject, "OnSpeechStatus", value);
    }

    private static void error(String value) {
        UnityPlayer.UnitySendMessage(unityObject, "OnSpeechError", value);
    }
}
