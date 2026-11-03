# LiveCaption

A minimal English-only live caption app for Android, built as a **Unity 6 project** for the HCI Mobile Lab.

The main course implementation is in [UnityProject](UnityProject/). It uses Unity for the app UI and interaction, plus a small Android Java plugin that calls Android's SpeechRecognizer. A standalone Kotlin/Compose prototype remains in `app/` as an optional native reference; it is **not** the Unity course deliverable.

## Course requirement fit

The HCI Mobile Lab asks students to adapt their previous application for a mobile device, use an appropriate mobile interaction method, deploy a first test to a phone or Android emulator, and document the work. This repository therefore uses Unity as the application framework and adds Android speech recognition as a native plugin. The current concept is LiveCaption; confirm with the instructor whether replacing the previous desktop app with this new captioning app is acceptable.

## Main Unity project

Open `UnityProject/` using **Unity 6 (6000.0.x)**.

### Features
- Minimal dark interface with one current caption (no transcript history).
- English (US / `en-US`) speech recognition.
- Partial results replace the current caption when the device recognizer provides them.
- Start / pause / clear.
- One-line / two-line caption layout and text-size controls.
- Runtime microphone permission request.
- Editor demo mode with sample text so the UI can be reviewed without an Android device.
- Stops listening when the app is backgrounded; foreground-only by design.

### Build on Windows
1. Install Unity Hub and Unity 6.
2. In Unity Hub, install **Android Build Support**, including Android SDK & NDK Tools and OpenJDK.
3. Open `UnityProject/`.
4. Wait for Unity to import assets. The startup scene is `Assets/Scenes/LiveCaption.unity`; the UI is created automatically at runtime.
5. In **File → Build Profiles** (or Build Settings), choose Android, switch platform, and select the listed LiveCaption scene.
6. Set Package Name to `com.wany864.livecaption`, Minimum API Level to Android 8.0 / API 26 or higher, and Scripting Backend to IL2CPP or Mono (use the default supported by your Unity install).
7. Enable **Development Build** for the first test, then Build an APK.
8. On your OPPO phone, enable Developer options and USB debugging, connect over USB, approve the prompt, and install the APK from Unity or with `adb install -r path-to-apk.apk`.
9. On first launch, tap **Start listening** and grant microphone access.

### Editor preview
Press Play in Unity Editor. The Start button cycles through sample English captions. This is only a UI preview; actual speech recognition runs on Android.

## Android plugin

`Assets/Plugins/Android/LiveCaptionSpeech.androidlib/` is an Android Library Project. It wraps Android's `SpeechRecognizer` and sends caption/status callbacks to the Unity GameObject named `LiveCaptionApp` using `UnityPlayer.UnitySendMessage`.

The recognizer requests `en-US` and partial results. Provider behavior varies by phone. Android may use a network speech service; offline recognition is not guaranteed. Android's SpeechRecognizer is not designed for unrestricted always-on continuous recognition and can consume battery/bandwidth. The app pauses when sent to the background.

## Optional native reference project

The root Gradle project and `app/` directory contain an independent Kotlin/Jetpack Compose implementation used as a reference prototype. The HCI submission path is the Unity project above.

## HCI experiment

See [docs/HCI-EXPERIMENT.md](docs/HCI-EXPERIMENT.md). Proposed study: compare one-line and two-line caption layouts with the same speaker, phone, script, and font size. Measure comprehension, readability, perceived effort, preference, and recognition issues. Do not invent participant results.

## Privacy

- Microphone permission is requested only when Start is pressed.
- LiveCaption itself does not save recordings or build a transcript history.
- The Android speech recognition provider may process audio remotely. Tell participants this during testing; do not describe the app as fully offline.
- Keep the app in the foreground during use.

## Status

The Unity source and Android plugin are committed, but this project has **not yet been imported, compiled in Unity, or tested on the OPPO phone**. The GitHub Actions workflow currently builds the optional Kotlin reference app, not the Unity project. The Unity APK must be built from Unity 6 on your Windows machine with Android Build Support installed.

## License

MIT. See [LICENSE](LICENSE).
