# Third-party materials

Selection checked against publisher sources on 2026-11-04. No paid subscription or hosted synthesis is required. This file records technical selection within the approved free-materials scope.

Current character update: the user selected Umplix's Lowpoly Dummy Character, CC0, https://opengameart.org/content/lowpoly-dummy-character10-animations . It replaces the removed ithappy scene characters. Source evidence and download are recorded in Licenses/UmplixDummy-Source.txt. Only this model's license is CC0; the user's Mixamo clips retain their separate terms. Earlier Quaternius selection notes below are historical and do not identify the active bystander model.

Update: the user supplied an ElevenLabs Roger MP3 to replace the rejected Supertonic shout and confirmed it was generated on the free plan. The application uses this local MP3 without runtime synthesis. Official terms checked 2026-11-04 restrict free-plan output to non-commercial use and require attribution when published. For this coursework, record the non-commercial constraint with the asset; do not present the package as having unrestricted commercial rights. No paid service was purchased by the agent.

ElevenLabs publication guidance requires `elevenlabs.io` or `11.ai` in the published content title. Use a release/submission title such as `화재 사실 알리기 — 음성: elevenlabs.io`, and carry `불이야! 음성: ElevenLabs (elevenlabs.io), Roger / 무료 플랜 생성, 비상업적 이용` in the delivered credits/notices. Credits alone do not replace the stated title requirement. These are delivery obligations to apply when authoring the menu and exporting/sharing the package, not completed UI work.

Official sources:
- https://elevenlabs.io/docs/help-center/legal/can-i-publish-the-content-i-generate-on-the-platform
- https://elevenlabs.io/terms-of-use (non-EEA, updated 2026-10-09, section 1(c))

This records the general free-plan terms; it does not independently certify the supplied account's agreements or generation feature. The original filename is provenance rather than proof of a specific model or feature status.

Current shout source filename: `ElevenLabs_2026-11-04T04_14_37_Roger - Laid-Back, Casual, Resonant_pre_s50_sim75_v4.mp3`. Kept unchanged under Audio/Voice; VoiceManifest.json references it directly. The former `shout.wav` is superseded and must not be selected for the delivered cue. Earlier Supertonic 3 terms below describe the rejected candidate, not the current shout.

| Material | Source / license | App and unitypackage treatment |
|---|---|---|
| Corridor and alarm geometry | Original geometry authored in this project | Local project assets |
| Civilian model | Quaternius Man, author upload https://poly.pizza/m/HMnuH5geEG, CC0 1.0 | GLB converted to FBX; foot bones reparented preserving bind pose; humanoid reference pose corrected. Source and license references retained in Licenses/QuaterniusMan-Source.txt |
| Idle and talking animation | Quaternius Universal Animation Library Standard, https://quaternius.itch.io/universal-animation-library, CC0 | Two selected humanoid clips; archive license retained in Licenses/QuaterniusAnimations-CC0.txt |
| Flame and smoke textures | Kenney Particle Pack, https://kenney.nl/assets/particle-pack, CC0 | Selected textures used by local Unity particle systems; license retained in Licenses/KenneyParticles-CC0.txt |
| Alarm sound (current) | Dave Welsh, School Bell - Fire Bell.wav, Freesound 523317, CC0 1.0 | Actual electric-bell recording, mono excerpt and loop crossfade; source preview, edited WAV and provenance included. Replaces the historical synthetic waveform. |
| Noto Sans KR | Google Fonts, SIL Open Font License 1.1 | Embed and redistribute with copyright and OFL text; do not sell the font by itself or imply endorsement. Preserve font name for unmodified font |
| Prerecorded Korean speech | Supertone Supertonic 2, revision 75e6727618a02f323c720cba9478152d4bc16ca4, BigScience Open RAIL-M | Local generated WAV output only; model and inference libraries are not shipped. Section 6 claims no licensor rights in generated output, subject to the license's restrictions. Educational fire notification narration is the intended use |
| Local TMP mobile SDF shader and properties include | Unity uGUI/TMP 2.0.0 essentials, Unity Companion License | Copied into Art/Shaders for Unity-dependent application/package distribution. Shader name changed to LWS/Text/Korean SDF to avoid duplicate shader names; other source retained. Copyright notice and full license retained in Documentation/Licenses/UnityTextMeshProNotice.md and UnityCompanionLicense.txt |

The delivered menu now displays `화재 사실 알리기 elevenlabs.io`, AI-voice disclosure, voice-provider credit and non-commercial coursework note. Preserve the separate published submission/title requirement when sharing the package.

TMP source paths before internalization: `Assets/TextMesh Pro/Shaders/TMP_SDF-Mobile.shader` and `TMPro_Properties.cginc`, imported from the installed uGUI/TMP essentials. Source license notice: `com.unity.ugui` 2.0.0 LICENSE.md. Full terms checked at https://unity.com/legal/licenses/unity-companion-license on 2026-11-04. UnityCG.cginc/UnityUI.cginc are engine includes and remain an engine prerequisite, not copied project files.

## Publisher sources

The revised player shout candidate uses `supertone-oss-archive/supertonic-3`, revision `aafc6e32416a594460b32413efc49d7fe4ce6d46`, also under BigScience Open RAIL-M. Its output/disclosure restrictions were checked against the publisher license: https://huggingface.co/supertone-oss-archive/supertonic-3/raw/aafc6e32416a594460b32413efc49d7fe4ce6d46/LICENSE . Full terms are retained in `Documentation/Licenses/Supertonic3.txt`. Only generated audio is delivered. Listening approval remains pending.

- Font source: https://github.com/google/fonts/tree/main/ofl/notosanskr
- Font terms: https://github.com/google/fonts/blob/main/ofl/notosanskr/OFL.txt
- Speech model: https://huggingface.co/Supertone/supertonic-2/tree/75e6727618a02f323c720cba9478152d4bc16ca4
- Model terms, including output section 6 and Attachment A: https://huggingface.co/Supertone/supertonic-2/raw/75e6727618a02f323c720cba9478152d4bc16ca4/LICENSE
- Inference example: https://github.com/supertone-oss-archive/supertonic/tree/main/py (MIT source license, retained with generation tooling if copied)

Local inference has no service character quota or per-request charge. Initial authoring requires downloading the model and Python dependencies; the distributed Unity application must not do this. Neither model weights nor Python are runtime prerequisites.

Attachment A(e) requires an intelligible disclosure that generated content is machine generated. The menu and narration UI must display `이 체험의 음성은 AI 합성 음성입니다.` This is a delivery requirement of the selected model, not a new education step. The full model license is retained in `Documentation/Licenses/Supertonic2.txt`.

Do not label narration as recorded by a real person or endorsed by Supertone. Speech pronunciation and calm delivery still require listening validation before T004 can be marked complete. Unity/XRI sample content, if later copied, must have its separate license retained here before shipping.

## Common UI font — 2026-11-05

The design.md v0.1.1 UI uses the unmodified Noto Sans CJK KR Regular original, not a renamed NotoSansKR file. Source: https://github.com/notofonts/noto-cjk/blob/main/Sans/OTF/Korean/NotoSansCJKkr-Regular.otf . License: SIL OFL 1.1, https://github.com/notofonts/noto-cjk/blob/main/Sans/LICENSE . The original and license are included in UI/Fonts/; original SHA-256: 6BCB2A0703AA137E874FC2DFFA85F6C21BA9A67FA329E81B8C801663AF7E992A. OFL permits bundling and redistribution with software while retaining notices and license; the font must not be sold alone. Generated DisasterKorean TMP atlas/material remain under the font license. The original stays local for offline dynamic glyph additions, including Korean, digits and punctuation. The design/script preload contains 563 glyphs with no missing printable characters. Existing Art/Fonts assets are retained for legacy references. Geometric UI backgrounds/icons are original project-generated resources.

## Wall alarm model and recorded bell — 2026-11-05

Original geometry authored through Blender MCP in Blender 5.2.3 LTS; no manufacturer photos, logos, certification marks or downloaded meshes are embedded. Shape references: https://www.alllitelife.com/page.php?page=sub2_3_7 and the Ilshin catalogue linked in Validation/WallAlarm.md. Product dimensions are shape references, not a certification or installation-standard claim.

Editing source: Art/Models/FireAlarm/Source~/WallAlarm.blend, with the user's original Scene preserved and a separate LWS_WallAlarm scene. The Noto Sans CJK KR Regular font is packed for source editing under the existing OFL. Unity ignores Source~ deliberately; runtime/package uses WallAlarm.fbx and Unity-native meshes/materials. The .blend source is kept in Git/workspace and is not part of unitypackage. BuildFireAlarm.py recreates the source in a fresh Blender file. Existing same-named equipment is guarded against overwriting.

Bell recording: “School Bell - Fire Bell.wav” by Dave Welsh, uploaded 2020-06-21, https://freesound.org/people/Dave%20Welsh/sounds/523317/ . Publisher labels it CC0 1.0, https://creativecommons.org/publicdomain/zero/1.0/ . Source description: solenoid striker Edwards Adaptabel bell, recorded using NTG4+ / Sony PCM-D50. This is a real electric bell, not a claim of the same Korean motor-bell model.

Source-ElectricBell.mp3 is the public high-quality 48 kHz MP3 preview from https://cdn.freesound.org/previews/523/523317_1985044-hq.mp3 ; it is not the original 96 kHz/24-bit WAV master. CorridorAlarm.wav uses 56.0–64.1 s, stereo to mono, mean removal, 100 ms circular crossfade and peak normalization to 0.5 (-6.02 dBFS). Output: 48 kHz, mono, PCM16, approximately 8 seconds. No pitch change or synthetic tones. Source and output hashes are in Audio/Alarm/RecordingInfo.json. The prior WAV .meta GUID is preserved. ProcessAlarmAudio.py reproduces the edit using Blender aud/numpy.

CC0 allows copying, editing and redistribution, including the source clip within unitypackage. A provenance copy is retained in Licenses/DaveWelsh-Bell-CC0.txt. Listening/comfort acceptance on headset is pending; real device SPL ratings are not headset volume targets.
