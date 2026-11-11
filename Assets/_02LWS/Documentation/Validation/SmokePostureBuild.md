# Build validation — 2026-11-10

First attempt rejected before build: Unity prohibits build output inside Assets, including Artifacts~. Corrected staging output to Artifacts/SmokePosture/SmokePosture-development.apk; if successful copy into the task archive path afterward. No source or XR setting change required.

Final updated Android development build: Succeeded, 0 errors, 1 warning, 58.748 seconds (incremental build). APK bytes=130107831, SHA256=2A7C172959F821DA71CCEA05292DF422B848E242541E50A3A3E96F335EB58F10. Source and archive copies match. Warning concerns crash-diagnostics debug-symbol detail, not compilation. Device install/run not attempted: ADB returned no devices.

Build cleanup: in-memory PlayerSettings preloaded assets already restored to empty by Unity; persisted YAML still contained the two build-injected XR entries. Restored only that exact generated block to the observed original empty list. XR simulation files were restored by Unity.

## T051-A current development build — 2026-11-12 KST

Android development APK with T046 recovery correction and user-approved six F1 cues: Succeeded, 0 errors, 1 warning, 119.306 seconds. Actual APK size 130109057 bytes (BuildReport totalSizeBytes includes build outputs and is not the APK size). SHA256: 237DA822F24D81ED2C83DCEB89E6004FA2E2B3E16B996F479880BFA72BB61AF4.

Build output: Artifacts/SmokePosture/SmokePosture-development-20261112.apk
Archive copy: Assets/_02LWS/Artifacts~/SmokePosture/SmokePosture-development-20261112.apk
Source/archive hashes match. Scene: Assets/_02LWS/Scenes/FireNotification.unity; Unity 6000.3.26f1; IL2CPP/ARM64, existing project XR settings. The build warning concerns diagnostics debug-symbol detail. No device install/run: ADB lists no attached devices. T051-A complete; T051 remains unchecked because T048~T050 and T043 final gate, then T051-B Export/receiver Import, remain pending.

After build, Unity restored XR simulation files and in-memory preloaded assets. Removed only the build-injected two-entry persisted preloadedAssets block to match its original empty list; reverted the session-generated dynamic font cache to its initially clean version. No intentional font, package, XR configuration or scene edit was made. Source state and runtime visual evidence: Artifacts/Validation/VisualEvidence/20261112-000952-smoke-system-recovery/verification.md.

## Final shared UI development build — 2026-11-12

T060 complete: Android Development/IL2CPP/ARM64 build Succeeded,0 errors,1 diagnostics-symbol warning,293.495 seconds. Actual APK 108592714 bytes; SHA256 E83CD20A3195E0BEE63B15EF05D32E44670396D8B5A227D2D86F8C698860FD92. Main scene Assets/_02LWS/Scenes/FireNotification.unity. No package/engine upgrade or intentional XR setting change.

Output: Artifacts/SmokePosture/SharedUI-development-20261112.apk
Identical archive: Assets/_02LWS/Artifacts~/SharedUI/SharedUI-development-20261112.apk

The warning requests SymbolTable/Full debug symbols for detailed crash traces. Current APK includes the one-Canvas shared UI and replaces earlier APKs only for this refactor's development testing. Source/archive hashes match. ADB returned no attached devices; install/run, actual Quest readability/performance and final receiver acceptance remain pending. No final unitypackage re-export/receiver certification is claimed.

Build-generated XR preloaded entries were restored to the initially empty list. Dynamic Korean glyph cache and automatic LiberationSans material serialization were removed from the change set only after byte-identical backups and hashes were retained under Artifacts/Validation/VisualEvidence/20261112-011720-shared-content-ui/generated-font-backup/. The initial automatic approval rejection was resolved by making cleanup reversible and verifying the backups; no user font edits were intentionally discarded. Raw evidence/media/backups remain Git ignored and outside Assets.