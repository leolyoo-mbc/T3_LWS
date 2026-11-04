# Environment inspection

Inspected 2026-11-04 in the connected Unity Editor. No packages or project settings changed.

This opening statement describes the initial inspection. Implementation later added the two owned scenes to the shared Android build list (menu first). Unity's Editor/build serialization also updated existing XR/URP/default-volume settings, the player preload list and UnityConnect settings in the working tree. These are outside the exported feature folder and are not supplied as replacement ProjectSettings to the receiving project. No XR SDK/package upgrade was performed; inspect these host-project serialization changes separately before integrating/committing the overall working tree.

| Item | Observed |
|---|---|
| Unity | 6000.3.26f1 (de8167e7440c) |
| Android module | BuildPipeline.IsBuildTargetSupported(Android) = true |
| Android toolchain | SDK, NDK, OpenJDK directories present under Editor/Data/PlaybackEngines/AndroidPlayer |
| Scripting backend / architecture | IL2CPP / ARM64, queried in Editor |
| Minimum / target SDK | 34 / 34; installation on the owned Quest 2 remains unverified |
| Input | activeInputHandler = 1 (Input System) |
| Render pipeline | URP 17.3.0 |
| XR packages | XRI 3.5.1; OpenXR 1.17.1; Meta OpenXR 2.5.1; XR Management 4.6.1 |
| Input System | 1.20.0 |
| Android OpenXR | OpenXRLoader present; MetaQuestFeature Android enabled, Quest 2 target enabled; OculusTouchControllerProfile Android enabled |

The old `OculusQuestFeature Android` is disabled. The newer `MetaQuestFeature Android` is enabled; these are distinct features, so the former is not evidence that Quest support is absent. Android XR Support is also enabled in the existing project and has not been changed. A build and device validation must check compatibility.

Sources: ProjectSettings/ProjectVersion.txt, ProjectSettings/ProjectSettings.asset, Packages/manifest.json and packages-lock.json, Assets/XR/Settings/OpenXRPackageSettings.asset, connected Editor query. The Editor reports automatic asset refresh disabled; explicitly refresh after writing files. No standalone Android build or controller/device test has been performed.
