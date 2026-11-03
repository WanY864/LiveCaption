# LiveCaption

**Minimal, English-first live captions for Android.**

LiveCaption keeps the screen focused on one current caption. Interim speech-recognition results replace the previous text instead of building a transcript history.

## Current prototype

- Android native app written in Kotlin + Jetpack Compose (no Unity dependency).
- Android SpeechRecognizer configured for English (US).
- Live partial-result updates when the device recognition provider supports them.
- One current caption only; no transcript history is stored by the app.
- Start / pause / clear controls.
- One-line / two-line layout and text-size controls for the HCI experiment.
- Microphone permission and recognition error states.
- Foreground-only: keep the app open while listening.

> **Prototype status:** source code is committed, but it has not yet been built or tested on the user's OPPO device. A GitHub Actions workflow builds a debug APK and publishes it as an artifact. Download the artifact from the latest successful Actions run.

## Build on Windows

### Requirements
- Windows 10/11
- Android Studio (includes Android SDK and a compatible JDK)
- Android SDK Platform 35
- Gradle 8.9 (or use Android Studio's Gradle integration)
- An OPPO Android phone and a USB data cable

### Build with Android Studio
1. Install Android Studio: <https://developer.android.com/studio>
2. Clone this repository.
3. Open the repository root in Android Studio.
4. Let Gradle sync. If Android Studio asks to install Android SDK Platform 35 / Build Tools, accept.
5. Select **Build → Build Bundle(s) / APK(s) → Build APK(s)**.
6. Android Studio writes the debug APK under `app/build/outputs/apk/debug/app-debug.apk`.

### Build from PowerShell
If Gradle 8.9 is installed and on PATH:

```powershell
gradle --version
gradle assembleDebug
```

The APK is at `app\build\outputs\apk\debug\app-debug.apk`.

## Install on an OPPO phone

1. On the phone, enable Developer options (Settings → About device → tap Version number / Build number repeatedly; exact menu names vary by ColorOS version).
2. Enable USB debugging in Developer options.
3. Connect the phone using a data-capable USB cable and approve the debugging prompt.
4. Install Android Platform Tools (ADB), then run:

```powershell
adb devices
adb install -r app\build\outputs\apk\debug\app-debug.apk
```

You can also copy the APK to the phone and open it, allowing installation from that file source when Android prompts.

## GitHub Actions APK

Every push to `main` and every pull request runs the Android debug build. Open the repository's **Actions** tab, select the latest successful **Android Debug APK** run, and download the `LiveCaption-debug-apk` artifact. GitHub artifacts are temporary build outputs, not a Play Store release.

## Speech recognition and privacy

- The app requests `RECORD_AUDIO` only when the user taps **Start listening**.
- Recognition is configured with the `en-US` locale. The installed Android recognition service determines actual language support and recognition quality.
- Android's platform `SpeechRecognizer` may stream audio to a remote recognition service. Offline recognition is not guaranteed and depends on the device/provider and downloaded language packs.
- LiveCaption itself does not save microphone recordings or a transcript history.
- The app is designed for foreground use. It pauses when the app leaves the foreground; it does not promise lock-screen listening or an overlay above other apps.
- Partial results are provider-dependent; some devices may update captions less frequently or only return final results.
- Android documents SpeechRecognizer as unsuitable for unrestricted continuous recognition because it can consume significant battery and bandwidth. This prototype restarts short recognition sessions after each result; long-session behavior must be tested on the OPPO phone.
- ColorOS battery optimization may stop microphone activity when the app is backgrounded. LiveCaption intentionally pauses when the activity leaves the foreground.

## HCI study

See [docs/HCI-EXPERIMENT.md](docs/HCI-EXPERIMENT.md) for the proposed comparison of one-line and two-line captions, procedure, measures, and reporting template.

## Project structure

```
app/
  src/main/
    AndroidManifest.xml
    java/com/wany864/livecaption/
      MainActivity.kt
      EnglishSpeechController.kt
docs/
  HCI-EXPERIMENT.md
.github/workflows/
  android-build.yml
```

## License

MIT. See [LICENSE](LICENSE).
