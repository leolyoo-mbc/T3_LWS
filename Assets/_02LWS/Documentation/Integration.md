# Fire notification integration

This is a non-commercial coursework content package. Display/publication title: `화재 사실 알리기 elevenlabs.io`. Keep ThirdPartyNotices.md and its ElevenLabs title-attribution requirement with any submission or redistribution. The supplied shout is a local ElevenLabs free-plan output; commercial rights are not included. Do not imply endorsement by the voice provider.

## Prerequisites

Use the recorded Unity 6000.3.26f1 setup: URP 17.3.0, XRI 3.5.1, OpenXR 1.17.1, Meta OpenXR 2.5.1, XR Management 4.6.1, CoreUtils 2.6.0, Input System 1.20.0, uGUI/TMP 2.0.0. SDK and package compatibility must be rechecked when the designated integration project is known. Do not upgrade its packages or overwrite ProjectSettings automatically.

Install Android Build Support including SDK, NDK and OpenJDK. Existing project uses IL2CPP, ARM64 and minimum/target SDK 34. These are observed settings, not a successful Quest installation result. Use Input System (`activeInputHandler=1`). Android XR loader is OpenXR. In its Android features enable Meta Quest Support with Quest 2 target and Oculus Touch Controller Profile. The old Oculus Quest Support feature is disabled in this version; do not mistake it for the newer enabled Meta Quest Support. Existing Android XR Support coexistence still needs a device build check.

The provided materials use URP/Lit and URP/Unlit; the custom static Noto Sans KR TMP atlas/material are local. The target must have its supported TMP runtime/essential resources configured. The font source and OFL are included; do not replace Korean glyph coverage with a Latin-only default font. No Python, ONNX model, authoring cache or TTS service is a player dependency.

The TMP mobile SDF shader and its properties include are internalized in Art/Shaders; material shader name is `LWS/Text/Korean SDF`. Their Unity Companion License and copyright notice are retained under Documentation/Licenses. Engine shader includes remain Unity prerequisites. Both scene dependency scans found zero references to project assets outside `_02LWS` after internalization.

## Scenes and launch

1. Add `Assets/_02LWS/Scenes/ContentMenu.unity` and `Assets/_02LWS/Scenes/FireNotification.unity` to the Android Build Profile Scene List, enabled, with ContentMenu first. If the profile uses Override Global Scene List, update that profile's list as well.
2. The current shared Build Settings list has these two scenes first, followed by the pre-existing SampleScene. Existing unrelated scene entries were retained. The delivered app enters through ContentMenu. For an isolated submission build, include the two content scenes; other scenes are not required by this feature.

The inspected active target is Android and no custom Build Profile is active, so this project's Android build uses the shared scene list. The prepared Android build explicitly selects only the two feature scenes; it does not include SampleScene.
3. Each supplied scene owns one XR Origin, AudioListener and EventSystem/XRUIInputModule. Single scene loading replaces them. Do not add a persistent duplicate rig or another UI input module. If integrating with an existing rig, deliberately remap these references and repeat all input/lifecycle checks.
4. Direct opening of FireNotification starts the first guidance and a new memory-only session. Menu return disposes the current run and snapshot before loading ContentMenu; no saved resume or scores exist.

## Controller input and layers

`Settings/LwsInputActions.inputactions` owns Head, Left and Right maps. Local references in Settings/InputReferences bind pose, tracking state, UI Press to triggerPressed, and teleport selection to the secondary button (B/Y). LwsXRInputs enables/disables this asset with the scene rig. Each tracked controller has an XRRayInteractor for UI and a separate XRPokeInteractor attached to visible index-finger geometry. There is no hand-tracking requirement or continuous movement provider.

All interactions currently use the project's default interaction layer (bit 0). Do not move or replace XRI's global interaction-layer settings. On import map this meaning to an enabled layer shared by the supplied interactors, alarm and teleport surface. If the integration project has remapped bit 0, adjust the prefab masks by meaning, not by copying arbitrary layer indexes. Poke-only hover/select filters also check XRPokeInteractor type, so layer overlap cannot turn a ray/grab into a successful bell push.

UI uses world-space Canvas/TrackedDeviceGraphicRaycaster and ray trigger presses. FreshPressButton rejects hover-only and a trigger held across panel activation. Teleport uses the ray plus secondary button on TeleportSurface. Button pressing is independent of trigger: cross the fixed alarm surface inward along its local +Z axis to at least 25 mm. Surface radius, contact/release distances, cap pose, hand fingertip offset and alarm height are serialized for Quest calibration. Release both hands from the button region after a resumed wait before a new push can arm.

## Lifecycle and local content

Nine stages are defined in ExperienceStage. Execution IDs invalidate old completions; transition commands commit in LateUpdate after immediate menu/suspension handling. Suspension combines application pause/focus and, after an XR session is observed, OpenXR session focus/presence/device activity. All blocking factors must clear before one stage re-entry. The nonserialized DebugEnter/DebugIsolateLifecycle hooks are Editor-only and are not build UI.

All six approved voice cues, subtitles, alarm and Korean font are local. GuidanceCatalog data changes do not require editing progression code. VoiceManifest identifies the actual selected MP3, hashes, cue text and generation history. `Audio/Voice/shout.wav`, if retained as an authoring candidate, is superseded and is never selected by the catalog. Review references rather than replacing the accepted ElevenLabs clip by filename convention.

## Package boundary and validation

Export Assets/_02LWS after verifying all project-asset dependencies stay under that directory; SDK package references are separate prerequisites. Preserve .meta GUIDs. Package Manager manifests, ProjectSettings (including OpenXR/Input/URP/scene lists), device configuration and Android tools are not included in a unitypackage. Supply this document separately with the package and apply prerequisites in the receiving project.

Existing Editor verification records are under Documentation/Validation. They do not establish Quest 2 standalone execution, first-launch offline behavior, performance, or clean-project import acceptance. Device/build/OS identifiers and actual results must be added to those records. The final submission integration project is still unspecified; even a clean-project preliminary import does not replace its later compatibility check.
