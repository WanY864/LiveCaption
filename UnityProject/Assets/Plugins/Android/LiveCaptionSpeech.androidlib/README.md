# LiveCaption Android Unity plugin

This Android Library Project exposes Android SpeechRecognizer to Unity through
UnityPlayer.UnitySendMessage. Unity's Gradle export merges this library and its
manifest when building the Android player.

The plugin requests the en-US recognition locale and partial results. The Android
speech recognition provider controls whether partial results are delivered and
whether recognition is on-device or network-backed.
