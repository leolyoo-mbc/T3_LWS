# Smoke posture acceptance

Core implementation and Editor checks complete; Android development APK built successfully (0 errors); source/archive hash verified. No Quest, learning-effect, final voice-rights or receiving-project PASS claimed.

| Procedure | Requirements | Result |
|---|---|---|
| Q1 deterministic input | FR006/007, SC002 | Editor probes 84/84 PASS |
| Q2 smoke/stationary | FR002/003/005/008, SC003 | Editor integration PASS; Quest pending |
| Q3 narration/readability | FR004/010, SC003 | Editor captions/layout PASS; voice listening/rights and Quest pending |
| Q4 physical recognition/latency | SC002/004 | Pending Quest |
| Q5 interruptions | FR009, SC002/006 | Editor injected PASS; actual device interruption pending |
| Q6 repeated loop | FR011, SC001/006 | 3 loops + four-stage exit, integration 63/63 PASS |
| Q7 content1/device/offline | FR001/002 | Existing UI 111/111, physical poke 25/25 PASS; Quest/offline pending |
| Q8 learning/performance | SC005 | Pending people/device |
| Q9 package/dependencies/privacy | FR012/013 | Outside-folder project dependencies 0; final voice rights/package/Import pending |

Each report must record source state, Unity/package versions, scene, device/input, expected/actual result and evidence. Existing content1 results do not certify new smoke content.
Review found and fixed effects audio-root coverage, invalid-tuning hold carryover, nested-reference recovery/loading and menu lifecycle locomotion. Focused review/UI probes 17/17 PASS, domain probes expanded to 84/84. Prior final shared-UI and physical-poke suites rerun after fixes. No minor findings deferred; remaining items are explicit environment/asset acceptance requirements, not assumed successes.

Historical 2026-11-10 task count: 37/45. At that time pending: T014 narration listening/redistribution; T039/T040 physical Quest input/interruption/offline; T041 Quest performance; T042 five-person education evaluation; T043 final asset-rights clearance; T044 full receiving-project runtime/submission acceptance; T045 final acceptance after remaining checks. No attached ADB devices. Development APK: Assets/_02LWS/Artifacts~/SmokePosture/SmokePosture-development.apk. Latest SHA256 2A7C172959F821DA71CCEA05292DF422B848E242541E50A3A3E96F335EB58F10.

Receiving project preliminary check passed after same-version local package setup and TMP Essential Resources import: roots=4, contents=2, missing scripts=0, compile/reference and domain completion=true. Full receiver runtime and final voice rights remain pending; see SmokePostureImport.md.

## Earlier recovery acceptance — 2026-11-12

T046 system recovery implemented and Editor verified: 42/42 lifecycle integration and 84/84 domain regression. Silent runtime MP4: `Artifacts/Validation/VisualEvidence/20261112-000952-smoke-system-recovery/execution.mp4`; accompanying verification.md defines its scope and timing limitations. Current F1 six-cue listening is explicitly approved by the user; actual hashes/meta/catalog/subtitle timing and pinned license/disclosure inspection passed. Heami provenance and the 37/45 summary above are historical.

Current tasks: 55 checkbox entries / 53 unique IDs / 43 checked / 12 unchecked. T014, F1 T041, T046 and T047 newly complete. T051-A latest Android development APK succeeded (0 errors, 1 diagnostics-symbol warning); T051 remains unchecked until its post-validation Export/Import step is also complete. T043 final dependency gate, T048 Quest, T049 performance, T050 five-person learning, T051-B receiver Import and T052/T045 final aggregate acceptance remain pending. No attached ADB devices, actual participant or final receiver result was invented. This is an incremental status update, not final T052 completion.

## Current shared UI acceptance — 2026-11-12

Single shared Canvas/narrator implemented for both contents. T054~T060 complete; 47 integration+84 posture domain+111 fire lifecycle+42 smoke recovery=284 Editor checks PASS. Saved/Play structure violations0, missing scripts0, external project assets0. Current task counts62 entries/60 unique IDs/50 checked/12 unchecked. Runtime MP4: `Artifacts/Validation/VisualEvidence/20261112-011720-shared-content-ui/execution.mp4`; procedure/limits in its verification.md. New SharedUI-development-20261112.apk built successfully,0 errors/1 diagnostics-symbol warning; exact hash and archive in SmokePostureBuild.md. Actual Quest, learning, performance and final package/receiver Import remain pending, so T045/T052 final acceptance stays unchecked.
