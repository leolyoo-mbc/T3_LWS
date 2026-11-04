# Setup validation — 2026-11-04

Current status: the user accepted the revised situation pronunciation. The replacement shout is user-selected; the other four cues had no issue reported during the six-cue review. T004 is now complete, with this review scope recorded per cue in VoiceManifest.json. T001–T004 are complete. Historical pending statements below describe earlier checkpoints. Foundation progress and the current scene blocker are recorded in Foundation.md.

Free-plan clarification: user confirmed the supplied ElevenLabs audio was generated on the free plan. Official non-commercial-use and publication-title attribution requirements were checked and recorded in ThirdPartyNotices.md and the manifest. T003's source/conditions investigation is complete again; required release credits/title are pending actual delivery work. T004 still awaits remaining narration listening review.

Latest update: user-provided ElevenLabs MP3 replaces the rejected Supertonic shout in VoiceManifest.json. Decoding, six-cue hash/text checks and Unity import passed; current shout is 1.0710204 s, mono, 44100 Hz, peak 0.5920 with no clipping. T003 is reopened only to verify the newly supplied source's distribution terms. T004 remains incomplete for remaining listening validation. Earlier completion and generated-shout measurements below are historical.

## Completed

- T001: plan folders created through connected Unity Editor refresh; Domain assembly has no Unity engine references; Runtime explicitly references Domain, XRI, CoreUtils, OpenXR, Input System, TMP and uGUI. Editor import succeeded and script recompile status reported completed, failed=false, errors=[], compilationFailed=false.
- T002: connected Editor and repository settings inspected; Android module, toolchain, IL2CPP/ARM64, SDK 34, Meta Quest Support and Touch profile recorded in Environment.md. No package upgrade or project setting replacement performed.
- T003: publisher licenses inspected, selected sources and redistribution conditions recorded in ThirdPartyNotices.md. Original primitive geometry and generated alarm are the selected production paths, not claims that those later assets already exist. Noto Sans KR and full OFL downloaded; model license retained. Generated speech disclosure is required in later UI.
- Ignore configuration: existing Unity .gitignore retained; added missing bin, OS/editor temporary patterns. No Docker, npm publishing, Terraform, Helm or JS lint toolchain applies to this Unity feature.

## T004 partial — listening review pending

Update after user feedback: the original situation and shout failed listening review (duplicated pronunciation / insufficient shouted delivery). Both have been replaced by revised candidates as detailed in VoiceGeneration.md. The table below records the initial generation, not the current two files. Revised situation is 14.80449 s / peak 0.2882; revised shout is 1.62839 s / peak 0.88. Current manifest hashes, script equality, no clipping and Unity imports were revalidated. Listening approval remains pending, so T004 has not been marked complete.

Six PCM_16 mono 44100 Hz WAV cues generated. All six imported as AudioClip in the connected Unity Editor; font imported as Font. VoiceManifest.json records exact text, source, model revision, voice, duration and SHA-256.

| Cue | Duration (seconds) | Peak amplitude | Samples at or above 0.999 |
|---|---:|---:|---:|
| situation | 13.81 | 0.3044 | 0 |
| next | 21.37 | 0.3788 | 0 |
| bell | 23.01 | 0.3646 | 0 |
| completion | 24.72 | 0.3269 | 0 |
| feedback | 8.29 | 0.4450 | 0 |
| shout | 1.46 | 0.3911 | 0 |

Mechanical verification: 6/6 exact approved script matches; 6/6 valid hashes, duration, finite/nonzero samples and no clipping. Font reports no missing characters among the 132 distinct Hangul syllables used by the manifest. This is a source-font coverage check, not a rendered TMP subtitle validation. git diff --check passed.

The available tools cannot directly listen to sound. Human listening review was requested for pronunciation and calm narration. No listening result is assumed; VoiceManifest.listeningReview remains pending and T004 remains unchecked. Review all six local files under Audio/Voice before marking T004 complete. Later stages have not been started because Setup is a prerequisite.

No scene, prefab, alarm sound, Android build, Quest 2 acceptance, performance measurement, package export or clean import is claimed. Those tasks remain unchecked. No extensions.yml existed when implementation pre-hooks were checked.
