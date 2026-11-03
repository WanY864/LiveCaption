# HCI Mobile Lab: LiveCaption Sticky Board

## Project concept

LiveCaption is a Unity Android application with two modes. The primary mode is an offline sticky-note board designed for direct touch interaction. The original English live-caption interface and Android SpeechRecognizer bridge remain available as an optional mode for devices with a compatible recognition service.

This design keeps the main interaction useful even when the phone does not provide speech recognition.

## Interaction design

### Sticky Board
- Tap **+ NOTE** to create a note.
- Tap the note body to enter or edit text with the mobile keyboard.
- Drag the note header to move the note around the board.
- Tap **COLOR** to cycle through pastel note colors.
- Tap a sticker tool (star, heart, sparkle, check mark) to add a decorative sticker.
- Drag stickers to position them.
- Notes, note colors, note positions, and sticker positions are saved locally and restored on relaunch.

### Optional Live Captions
- The Captions page retains the English-only Android speech-recognition interface.
- It requests microphone permission only when the user starts listening.
- Speech recognition depends on an installed Android recognition service and may use network processing.
- If the device has no compatible recognition service, users can return to Notes; the note-taking feature is independent.

## Research questions

1. Can users create and edit a note without instruction?
2. Can users understand that the note header is a drag handle?
3. Can users add and reposition a decorative sticker?
4. Does local persistence make the board feel reliable across app restarts?
5. Is the optional captions feature discoverable without distracting from the main note-taking task?

## Proposed participants and materials

Recruit 5–8 volunteers if available. Participation is voluntary. Do not collect names or sensitive personal information.

Materials:
- One Android phone running LiveCaption.
- Three short note-taking tasks (for example: add a reminder, make a short shopping list, and place a star sticker beside an important note).
- A short task-completion checklist.
- A 5-point perceived ease-of-use and satisfaction questionnaire.

## Procedure

1. Explain the prototype and obtain consent.
2. Ask the participant to create a note containing a short reminder.
3. Ask them to edit the note and change its color.
4. Ask them to move the note to another area of the board.
5. Ask them to add a sticker and reposition it.
6. Close and relaunch the app, then ask whether the note and sticker are still present.
7. Ask the participant to rate ease of use and describe any confusing interactions.
8. If testing on a device with a compatible speech-recognition service, optionally test the Captions page separately. Do not make this a prerequisite for the sticky-board study.

## Measures

| Measure | Collection method |
|---|---|
| Task completion | Completed / not completed for each task |
| Time on task | Stopwatch, seconds |
| Interaction errors | Mis-taps, failed drags, accidental deletion |
| Learnability | Whether the task was completed without help |
| Perceived ease of use | 1–5 rating |
| Satisfaction | 1–5 rating |
| Persistence | Whether notes and stickers survive relaunch |
| Participant comments | Short anonymized notes |

## Data table template

| Participant | Create note | Edit text | Change color | Move note | Add/move sticker | Persistence | Ease (1–5) |
|---|---|---|---|---|---|---|---:|
| P1 | | | | | | | |
| P2 | | | | | | | |
| P3 | | | | | | | |

## Limitations

- The prototype uses Unity's legacy UGUI InputField; keyboard appearance and text editing should be tested on the target Android device.
- Dragging and layout should be evaluated across screen sizes and aspect ratios.
- Notes are stored locally on the device; there is no cloud backup or cross-device synchronization.
- The optional speech feature depends on the Android device's installed recognition provider. Speech recognition availability and partial-result behavior are not guaranteed.
- A small convenience sample is exploratory and not statistically representative.

## Report structure

1. Motivation and target users.
2. Mobile design and screenshots.
3. Unity implementation and touch interactions.
4. Local data persistence and privacy.
5. User study procedure and participant tasks.
6. Actual results and observations.
7. Limitations and future improvements.
