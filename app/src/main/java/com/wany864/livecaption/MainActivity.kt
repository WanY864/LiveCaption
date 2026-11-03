package com.wany864.livecaption

import android.Manifest
import android.content.pm.PackageManager
import android.os.Bundle
import androidx.activity.ComponentActivity
import androidx.activity.compose.rememberLauncherForActivityResult
import androidx.activity.compose.setContent
import androidx.activity.result.contract.ActivityResultContracts
import androidx.compose.foundation.background
import androidx.compose.foundation.layout.*
import androidx.compose.foundation.shape.CircleShape
import androidx.compose.foundation.shape.RoundedCornerShape
import androidx.compose.material3.*
import androidx.compose.runtime.*
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.graphics.Color
import androidx.compose.ui.text.style.TextAlign
import androidx.compose.ui.text.style.TextOverflow
import androidx.compose.ui.unit.dp
import androidx.compose.ui.unit.sp
import androidx.core.content.ContextCompat

private val Ink = Color(0xFF080B12)
private val Panel = Color(0xFF151B27)
private val Accent = Color(0xFF71E0C1)
private val Muted = Color(0xFF929BAA)
private val Paper = Color(0xFFF4F7FB)

class MainActivity : ComponentActivity() {
    override fun onCreate(savedInstanceState: Bundle?) {
        super.onCreate(savedInstanceState)
        window.statusBarColor = android.graphics.Color.rgb(8, 11, 18)
        window.navigationBarColor = android.graphics.Color.rgb(8, 11, 18)
        setContent { LiveCaptionApp() }
    }
}

@Composable
private fun LiveCaptionApp() {
    val context = androidx.compose.ui.platform.LocalContext.current
    var caption by remember { mutableStateOf("") }
    var status by remember { mutableStateOf("Ready") }
    var listening by remember { mutableStateOf(false) }
    var twoLines by remember { mutableStateOf(false) }
    var largeText by remember { mutableStateOf(true) }

    val controller = remember {
        EnglishSpeechController(
            context = context,
            onCaption = { caption = it },
            onStatus = { status = it },
            onListeningChanged = { listening = it }
        )
    }
    DisposableEffect(controller) { onDispose { controller.destroy() } }

    val permissionLauncher = rememberLauncherForActivityResult(
        ActivityResultContracts.RequestPermission()
    ) { granted ->
        if (granted) controller.start()
        else status = "Microphone permission is required"
    }

    MaterialTheme {
        Surface(Modifier.fillMaxSize(), color = Ink) {
            Column(
                Modifier.fillMaxSize().padding(horizontal = 24.dp, vertical = 22.dp),
                verticalArrangement = Arrangement.SpaceBetween
            ) {
                Row(verticalAlignment = Alignment.CenterVertically) {
                    Box(Modifier.size(10.dp).background(if (listening) Accent else Muted, CircleShape))
                    Spacer(Modifier.width(10.dp))
                    Text(
                        if (listening) "LISTENING · ENGLISH" else status.uppercase(),
                        color = if (listening) Accent else Muted,
                        fontSize = 12.sp,
                        letterSpacing = 1.3.sp
                    )
                    Spacer(Modifier.weight(1f))
                    Text("LIVE CAPTION", color = Muted, fontSize = 11.sp, letterSpacing = 1.2.sp)
                }

                Box(
                    Modifier.fillMaxWidth()
                        .background(Panel, RoundedCornerShape(24.dp))
                        .padding(horizontal = 22.dp, vertical = 30.dp),
                    contentAlignment = Alignment.CenterStart
                ) {
                    Text(
                        text = caption.ifBlank { "Your English captions will appear here…" },
                        color = if (caption.isBlank()) Muted else Paper,
                        fontSize = if (largeText) 30.sp else 23.sp,
                        lineHeight = if (largeText) 39.sp else 31.sp,
                        maxLines = if (twoLines) 2 else 1,
                        overflow = TextOverflow.Ellipsis,
                        textAlign = TextAlign.Start
                    )
                }

                Column(verticalArrangement = Arrangement.spacedBy(12.dp)) {
                    Row(Modifier.fillMaxWidth(), horizontalArrangement = Arrangement.SpaceBetween) {
                        Text("Caption layout", color = Muted, fontSize = 13.sp)
                        Row {
                            TextButton(onClick = { twoLines = false }) {
                                Text("1 line", color = if (!twoLines) Accent else Muted)
                            }
                            TextButton(onClick = { twoLines = true }) {
                                Text("2 lines", color = if (twoLines) Accent else Muted)
                            }
                        }
                    }
                    Row(Modifier.fillMaxWidth(), horizontalArrangement = Arrangement.SpaceBetween) {
                        Text("Text size", color = Muted, fontSize = 13.sp)
                        Row {
                            TextButton(onClick = { largeText = false }) {
                                Text("A", color = if (!largeText) Accent else Muted)
                            }
                            TextButton(onClick = { largeText = true }) {
                                Text("A+", color = if (largeText) Accent else Muted, fontSize = 19.sp)
                            }
                        }
                    }
                    Row(Modifier.fillMaxWidth(), horizontalArrangement = Arrangement.spacedBy(10.dp)) {
                        Button(
                            Modifier.weight(1f).height(54.dp),
                            onClick = {
                                if (listening) controller.pause()
                                else {
                                    val granted = ContextCompat.checkSelfPermission(
                                        context, Manifest.permission.RECORD_AUDIO
                                    ) == PackageManager.PERMISSION_GRANTED
                                    if (granted) controller.start()
                                    else permissionLauncher.launch(Manifest.permission.RECORD_AUDIO)
                                }
                            },
                            colors = ButtonDefaults.buttonColors(containerColor = Accent, contentColor = Ink),
                            shape = RoundedCornerShape(16.dp)
                        ) { Text(if (listening) "Pause" else "Start listening", fontSize = 15.sp) }

                        OutlinedButton(
                            Modifier.height(54.dp),
                            onClick = { controller.pause(); caption = ""; status = "Ready" },
                            shape = RoundedCornerShape(16.dp)
                        ) { Text("Clear", color = Paper) }
                    }
                    Text(
                        "English only · Keep this screen open while listening",
                        Modifier.fillMaxWidth(),
                        color = Muted,
                        fontSize = 11.sp,
                        textAlign = TextAlign.Center
                    )
                }
            }
        }
    }
}
