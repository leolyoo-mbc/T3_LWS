> **2026-11-05 single-scene revision:** The records below describe the former two-scene version. They do not validate the new spatial navigation or a current Android APK. See SingleScene.md for the current Editor checks. Quest 2, performance and clean-project import of the revised package remain pending. The previously generated APK is stale for this revision.

# Android build — 2026-11-04

Unity build ID `build_ee6734680cf5`, target Android, IL2CPP/ARM64, result `Succeeded`, 0 errors, 0 warnings in the BuildReport. Duration 438429 ms (visual replacement rebuild). Explicit scene selection: ContentMenu then FireNotification; SampleScene excluded from this artifact.

- APK: `Artifacts/FireNotification.apk`, 52,714,505 bytes.
- APK SHA-256: `561255e8e2dbbfe181939393d76703509b9b35e20ef1a59b9caef3cd45473908`.
- Application ID: `com.DefaultCompany.VRTemplate` (existing project setting retained).
- Symbols generated separately: `Artifacts/FireNotification-1.0-v1-IL2CPP.symbols.zip`.
- Existing SDK minimum/target: 34. No device installation or launch performed; the user cannot currently connect the Quest 2.

The build result proves compilation/packaging, not standalone controller behavior, headset lifecycle, first-offline launch, performance or compatibility with the later submission project. T037/T038 remain incomplete. Artifact byte size above is the actual APK filesystem size; BuildReport aggregate totalSizeBytes includes other build outputs and is not used as the APK size.

