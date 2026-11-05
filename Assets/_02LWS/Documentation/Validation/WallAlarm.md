# Wall-mounted alarm — 2026-11-05

## Implemented

User selected a wall-mounted call-point set without a hydrant cabinet and requested direct Blender modeling through the configured MCP. This replaces the floating box and synthetic alarm. Existing script, controller-hand poke and stage sequence remain. No AGENTS.md or checklist changes.

Blender 5.2.3 LTS MCP: preserved the original Scene (Cube/Camera/Light), created LWS_WallAlarm, modeled the plate, circular call point, separate MovingCap, location lamp, convex gong, screws and Korean labels. Original geometry; no logos, certification marks or copied product photo textures. Equipment mesh: 9,369 static triangles + 764 moving triangles = 10,133, six material appearances. Approximate overall dimensions 0.20 x 0.49 x 0.084 m; cap diameter 0.05 m. Front and oblique Blender renders were inspected; small text was enlarged after the initial review. Unity URP front/oblique captures were inspected after placement.

References: [Alllite manufacturer](https://www.alllitelife.com/page.php?page=sub2_3_7), including directly inspected bell 120 x 50 mm drawing; [Ilshin supplier catalogue](https://waf-e.dubudisk.com/ilshinfa.dubuplus.com/yollam%40naver.com/O18B62L/DubuDisk/www/2021%EC%B9%B4%EB%8B%A4%EB%A1%9C%EA%B7%B8.pdf), indexed round call point 144 mm, bell 120 mm and lamp 87 mm. Model is an original simplified training representation, not a particular manufacturer's certified product.

## Unity integration

FireAlarm prefab GUID and original moving-cap Transform identity retained. Imported FBX has Blender-to-Unity axis/unit transforms; native Unity meshes bake those transforms to metres and face local -Z. Unity runtime has no .blend or installed-Blender dependency. Both static/moving mesh asset names match filenames. Backplate local X=2.5 m matches the right wall's interior plane; centre of button Y=1.2 m, Z=1.1 m in the content space (world X=42.5 m). These are project tuning values, not legal installation rules.

The moving cap travels 8 mm along surface-local +Z. Its front aligns with the stationary surface at Z=-0.067 m; circular acceptance radius 24 mm, maximum penetration 35 mm, release margin 10 mm, contact tolerance 1 mm. The square broad-phase collider is 50 x 50 x 24 mm, but the radial guard rejects its corners outside the circular cap. No ray/grab success and no contact-only success. Baseline cap/reset reference retained.

BellApproach uses the existing MenuTeleportationProvider. Actual Anchor.RequestTeleport() moves head horizontally over (41.9,0.012,1.1) and faces +X toward the alarm; horizontal head-to-button gap is approximately 0.533 m. Internal menu blocks queued requests. A cyan floor ring marks the approach. This is an initial reach arrangement; actual seated/standing controller reach remains device validation.

Alarm uses a dedicated BellEmitter child at the gong, spatialBlend=1, linear rolloff minDistance=1 m/maxDistance=14 m and Doppler=0. Effects settings still control volume. Removed the superseded root AudioSource so moving the emitter cannot move the equipment. Lifecycle cancellation/restart, internal pause/resume and completion cleanup use the existing AlarmResultPresenter.

The formerly oversized floating label is attached above the equipment, 0.36 x 0.09 m, with explicit two-line text and no pointer target; it remains excluded from ComfortFollowUI.

## Real recording and provenance

[Dave Welsh — School Bell - Fire Bell.wav](https://freesound.org/people/Dave%20Welsh/sounds/523317/), CC0 1.0. Actual solenoid electric bell; not claimed identical to the modeled Korean motor-bell product. Source is the publisher HQ MP3 preview, 48 kHz stereo, not the 96 kHz WAV master. ProcessAlarmAudio.py extracts 56.0–64.1 s, downmixes to mono, removes DC, creates a 100 ms circular crossfade and normalizes peak to 0.5. Local WAV is 48 kHz/PCM16 mono, 7.999979 s (one-sample rounding from 8 s). Source WAV GUID 0f85c5eab42ece34b9c3fe82656f0890 is unchanged. Source preview, source/output hashes and exact transformation are retained in Audio/Alarm; attribution/CC0 record in Documentation/Licenses. No waveform synthesized. No loudness/SPL equivalence to real alarm equipment claimed.

## Verified evidence

Unity 6000.3.26f1 Editor: **162 checks passed**: 25 actual synthetic XRPokeInteractor checks, 6 actual approach-anchor teleport checks, 111 existing progression/menu/lifecycle/full-duration sequence checks, 20 stopped-scene checks. Raw results: WallAlarm-PlayMode.txt, WallAlarm-Teleport.txt, WallAlarm-Regression.txt and WallAlarm-SceneAudit.json. Frames advanced, actual physics overlap produced XRI hover, actual transform penetration triggered alarm. No direct successful PressBell command substitutes for the poke test; direct duplicate commands test rejection.

Initial new test had 1 failure: a diagonal contact in the square collider outside the visible round cap still activated the bell. BellPushInput was changed to a radial lateral check; fresh rerun passed all 25. The synthetic hand is temporary, removed after testing; the original discovered poke array is restored.

The 6 teleport checks invoke Anchor.RequestTeleport(), not a fabricated destination transform. They check menu blocking, final horizontal head position, heading, gap and no stage progression. No real controller is asserted. The 111-check regression uses programmatic input commands and synthetic lifecycle signals, separate from the physical poke coverage.

Static review found no remaining critical/important issue. Zero current compile/console errors at final validation; console retained historical tooling errors and a pre-existing XR audio-output fallback warning. Mesh-name warnings from the initial save were corrected.

## Source and delivery

Art/Models/FireAlarm/Source~/WallAlarm.blend is editable and tracked in the workspace/Git; Source~ deliberately prevents automatic Blender conversion by Unity and is not in unitypackage. The package includes FBX, baked native meshes, URP materials, rebuild/audio-processing scripts and local source/edited audio. Packed OFL font in .blend uses the existing included font license. Full source project still needs the existing matching XRI 3.5.1 Starter Assets prerequisite (40 outside-folder dependencies), not a newly self-contained rig.

## Not verified

T073 remains unchecked: actual headset reach, visual recognition, press feel, recorded-bell timbre/volume and comfort. No new APK, Quest 2 standalone/performance results or receiving-project import. Existing T037/T038/T041/T042/T060/T066 remain unchecked. This work supplies a real electric bell recording, not listening approval of a specific domestic appliance.

Package/source/meta integrity is checked after final documentation export; current hash and inventory live in Artifacts/WallAlarmPackageAudit.json and SHA256SUMS.txt. No before/after extension hooks (.specify/extensions.yml absent).

## Teleport selection layer correction — 2026-11-05

User reported that the new approach anchor worked through grip but not the stick teleport ray. Diagnosis in the running project found the anchor interaction mask Default=1, while both dedicated Teleport Interactors use Teleport bit 31 (signed -2147483648). Physical raycast hit BellApproach, but the layer intersection was zero, so no hover/select/release event reached the anchor. General Near-Far grip selection overlapped Default. The previous RequestTeleport() tests bypassed this input boundary and did not prove a working controller selection route.

Corrected the anchor to the existing dedicated ray mask and aligned the owned TeleportSurface prefab's default; no global InteractionLayerSettings/Input Actions changes. Scene floor already had the correct overridden teleport layer. General grip interactors no longer overlap. Integration must map the Teleport semantic layer consistently; signed bit 31 is this project's current configuration, not a universal Unity layer index.

New reproduction uses an actual XRPoke-free XRRayInteractor, the real rig's teleport ray layer/physics masks, straight physics ray against the anchor collider and manual XRInputButtonReader select/release values equivalent to the stick action. No direct Anchor.RequestTeleport() call or fabricated destination assignment is used. Before fix: 5/6 checks failed, collider hit but no hover/select/teleport. After fix: **6/6 passed**: layer overlap, physics hover, input selection, release-triggered locomotion, grip exclusion and floor-layer compatibility. See WallAlarm-RayInput.txt. This is synthetic input through the full selection path; physical Quest stick testing remains pending.
