# Build validation — 2026-11-10

First attempt rejected before build: Unity prohibits build output inside Assets, including Artifacts~. Corrected staging output to Artifacts/SmokePosture/SmokePosture-development.apk; if successful copy into the task archive path afterward. No source or XR setting change required.

Final updated Android development build: Succeeded, 0 errors, 1 warning, 58.748 seconds (incremental build). APK bytes=130107831, SHA256=2A7C172959F821DA71CCEA05292DF422B848E242541E50A3A3E96F335EB58F10. Source and archive copies match. Warning concerns crash-diagnostics debug-symbol detail, not compilation. Device install/run not attempted: ADB returned no devices.

Build cleanup: in-memory PlayerSettings preloaded assets already restored to empty by Unity; persisted YAML still contained the two build-injected XR entries. Restored only that exact generated block to the observed original empty list. XR simulation files were restored by Unity.
