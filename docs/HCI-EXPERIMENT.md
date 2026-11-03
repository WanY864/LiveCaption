# HCI Experiment: How should live captions be presented?

## Research question

Does showing one line or two lines of the current English caption help users follow spoken information more comfortably?

## Hypothesis

Two-line captions may preserve more context and reduce truncation, while one-line captions may feel cleaner and reduce visual clutter. This is a hypothesis to test, not a predetermined result.

## Independent variable

Caption layout:
- **Condition A — One line:** the current recognition result is constrained to one line.
- **Condition B — Two lines:** the same current recognition result may occupy up to two lines.

The recognition engine, phone, font size, room, speaker, spoken script, and task remain the same.

## Participants

Recruit 5–8 volunteers if available. Participation is voluntary. Do not collect names or sensitive personal information. Ask permission before collecting questionnaire responses.

## Materials

- One Android phone running LiveCaption.
- A quiet room and the same English speaker/script for all trials.
- Two short English passages of similar length and difficulty.
- A short comprehension questionnaire (3–5 factual questions per passage).
- A 5-point rating form.

## Procedure

1. Explain the task and obtain consent.
2. Give the participant a short practice trial.
3. Randomize condition order (A→B or B→A) to reduce order effects.
4. Play/read passage 1 while the participant follows the live captions.
5. Ask the comprehension questions and collect ratings.
6. Repeat with passage 2 and the other layout.
7. Ask which layout they prefer and why.

## Measures

| Measure | Collection method |
|---|---|
| Comprehension | Number of correct answers |
| Perceived readability | 1–5 Likert scale |
| Perceived effort | 1–5 Likert scale |
| Preference | A / B / no preference |
| Recognition issues | Researcher notes: missed words, delays, truncation |

Do not report invented results. Record actual observations from the participants and device.

## Data table template

| Participant | Order | A comprehension | B comprehension | A readability | B readability | Preference |
|---|---|---:|---:|---:|---:|---|
| P1 | A→B | | | | | |
| P2 | B→A | | | | | |
| P3 | A→B | | | | | |

## Limitations to document

- Android SpeechRecognizer providers may differ between devices.
- Partial-result frequency and recognition quality are not guaranteed.
- Speech recognition may use network services; offline behavior depends on the device and language pack.
- A small convenience sample is exploratory and not statistically representative.

## Blog report structure

1. Motivation and accessibility context.
2. Design goals and screenshots.
3. Android implementation and speech-recognition pipeline.
4. Permission and privacy handling.
5. Experiment design and participant procedure.
6. Actual results (tables/plots) and participant comments.
7. Limitations and future improvements.
