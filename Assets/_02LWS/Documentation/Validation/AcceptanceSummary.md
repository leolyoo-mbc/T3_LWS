## Shared-system revision — 2026-11-05

Current saved scene keeps the main Directional Light, XR/input/navigation and application-interruption sensing under always-active SharedSystems. 90 shared-system checks and 149 single-scene regression checks passed; current menu/content screenshots inspected and console errors 0. See SharedSystems.md. Package regenerated after this revision; source/archive comparison and SHA-256 are outside the package in Artifacts. The existing Starter Assets prerequisite, pending Quest/Android/performance/import limits still apply.

## Current single-scene status — 2026-11-05

149 Editor Play Mode checks PASS, zero console errors; menu/content captures inspected. One runtime scene, exclusive spaces, persistent rig, fresh sessions, all nine-stage menu returns and the full authored-duration command sequence validated. See [SingleScene.md](SingleScene.md) for procedures and limits. Current rig requires the XRI 3.5.1 Starter Assets sample (40 outside-folder dependency paths); current package is not self-contained. This differs from the historical dependency/import records below. Revised package export excludes the old ContentMenu source scene. Revised APK, receiving-project import and Quest 2 acceptance remain pending. Final package hash is maintained outside the archive in Artifacts/SHA256SUMS.txt.

> **2026-11-05 single-scene revision:** The records below describe the former two-scene version. They do not validate the new spatial navigation or a current Android APK. See SingleScene.md for the current Editor checks. Quest 2, performance and clean-project import of the revised package remain pending. The previously generated APK is stale for this revision.

# Acceptance summary — implementation complete, device acceptance pending

2026-11-04. This record distinguishes working Editor implementation from submission acceptance. Do not label the whole 42-task feature complete until the remaining device/import/build evidence is present in tasks.md.

## Current evidence

| Area | Actual evidence |
|---|---|
| State/data foundation | 57 edit-mode domain/asset checks reported PASS; compilation completed without error |
| US1 | 15 Editor Play Mode checks PASS: narration, choices, feedback, shout, no-input/hover/repeat, suspension |
| US2 | 17 Editor Play Mode checks PASS: independent bell guidance, actual XRI poke components/physics under programmatic motion, depth/type/rearm/cancel |
| US3 | 9 Editor Play Mode checks PASS: next wait, no auto advance, baseline restore, late completion and restoration suspension |
| US4 normal loop | 14 Editor Play Mode checks PASS using actual narration durations, scene changes and programmatic UI/poke motion |
| US4 return matrix | 49 Editor Play Mode checks PASS across nine stages plus feedback, duplicate navigation, session reset and rig/listener/EventSystem counts |
| Independent review | Read-only reviewer found no confirmed Critical/Important defects; device/comfort/audio/integration boundaries retained |
| Local content | Six cue texts/hashes/durations and finite unclipped samples verified; accepted supplied MP3 selected by catalog; Korean font/subtitles local |
| Asset ownership | Both scene dependency scans report zero project assets outside Assets/_02LWS; local TMP shader error check false |
| Existing test suite | Unity Test Framework enumeration found 0 tests; no existing suite was available to run. The authoring verification commands above are distinct evidence |
| Quest 2 | No devices connected; user confirmed connection is currently unavailable. No installed/standalone/offline or performance claim |
| Visual correction | CC0 clothed people/idle animation and flame/smoke textures applied; humanoid reference pose corrected; idle/reaction/freeze/restore checks and normal 14-check loop passed again. Repeated separate-project import audit passed. See VisualReplacement.md |

## Requirements and success criteria

FR-001–017 and the command/UI-adapter portion of FR-021 have Editor evidence in US1–US4. FR-018 requires the actual installed Quest build. FR-019 has synthetic lifecycle evidence but still needs real headset removal/system-menu behavior. FR-020 has local-asset/no-runtime-fetch inspection but still needs a fresh installed first launch with internet blocked. Actual controller tracking/ergonomics for FR-006/011/021 remain device checks.

SC-001–005 have Editor loop/wait/repetition evidence. SC-006–008 are not satisfied by those checks and remain pending actual Quest 2 standalone, lifecycle and first-offline acceptance.

## Remaining handoff work

Android build completed successfully with zero BuildReport errors/warnings; artifact details and SHA-256 are in AndroidBuild.md. Final content/scene audit passed with six cues, two choices, five bound buttons, thirteen text components, zero external project dependencies and zero missing scripts (ContentAudit.md).

Separate Unity 6000.3.26f1 project import and edit-mode audit passed: two scenes, no missing scripts, no outside project dependencies, valid local references and rig counts. ExportImport.md records prerequisites and exact validation limits. User-confirmed unavailable device access leaves T037, T038, the device-dependent portion of T041 and final T042 acceptance incomplete. Prepared deliverables are `Artifacts/FireNotification.apk` and `Artifacts/FireNotification.unitypackage`; final hashes are in `Artifacts/SHA256SUMS.txt`.

Record final Android artifact and package paths/hashes and clean-import findings when generated. T037/T038 remain unchecked; T041 remains incomplete until its newly built Quest loop and offline test are performed, even if the source package imports cleanly. T042 remains incomplete until required final acceptance evidence is complete. The later designated submission integration project must be revalidated separately.

Follow Integration.md and ThirdPartyNotices.md, including the supplied ElevenLabs free-plan non-commercial use and publication-title attribution. No paid plan, real-time TTS, microphone or downloaded player content was introduced.

## Current shared UI revision — 2026-11-05

This section supersedes earlier UI/artifact counts for the current source. design.md v0.1.1 styles, font, pause/settings/confirm/result UI are implemented under _02LWS. SharedUI.md and its raw result files identify executed evidence and limitations; historical validation sections above describe older revisions. The existing APK predates the UI revision. T060 and T037/T038/T041/T042 remain pending actual device/receiving-project evidence. Do not interpret Editor synthetic suspension or Input System mouse results as Quest standalone verification.

Current teardown fix verified: 7 focused teardown checks and repeated 35-check input/layout run pass. Stopping Play Mode with settings open produces no error. The final unitypackage is re-exported after this change; the older APK remains unchanged.


2026-11-05 공간 UI 추종 추가: Validation/ComfortFollow.md 및 ComfortFollow-Editor/Input/Regression.txt가 최신 증거다. Editor 추종/입력 13개 및 기존 회귀 111개 통과. 실기기 T066/T060과 기존 기기/Import 미완료 유지. 최신 unitypackage는 재Export 후 Artifacts/SharedUIPackageAudit.json으로 원본/meta 일치를 확인한다. APK는 이전 버전이다.

