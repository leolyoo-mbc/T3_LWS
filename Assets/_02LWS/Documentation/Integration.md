## Current wall alarm revision — 2026-11-05

Wall-mounted Blender model, circular push guard, dedicated 3D gong emitter, CC0 actual electric-bell recording and approach anchor applied. Editor 162 checks passed; actual VR appearance/reach/listening, Quest performance, new APK and receiving-project import remain unverified. See [WallAlarm validation](Validation/WallAlarm.md). Editable .blend is in Art/Models/FireAlarm/Source~ (workspace/Git only); unitypackage uses FBX/native meshes and needs no Blender. Existing external XRI 3.5.1 Starter Assets prerequisites remain.

# Fire notification integration

This is a non-commercial coursework content package. Display/publication title: `화재 사실 알리기 elevenlabs.io`. Keep ThirdPartyNotices.md and its ElevenLabs title-attribution requirement with any submission or redistribution. The supplied shout is a local ElevenLabs free-plan output; commercial rights are not included. Do not imply endorsement by the voice provider.

## Prerequisites

Use the recorded Unity 6000.3.26f1 setup: URP 17.3.0, XRI 3.5.1, OpenXR 1.17.1, Meta OpenXR 2.5.1, XR Management 4.6.1, CoreUtils 2.6.0, Input System 1.20.0, uGUI/TMP 2.0.0. SDK and package compatibility must be rechecked when the designated integration project is known. Do not upgrade its packages or overwrite ProjectSettings automatically.

Install Android Build Support including SDK, NDK and OpenJDK. Existing project uses IL2CPP, ARM64 and minimum/target SDK 34. These are observed settings, not a successful Quest installation result. Use Input System (`activeInputHandler=1`). Android XR loader is OpenXR. In its Android features enable Meta Quest Support with Quest 2 target and Oculus Touch Controller Profile. The old Oculus Quest Support feature is disabled in this version; do not mistake it for the newer enabled Meta Quest Support. Existing Android XR Support coexistence still needs a device build check.

The provided materials use URP/Lit and URP/Unlit; the custom static Noto Sans KR TMP atlas/material are local. The target must have its supported TMP runtime/essential resources configured. The font source and OFL are included; do not replace Korean glyph coverage with a Latin-only default font. No Python, ONNX model, authoring cache or TTS service is a player dependency.

The TMP mobile SDF shader and its properties include are internalized in Art/Shaders; material shader name is `LWS/Text/Korean SDF`. Their Unity Companion License and copyright notice are retained under Documentation/Licenses. Engine shader includes remain Unity prerequisites. The earlier two-scene dependency scans found zero references to project assets outside `_02LWS` after internalization.

## Current XR sample prerequisite — 2026-11-05

The existing LwsXRStarterRig depends on XRI 3.5.1 Starter Assets under Assets/Samples/XR Interaction Toolkit/3.5.1/Starter Assets. Import this matching sample using Package Manager before opening the scene in a receiving project; preserve its path/GUIDs. This sample is not included in the _02LWS export. The current saved-scene scan finds 40 outside-folder paths inherited from that rig; the earlier zero-external-dependency records predate it. A self-contained submission requires later internalization/remapping or the same verified prerequisites in the designated receiving project. The single-scene revision preserves the user's current rig.

## Scenes and launch — single scene revision 2026-11-05

1. Register only `Assets/_02LWS/Scenes/FireNotification.unity` as this feature's build entry. If a receiving Android Build Profile overrides the global list, apply the same entry there. The current project has no active custom profile. Its shared list starts with FireNotification and retains the unrelated SampleScene.
2. FireNotification starts in MenuSpace at the origin; selecting content 1 moves the shared XR player to ContentStart in FireNotificationSpace, positioned 40 metres along world X. These positions are implementation choices. Separate spaces are mutually exclusive: the unselected root is inactive, including its renderers, inputs, narration, alarm and NPC animation.
3. SharedSystems is a sibling of MenuSpace/FireNotificationSpace and remains active throughout. It contains the existing XR rig (camera/listener/controller inputs/interaction manager), LwsEventSystem, SpaceNavigation, SharedDirectionalLight and SharedLifecycle. No duplicated runtime manager or new framework is introduced. The existing base Directional Light keeps its world rotation and intensity (1.2) and illuminates both spaces; the fire Point Light stays inside the content space. The scene Corridor prefab instance was unpacked so its light could move; the Corridor source prefab is unchanged. SpaceNavigation and one XR Origin, AudioListener and EventSystem/XRUIInputModule remain active throughout. No runtime scene load occurs. The navigator aligns the tracked camera's heading and horizontal position to each anchor while preserving head height. Do not duplicate the rig when integrating with an existing host; remap and revalidate it deliberately.
4. FireNotificationController.startOnSceneEntry is false in the combined scene. After activation, ContentSceneNavigator calls BeginExperience to create a fresh memory-only session and start the first guidance. Menu return closes the session, clears the snapshot and cancels presentation before disabling the space. Re-entry starts over; app interruption within the same active experience retains the existing stage-restart rule.
5. The old ContentMenu.unity asset is retained only as a migration source, outside the build and exported package. It is not a supported launch scene after this revision. Do not register it or use its former scene-loading navigator. Candidate topics 2–8 are still unimplemented: add a separate space and routing only after their educational flow is approved; do not expose placeholder topics.

## Controller input and layers

`Settings/LwsInputActions.inputactions` owns Head, Left and Right maps. Local references in Settings/InputReferences bind pose, tracking state, UI Press to triggerPressed, and teleport selection to the secondary button (B/Y). LwsXRInputs enables/disables this asset with the scene rig. Each tracked controller has an XRRayInteractor for UI and a separate XRPokeInteractor attached to visible index-finger geometry. There is no hand-tracking requirement or continuous movement provider.

All interactions currently use the project's default interaction layer (bit 0). Do not move or replace XRI's global interaction-layer settings. On import map this meaning to an enabled layer shared by the supplied interactors, alarm and teleport surface. If the integration project has remapped bit 0, adjust the prefab masks by meaning, not by copying arbitrary layer indexes. Poke-only hover/select filters also check XRPokeInteractor type, so layer overlap cannot turn a ray/grab into a successful bell push.

UI uses world-space Canvas/TrackedDeviceGraphicRaycaster and ray trigger presses. FreshPressButton rejects hover-only and a trigger held across panel activation. Teleport uses the ray plus secondary button on TeleportSurface. Button pressing is independent of trigger: cross the fixed alarm surface inward along its local +Z axis to at least 25 mm. Surface radius, contact/release distances, cap pose, hand fingertip offset and alarm height are serialized for Quest calibration. Release both hands from the button region after a resumed wait before a new push can arm.

## Lifecycle and local content

Nine stages are defined in ExperienceStage. Execution IDs invalidate old completions; transition commands commit in LateUpdate after immediate menu/suspension handling. Suspension combines application pause/focus and, after an XR session is observed, OpenXR session focus/presence/device activity. ExperienceLifecycle now runs on SharedSystems/SharedLifecycle, including while the menu is active. The controller subscribes only during an active experience and checks the current blocked state when beginning a new session. All blocking factors must clear before one stage re-entry. The nonserialized DebugEnter/DebugIsolateLifecycle hooks are Editor-only and are not build UI.

All six approved voice cues, subtitles, alarm and Korean font are local. GuidanceCatalog data changes do not require editing progression code. VoiceManifest identifies the actual selected MP3, hashes, cue text and generation history. `Audio/Voice/shout.wav`, if retained as an authoring candidate, is superseded and is never selected by the catalog. Review references rather than replacing the accepted ElevenLabs clip by filename convention.

## Package boundary and validation

Export Assets/_02LWS after verifying all project-asset dependencies stay under that directory; SDK package references are separate prerequisites. Preserve .meta GUIDs. Package Manager manifests, ProjectSettings (including OpenXR/Input/URP/scene lists), device configuration and Android tools are not included in a unitypackage. Supply this document separately with the package and apply prerequisites in the receiving project.

Existing Editor verification records are under Documentation/Validation. They do not establish Quest 2 standalone execution, first-launch offline behavior, performance, or clean-project import acceptance. Device/build/OS identifiers and actual results must be added to those records. The final submission integration project is still unspecified; even a clean-project preliminary import does not replace its later compatibility check.

## Edit-mode visibility

Both MenuSpace and FireNotificationSpace are saved active for Scene view authoring. Play startup disables the content root and starts in the menu without a session. Only selection begins the experience. Exiting Play restores both spaces. This is not an all-spaces-visible runtime mode.

## design.md shared UI revision — 2026-11-05

The current entry scene includes shared menu/settings/confirmation/result views and styled existing content UI, backed by UI/Theme/DisasterTheme.asset and UI/Fonts/DisasterKorean.asset. Preserve these assets, their original font and OFL license, and .meta GUIDs. UI assets are internal to _02LWS for standalone unitypackage delivery rather than the design document's proposed external SharedUI folder. No package/engine upgrade is required.

SharedSystems/SharedUIController owns transient settings and modal state; project callbacks control content start/restart/return. Restart is a new session. Internal pause resumes the same playback position; lifecycle suspension still restarts the current stage, and only after an open internal menu closes. Keep MenuTeleportationProvider wired to teleport areas; it rejects menu-time requests and clears queued requests. Existing bell input remains poke/depth based. Do not replace it with UI trigger selection.

Desktop testing uses XR inactive, XRUIInputModule mouse fallback and ordinary GraphicRaycasters. Actual XR uses tracked-device raycasters, disabling desktop raycasters; the same FreshPressButton pointer policy is used in Editor and player. One EventSystem and XR UI input module remain. Default settings are voice/effects 100%, subtitles on and text 100%; enlarged text is 150%, retained only during the same process. Absent background music has no settings row.

Shared UI view Prefabs have no cross-scene callback targets; the authored FireNotification scene supplies bindings. There is no final integration menu or persistent save contract yet. On integration, retain one source for shared assets and adapt navigation/settings ownership instead of introducing another rig or EventSystem. Original design.md is the v0.1.1 reference; proposed sizes are this project's initial choices, not newly claimed team consensus.

The current package includes this revision; the old APK does not. See Validation/SharedUI.md for exact Editor/device limits. Quest 2 and receiving-project validation remain open.

UI artifact note: UI/Fonts/DisasterKorean uses the existing internal Art/Shaders/LwsKoreanSDF.shader. The remaining 40 external scene dependencies are the existing XRI 3.5.1 Starter Assets sample; the UI revision adds no new outside-font/shader dependency. Scene wiring audit is Validation/SharedUI-SceneAudit.json.

Teardown follow-up: stopping a session while a modal is open no longer dereferences a destroyed controller. UI disable closes the modal and restores surviving input objects; re-enable restores the static input owner. Seven focused teardown/re-enable checks pass. The package must include this final fix; see SharedUI.md.

## 2026-11-05 공간 UI 추종

Meta 지침 기반 완만한 추종과 텔레포트 완료 후 재배치를 반영했다. 초기값과 공식 근거, Editor 13개 추종/입력 및 111개 회귀 결과는 Validation/ComfortFollow.md를 참고한다. 실기기 가독성/불편감/지속 입력 검증은 미완료이며 최신 APK는 없다. 패키지를 다시 Export한 결과는 Artifacts/SharedUIPackageAudit.json 기준이다.

Teleport correction: BellApproach and the owned TeleportSurface prefab use the existing dedicated Teleport interaction layer (current bit 31), excluding ordinary grip selection. On import, match this semantic layer with both Teleport Interactors. New full ray-selection/release regression passes 6/6; see Validation/WallAlarm-RayInput.txt.
