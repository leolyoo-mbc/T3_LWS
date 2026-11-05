# Shared UI validation — design.md v0.1.1

Date: 2026-11-05. Host Unity 6000.3.26f1, XRI 3.5.1, uGUI/TMP. Source package revision: shared UI implementation T054–T061. Editor desktop mode uses Input System mouse with XR inactive; this is not physical Quest controller evidence.

## Implemented

Exact Noto Sans CJK KR Regular source, local dynamic TMP atlas, geometric nine-slice backgrounds/icons, central Theme, styled existing entry/choices/next/return/subtitles, shared pause/settings/confirmation/result screens. All owned resources remain under Assets/_02LWS. Settings preserve voice/effects volume, subtitle visibility and 100%/150% text during the current process. No progress persistence or extra content/score was added. ElevenLabs publication-title attribution and non-commercial notice remain visible on entry.

Internal pause preserves the same voice/subtitle/result playback position, NPC rotation/animation, particles and light time. System suspension still cancels and restarts the current stage once, deferred until the internal menu closes. Modal screens block underlying canvases, bell commands and teleport requests; queued teleport requests are canceled. XR head/hand tracking and menu UI are not paused. Same EventSystem/XR UI module is reused. Desktop GraphicRaycasters are enabled only with XR inactive; FreshPressButton uses the same XR-aware pointer policy in Editor and player.

## Executed evidence

SharedUI-PlayMode.txt: 111 checks, 0 failures. The 9 stages and wrong-answer feedback were exercised with 3-second internal pauses; execution ID, stage, subtitle elapsed/content, audio state, NPC rotation/speed, particles, blocked commands, blocked underlying canvases, same-execution continuation, overlapping synthetic system suspension, deferred restart, restart/exit cancel and confirm, settings retention, complete normal-command educational loop, and result-menu return were checked. Frames were observed advancing; no success was inferred solely from entering Play Mode.

SharedUI-Contrast.json: all eight primary-text/background state pairs exceed 4.5:1. Lowest measured sRGB pair is primary Hover at 5.40:1. This numeric calculation is not a headset readability measurement.

Every approved cue/segment was measured at 150% text size against the subtitle panel. Long next/bell/completion captions were split into 5/5/6 sentences at measured audio pauses (10 ms RMS windows, threshold 0.008). Original audio and approved full strings are unchanged; GuidanceCue.Validate verifies exact reconstruction and valid intervals. No enlarged segment overflows the panel. Situation remains 4 segments; feedback and shout remain one each. Fine listening alignment and actual Quest readability still need headset review.

Button hover colors and disabled/busy rejection were checked in Play Mode. Missing-reference/scene-input audits and package-source comparison are recorded by the final export audit. No project-authored EditMode/PlayMode test assemblies were found; the detailed scenario scripts are validation probes, not a claimed installed automated test suite.

## Device and artifact limits

ADB devices -l returned an empty device list on 2026-11-05. T060 and existing T037/T038/T041/T042 remain incomplete. No Quest standalone execution, actual controller tracking/poke geometry, physical glyph/button angle, headset removal/system-menu behavior, first-install offline run, GPU timing or fresh receiving-project import is certified by these Editor checks. No new APK was built in this change. The earlier APK predates this UI revision.

The reusable UI Prefabs are unbound views; scene instances use project-owned adapter callbacks. Final integration still needs its designated project, one shared rig/EventSystem, prerequisites in Integration.md, and settings/menu callback ownership. Scene/prefab YAML carries Unity-generated blank-field whitespace; source/Markdown whitespace checks are separate from serialized asset generation.

Final input run: SharedUI-Input.txt records 35 checks, 0 failures, using InputSystem.QueueStateEvent mouse down/up through actual EventSystem raycasts. It entered content, selected Continue, measured all common-panel text at 150%, rejected menu-time teleport requests and confirmed no stale movement after Continue. Every final enlarged subtitle segment fits; hover color and disabled/busy rejection also passed. PC standalone was source-reviewed but not launched. Independent read-only review identified the old non-Editor mouse rejection; it is now replaced by an active-XR condition, and the same branch was re-tested in Editor. The follow-up review found no remaining critical/important issue.

Final visual review: 1920×1080 camera captures of the menu and 150% settings were inspected. Settings button gaps are 16u, enlarged strings fit, and the panel uses the 20u sliced corner. Modal height is expanded to 960u for enlarged settings, rather than shrinking mandatory text. Captures are retained under Artifacts/SharedUI/ for handoff. Quest ergonomic limits remain unmeasured.

Console: zero compilation/runtime errors in the final run. Two XR audio-output warnings fell back to the default desktop driver; this is not evidence of Quest audio. Documentation imports also reported stale disk timestamps while files were being edited; explicit refresh before export is required and performed. Unity window focus was restored after a main-thread command delay; no Editor restart or unsaved-scene discard was used.

Final scene audit: SharedUI-SceneAudit.json records 23 valid bound buttons, zero missing scripts, one XR Origin/EventSystem/AudioListener, valid modal CanvasGroups and both spaces visible in edit mode. The generated font material was connected to the existing internal LWS/Text/Korean SDF shader so the UI adds no external TMP shader dependency. The remaining 40 external project dependencies are the pre-existing XRI 3.5.1 Starter Assets prerequisite; this package is not presented as self-contained without that sample.

## Teardown follow-up — 2026-11-05

Stopping Play Mode with a modal open exposed a destroyed-controller reference in SharedUIController.OnDisable after the original run. The final implementation checks the controller's Unity lifetime and enabled state before unpausing, closes the modal, safely restores remaining input objects and restores the static input owner on re-enable. The controller avoids accessing presentations when its experience is not started or its component is inactive.

SharedUI-Teardown.txt: 7 checks, 0 failures. UI disable releases pause and modal visibility, restores canvases without replacing the active run, re-enable restores the input owner and can pause again, and destroying the controller before disabling UI is safe. Console errors were zero during this targeted run and on stopping it. This follow-up supersedes the earlier stop-time exception; final package is re-exported after the fix. XR output-driver fallback warnings remain desktop-environment warnings.

Final follow-up verification: the 35-check mouse/layout run passed again after the teardown fix. Settings was then opened and Play Mode was stopped with the modal still open; console/runtime/compilation errors were zero. Both spaces were restored to their authored edit-mode state. The detailed 111-check progression run, 35-check input/layout run and 7-check teardown run are separate evidence sets, not a claimed physical-device test suite.
