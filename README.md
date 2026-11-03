# LiveCaption

**LiveCaption** is a Unity 6 Android app prototype with two modes:

- **Sticky Board (primary):** an offline, touch-friendly sticky-note board. Create and edit notes, drag note cards around, change note colors, and add draggable decorative stickers.
- **Live Captions (optional):** the original English-only Android speech-recognition interface and Java bridge are retained as an optional feature for devices that provide a compatible Android speech-recognition service.

The sticky board does not require a speech service, network connection, account, or server. Notes and sticker positions are saved locally in the app's persistent data folder as JSON.

## Main Unity project

Open `UnityProject/` with **Unity 6.6 (6000.6.5f1)**.

### Sticky Board features
- Create multiple notes with the `+ NOTE` button.
- Tap inside a note to edit it using the phone keyboard.
- Drag a note by its header to reposition it.
- Tap `COLOR` on a note to cycle through pastel colors.
- Add star, heart, sparkle, and check-mark stickers from the toolbar.
- Drag stickers anywhere on the board.
- Note text, colors, positions, and sticker positions are saved locally and restored on next launch.
- No sign-in, cloud sync, or network permission is needed for the notes feature.

### Optional Live Captions
- English (US / `en-US`) speech-recognition bridge using Android `SpeechRecognizer`.
- Partial results replace the current caption when the device recognition provider supports them.
- Start / pause / clear, font-size and caption-background opacity controls.
- Speech recognition is optional. Some Android devices do not include a compatible speech-recognition service; on those devices the Notes mode remains fully usable and the Captions mode reports an error rather than disabling the app.

## Build on Windows

1. Install Unity Hub and Unity 6.6.5f1.
2. In Unity Hub, install **Android Build Support**, including Android SDK & NDK Tools and OpenJDK.
3. Open `UnityProject/`.
4. Wait for Unity to import assets. The startup scene is `Assets/Scenes/LiveCaption.unity`; the app UI is created automatically at runtime.
5. In **File → Build Profiles**, choose Android, switch platform, and ensure `Assets/Scenes/LiveCaption.unity` is included in the Scene List.
6. Set Package Name to `com.wany864.livecaption`, Minimum API Level to Android 8.0 / API 26 or higher, and use ARM64. Use Unity-installed JDK, SDK, NDK, and Gradle.
7. Build an APK.
8. Transfer the APK to the Android phone and install it. On first launch, Notes mode opens by default. No microphone permission is required for notes.

## Use the Sticky Board

- Tap **+ NOTE** to create a note.
- Tap the note body to type.
- Drag the note's top header to move it.
- Tap **COLOR** to cycle its background.
- Tap **★**, **♥**, **✦**, or **✓** to add a sticker, then drag the sticker to place it.
- Switch to **CAPTIONS** only if you want to try the optional speech-recognition feature.

## Data and privacy

- Notes are saved locally at `Application.persistentDataPath/livecaption_sticky_board.json`.
- The app does not upload note text, stickers, or positions.
- Android speech recognition is a separate optional feature. The Android speech provider may process audio remotely; offline speech recognition is not guaranteed.
- The app pauses speech recognition when it is sent to the background.

## HCI study

See [docs/HCI-EXPERIMENT.md](docs/HCI-EXPERIMENT.md) for a proposed mobile usability study focused on note creation, drag-and-drop placement, sticker use, and the optional captions mode. Record actual participant observations; do not invent results.

## Project structure

```
UnityProject/
  Assets/
    Scenes/LiveCaption.unity
    Scripts/
      LiveCaptionApp.cs
      StickyNotesBoard.cs
    Plugins/Android/LiveCaptionSpeech.androidlib/
      src/main/java/com/wany864/livecaption/LiveCaptionSpeechBridge.java
app/                         # Optional Kotlin reference prototype
docs/HCI-EXPERIMENT.md
```

## Status

The Sticky Board and optional captions integration have been added to the Unity source and committed to GitHub. The updated project has **not yet been compiled in the user's Unity editor or tested on the phone**. Build and device verification are still required.

## License

MIT. See [LICENSE](LICENSE).
