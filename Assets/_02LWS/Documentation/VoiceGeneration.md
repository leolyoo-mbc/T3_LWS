# Prerecorded speech generation

Current review status: the user accepted the revised situation pronunciation. The ElevenLabs shout is the user-selected replacement. No issues were reported for the other four cues in the six-cue review. T004 is complete; VoiceManifest.json records the distinction between explicit approval and no issue reported. Historical pending notes below are retained as the generation/review history.

## Current shout override — user supplied (2026-11-04)

Subsequent clarification: the user confirmed free-plan generation. General output conditions are now recorded in ThirdPartyNotices.md and the cue manifest: non-commercial use and title attribution on publication. The earlier statement below that the account plan/terms were unknown describes the initial import, before this clarification.

The user rejected the revised Supertonic shout and supplied `ElevenLabs_2026-11-04T04_14_37_Roger - Laid-Back, Casual, Resonant_pre_s50_sim75_v4.mp3`. The shout cue now references that original local file directly, without transcoding or gain edits. Roger/provider are identified from the user-provided filename; model, synthesis settings and account plan are not independently verified.

Decoded duration: 1.0710204 s, mono, 44100 Hz, peak 0.5920, no clipped samples. Unity imported the MP3 with matching duration and format; asset GUID `b26d75a926d117345aa212a0cd303208` retained. MP3 header-only duration inspection reported a different duration, so the manifest uses actual decoded sample length, confirmed by Unity. All six manifest hashes and approved text entries were checked again.

The source MP3 and the other five current cues are preserved. `shout.wav` is an unused superseded candidate. Existing generation tooling preserves the manifest-selected MP3 and cannot silently replace it with a newly synthesized shout. Delivery rights for the newly supplied source remain unverified; T004 also still awaits the remaining narration listening review. The generation descriptions below are history and defaults for locally synthesized material.

Generated 2026-11-04 from the approved `specs/001-fire-notification/content-script.md` without text edits. This is authoring-only processing. No Python, ONNX model, network connection or synthesis service is needed by the Unity player.

- Model: Supertone/supertonic-2, revision `75e6727618a02f323c720cba9478152d4bc16ca4`.
- Publisher reference helper: supertone-oss-archive/supertonic, revision `1e9799e964ea4c0dad7cde993b65c3c813a7b373`, `py/helper.py` (MIT).
- Local CPU inference: Python 3.13.15, onnxruntime 1.31.1, numpy 2.5.4, soundfile 0.14.0.
- Korean (`ko`), 16 denoising steps, speed 0.95, F1 narration/feedback and M1 player shout; numpy seeds 20261104 through 20261109 in cue order.
- PCM 16-bit mono WAV, source sample rate; file duration and SHA-256 are recorded in `Content/VoiceManifest.json`.
- Cue order: situation, next, bell, completion, feedback, shout. The first four are extracted from the four standalone quoted paragraphs in the approved script. Feedback and shout use the approved exact wording.
- Local generation helper/cache: `.utmp/fire-notification-voice/`, ignored by Git and excluded from submission. Publisher code/models are not copied into the delivered application.

## Validation boundary

### Revision after user listening feedback (2026-11-04)

User reported that shout sounded spoken rather than shouted and situation contained a duplicated-syllable pronunciation around 어떻게. These are listening failures; the original mechanical checks did not detect them.

- Situation candidate: same approved words and F1 voice/model 2, each of the four sentences synthesized separately, 24 steps, speed 0.95, seeds 202611140–202611143, 240 ms silence between sentences. Duration 14.80449 seconds. Segment times and exact input are retained in the manifest.
- Shout candidate: Supertonic 3 revision `aafc6e32416a594460b32413efc49d7fe4ce6d46`, M1, 24 steps, speed 1.05, seed 202611150, synthesis input `<scream>불이야!`; spoken script remains 불이야!. Peak normalized to 0.88 without clipping. Duration 1.62839 seconds. This is an expression-tag attempt, not verified shouted delivery. Check that the tag itself was not spoken and no unwanted nonverbal scream was added.
- The manifest's top-level model/steps/speed describe the initial defaults. Cue-level model/revision and generation fields override them for revised cues.
- Original two WAVs and two alternative shout candidates are retained in ignored `.utmp/fire-notification-voice/review-20261104/` for comparison; excluded from submission. The other four WAV hashes remain identical to the initial manifest.
- Generation entry point preserves existing cue metadata/hashes when rerun, so it cannot silently relabel these revised files as the original model/settings.

The six-cue mechanical verification passed again: approved text equality, hashes, duration, finite/nonzero samples, no clipping. Both revised clips imported in Unity. Acoustic suitability remains `revision-pending`; T004 is still unchecked.

File integrity, finite/nonzero samples, local imports and script equality can be checked mechanically. This does not prove pronunciation, intelligibility or calm delivery. Listening review is pending and T004 must remain unchecked until it is performed. Review all six cues, especially the short player shout and the quoted words within the next/bell narration. Record reviewer feedback before regenerating any cue.

If regenerating, preserve the model/code revisions and exact approved script; use the publisher helper's `load_text_to_speech`, `load_voice_style` and `tts(text, 'ko', style, 16, 0.95)` API. Trim to the returned duration and save with soundfile as PCM_16. Update manifest hashes and duration, then repeat listening and Unity import checks. Generated output must be disclosed in the menu/narration UI as described in ThirdPartyNotices.md.
