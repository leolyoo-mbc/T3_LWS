# Shared systems — 2026-11-05

## Result

SharedSystems is an always-active sibling of MenuSpace and FireNotificationSpace. It contains the existing LwsXRStarterRig (camera, AudioListener, controller inputs, XRInteractionManager), LwsEventSystem, SpaceNavigation, SharedDirectionalLight and SharedLifecycle. No runtime framework or new product script was introduced. The original Corridor scene prefab instance was unpacked to allow its Directional Light to move; the source Corridor prefab remains unchanged. Existing world light rotation and intensity (1.2) were preserved. Fire Point Light, NPCs, alarm, narration UI/audio and progression remain content-specific. Existing font/input/material references remain shared assets.

Play Mode: **90 shared-system checks PASS, 0 failures**, plus **149 existing single-scene regression checks PASS, 0 failures** after this scene revision. One shared rig/listener/EventSystem/UI input module/lifecycle, unchanged instance identities across repeated menu/content/return transitions, active base light throughout, content-only fire light/audio cleanup, and entry while the shared lifecycle is blocked were checked. Clearing the block starts the first presentation. The 149-check suite covers nine-stage synthetic suspension and return/reentry plus the full authored-duration educational command flow. The first shared-check attempt expected a null Run reference; the contract permits the previous canceled run reference. The check was corrected to accept null or Canceled, with a preceding completed menu return to reproduce that case. No product fix was needed.

Fresh Game View menu and content captures at the normal saved camera height were inspected: title/button/disclosures remain visible, sky lighting is present in the menu, and the corridor/NPC/fire and return UI render after entering content. Captures and authoring-only drivers are in .utmp. No verification component was added to the scene. Console errors 0; compilationFailed false.

## Limits

These are Editor checks with programmatic commands and synthetic interruption, not physical Quest controller/teleport/comfort/lifecycle acceptance. Quest 2, performance, revised Android APK and receiving-project import remain pending (T037/T038/T041/T042). The existing XR Starter rig still needs the matching XRI 3.5.1 Starter Assets sample; the prior 40 outside-folder dependency paths are not internalized by this revision. See Integration.md. The exported package excludes the obsolete ContentMenu source scene and includes only _02LWS files; hashes and byte comparison live in Artifacts/SingleScenePackageAudit.json and SHA256SUMS.txt.

## Raw shared-system results

```text
Shared systems Play Mode checks
PASS menu independent active shared root
PASS menu same enabled main light
PASS menu preserved main light intensity
PASS menu same active lifecycle
PASS menu same active XR and navigator
PASS menu one rig/listener
PASS menu one same EventSystem and UI module
PASS menu one shared lifecycle, none in content
PASS menu fire point light stays local
PASS fire point light off in menu
PASS entry while blocked independent active shared root
PASS entry while blocked same enabled main light
PASS entry while blocked preserved main light intensity
PASS entry while blocked same active lifecycle
PASS entry while blocked same active XR and navigator
PASS entry while blocked one rig/listener
PASS entry while blocked one same EventSystem and UI module
PASS entry while blocked one shared lifecycle, none in content
PASS entry while blocked fire point light stays local
PASS menu suspension blocks entry narration/progress
PASS resume starts initial presentation
PASS menu 0 independent active shared root
PASS menu 0 same enabled main light
PASS menu 0 preserved main light intensity
PASS menu 0 same active lifecycle
PASS menu 0 same active XR and navigator
PASS menu 0 one rig/listener
PASS menu 0 one same EventSystem and UI module
PASS menu 0 one shared lifecycle, none in content
PASS menu 0 fire point light stays local
PASS content 0 independent active shared root
PASS content 0 same enabled main light
PASS content 0 preserved main light intensity
PASS content 0 same active lifecycle
PASS content 0 same active XR and navigator
PASS content 0 one rig/listener
PASS content 0 one same EventSystem and UI module
PASS content 0 one shared lifecycle, none in content
PASS content 0 fire point light stays local
PASS local fire light active in content 0
PASS local fire and audio off after return 0
PASS menu 1 independent active shared root
PASS menu 1 same enabled main light
PASS menu 1 preserved main light intensity
PASS menu 1 same active lifecycle
PASS menu 1 same active XR and navigator
PASS menu 1 one rig/listener
PASS menu 1 one same EventSystem and UI module
PASS menu 1 one shared lifecycle, none in content
PASS menu 1 fire point light stays local
PASS content 1 independent active shared root
PASS content 1 same enabled main light
PASS content 1 preserved main light intensity
PASS content 1 same active lifecycle
PASS content 1 same active XR and navigator
PASS content 1 one rig/listener
PASS content 1 one same EventSystem and UI module
PASS content 1 one shared lifecycle, none in content
PASS content 1 fire point light stays local
PASS local fire light active in content 1
PASS local fire and audio off after return 1
PASS menu 2 independent active shared root
PASS menu 2 same enabled main light
PASS menu 2 preserved main light intensity
PASS menu 2 same active lifecycle
PASS menu 2 same active XR and navigator
PASS menu 2 one rig/listener
PASS menu 2 one same EventSystem and UI module
PASS menu 2 one shared lifecycle, none in content
PASS menu 2 fire point light stays local
PASS content 2 independent active shared root
PASS content 2 same enabled main light
PASS content 2 preserved main light intensity
PASS content 2 same active lifecycle
PASS content 2 same active XR and navigator
PASS content 2 one rig/listener
PASS content 2 one same EventSystem and UI module
PASS content 2 one shared lifecycle, none in content
PASS content 2 fire point light stays local
PASS local fire light active in content 2
PASS local fire and audio off after return 2
PASS final menu independent active shared root
PASS final menu same enabled main light
PASS final menu preserved main light intensity
PASS final menu same active lifecycle
PASS final menu same active XR and navigator
PASS final menu one rig/listener
PASS final menu one same EventSystem and UI module
PASS final menu one shared lifecycle, none in content
PASS final menu fire point light stays local
DONE checks=90 failed=0
```
