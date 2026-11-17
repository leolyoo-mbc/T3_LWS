# School architecture

Original Blender geometry for feature 008. Source: `Art/Models/SchoolArchitecture/Source~/SchoolArchitecture.blend` and reproducible `BuildSchoolArchitecture.py`. The script rebuilds only `LWS_SchoolArchitecture`; it preserves the existing Cube, Camera and Light. No downloaded architecture assets or textures are used.

Unity uses `SchoolArchitecture.fbx`, seven shared model prefabs and six explicitly mapped URP Lit materials. Blender source and script are editing sources, excluded from Unity import and distribution by `Source~`.

One Unity unit is one metre. Models use a floor pivot and local -Z front, except the stairwell whose original local footprint is x0..4,z-2.4..2.4. Read/Write, animation, camera/light import and generated colliders are disabled; static lightmap UVs are generated. Imported material names remain stable on re-export.

Existing scene and source environment prefabs are separate owners. Keep scene hierarchy and original prefab GUIDs. Retained original door objects carry existing colliders and references while their renderers are replaced. OpenDoorHinge and its current open rotation remain authoritative. A Classroom04 is the explicit relocated opening; elevator and navigation anchors remain fixed. Scenario B is moved only if actual overlap is found.

Stair geometry uses two ten-step flights, width1.25m, rise0.16m and tread0.27m, landings at1.60m/3.20m and roof6.40m. These are authoring dimensions, not a code compliance claim. Upper geometry is visual background without new teleport surfaces or climbing. Both flights run along the longer4.8m footprint axis to fit their landings.

Glass is an opaque frosted first proposal without outdoor scenery, refraction or screen-copy effects. Colours and glass appearance require user approval. Character appearance, floor materials/UV scale and the empty smoke practice room remain protected.

Validation evidence is outside Assets, under `Artifacts/Validation/VisualEvidence/20261117-132627-school-realism/`. It is not packaged. User visual approval, Editor checks, receiver import and physical Quest acceptance are separate statuses. Implementation and approval results are recorded in that run's acceptance summary.

Final lighting uses seven baked fill points at2.10m/2.08m, intensity1.6 and shadow radius0.30 for the first two rooms. Existing12 route luminaires use Mixed lighting, intensity1.6, height2.30m and their existing5m range. Four new upper stair fixtures use baked-only points at5.50m, intensity3 and8m range. They add no realtime point lights, controls or colliders. HDR, URP additional-light limit and shadow configuration remain unchanged. Bake quality remains12texels/m with1024 maximum atlas; actual atlas packing is recorded separately. Final data is moved via AssetDatabase to `Lighting/BakedEnvironment`, retaining new asset references, and verified after scene reopen.

After user review, the upper flight uses0.16m treads on a continuous0.18m waist slab with landing bearing beams and two intermediate landing piers. The original outer4.0x4.8m footprint remains. A Classroom04 is at corridor localx11.4m, giving1.25m clear frame separation from the fixed elevator, versus0.45m in the rejected trial. Both revised geometry and spacing were approved from actual images. No new colliders, anchors or realtime lights were added by this revision.
