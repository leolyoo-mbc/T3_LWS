#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace LWS.FireNotification.Editor
{
    // Read-only scene inspection. Evidence never becomes a runtime dependency.
    public static class SchoolArchitectureChecks
    {
        [Serializable] public sealed class Entry
        {
            public string objectId, hierarchyPath, role, action, state;
        }
        [Serializable] public sealed class Baseline { public string scene; public Entry[] inventory, protectedState; }
        [Serializable] public sealed class Change
        {
            public string objectId, reason, beforePose, afterPose;
            public string[] allowedFields, evidenceFiles;
        }
        [Serializable] public sealed class Changes { public Change[] changes = Array.Empty<Change>(); }
        [Serializable] public sealed class Check { public string name, detail; public bool passed; }
        [Serializable] public sealed class Report { public int failureCount; public Check[] checks; }

        [Serializable] public sealed class FloorState { public string hierarchyPath, pose, meshId, geometryHash, assetPath; }
        [Serializable] public sealed class FloorSnapshot { public string unityVersion, source; public bool sourceAssetsVerified; public FloorState[] floors; }
        static bool IsFloor(Renderer renderer) => renderer.sharedMaterials.Any(m => m && m.name.IndexOf("Floor", StringComparison.OrdinalIgnoreCase) >= 0);
        static string GeometryHash(Mesh mesh)
        {
            if (!mesh) return "missing";
            string data;
            if (mesh.isReadable)
            {
                data = string.Join("|", mesh.vertices.Select(v => v.ToString("R"))) + "\n" + string.Join("|", mesh.uv.Select(v => v.ToString("R")));
                for (int s=0;s<mesh.subMeshCount;s++) data += "\n" + string.Join(",", mesh.GetIndices(s));
            }
            else data = EditorJsonUtility.ToJson(mesh);
            using (var hash=System.Security.Cryptography.SHA256.Create())
                return BitConverter.ToString(hash.ComputeHash(System.Text.Encoding.UTF8.GetBytes(data))).Replace("-", "").ToLowerInvariant();
        }
        static FloorState Floor(Renderer renderer)
        {
            var filter=renderer.GetComponent<MeshFilter>(); var mesh=filter ? filter.sharedMesh : null;
            return new FloorState { hierarchyPath=Hierarchy(renderer.transform), pose=Pose(renderer.transform), meshId=mesh ? ObjectId(mesh) : "missing", geometryHash=GeometryHash(mesh), assetPath=mesh ? AssetDatabase.GetAssetPath(mesh) : "" };
        }
        static bool TryPosition(string text, out Vector3 position)
        {
            position=default; if (string.IsNullOrWhiteSpace(text)) return false;
            var values=text.Trim().Trim('(',')').Split(','); if(values.Length!=3)return false;
            var culture=System.Globalization.CultureInfo.InvariantCulture;
            if(!float.TryParse(values[0],System.Globalization.NumberStyles.Float,culture,out float x) || !float.TryParse(values[1],System.Globalization.NumberStyles.Float,culture,out float y) || !float.TryParse(values[2],System.Globalization.NumberStyles.Float,culture,out float z))return false;
            position=new Vector3(x,y,z);return float.IsFinite(x)&&float.IsFinite(y)&&float.IsFinite(z);
        }
        static bool RegisteredPosition(Entry before, Entry after, Change change)
        {
            return change.allowedFields.Contains("position") && TryPosition(change.beforePose,out var registeredBefore) && TryPosition(change.afterPose,out var registeredAfter) &&
                TryPosition(before.state.Split('|')[0],out var original) && TryPosition(after.state.Split('|')[0],out var current) &&
                Vector3.Distance(original,registeredBefore)<=.001f && Vector3.Distance(current,registeredAfter)<=.001f &&
                before.state.Substring(before.state.IndexOf('|')+1)==after.state.Substring(after.state.IndexOf('|')+1);
        }
        public static string CaptureFloorSupplement(string outputDirectory)
        {
            string dir=DirectoryChecked(outputDirectory), candidate=Path.Combine(dir,"floor-baseline.candidate.json");
            if(File.Exists(candidate)||File.Exists(Path.Combine(dir,"floor-baseline.json")))throw new InvalidOperationException("Floor snapshot is immutable once reconstructed");
            string source=Path.Combine(dir,"before-scene.unity"); if(!File.Exists(source))throw new FileNotFoundException("Immutable original scene snapshot required",source);
            var scene=EditorSceneManager.OpenPreviewScene(source);
            try
            {
                var floors=scene.GetRootGameObjects().SelectMany(g=>g.GetComponentsInChildren<Renderer>(true)).Where(IsFloor).Select(Floor).ToArray();
                File.WriteAllText(candidate,JsonUtility.ToJson(new FloorSnapshot { source="before-scene.unity, original mesh asset blobs require independent Git verification", unityVersion=Application.unityVersion, sourceAssetsVerified=false, floors=floors },true));
            }
            finally { EditorSceneManager.ClosePreviewScene(scene); }
            return candidate;
        }
        static void CheckFloorSnapshot(string dir, Transform[] objects, List<Check> results, Baseline baseline)
        {
            string file=Path.Combine(dir,"floor-baseline.json");
            bool captured=baseline.protectedState.Any(e=>e.role=="floorGeometry");
            if(!File.Exists(file)){results.Add(new Check { name="floor geometry evidence",passed=captured,detail=captured?"UV, vertex topology and pose captured in baseline":"legacy baseline requires immutable-scene/unchanged-mesh supplement" });return;}
            var snapshot=JsonUtility.FromJson<FloorSnapshot>(File.ReadAllText(file));
            bool valid=snapshot!=null&&snapshot.sourceAssetsVerified&&snapshot.unityVersion==Application.unityVersion&&snapshot.floors!=null&&snapshot.floors.Length>0;
            results.Add(new Check { name="floor source evidence verified",passed=valid,detail="Original scene snapshot and source mesh blob verification; same Unity version required" });
            if(!valid)return;
            var current=objects.SelectMany(t=>t.GetComponents<Renderer>()).Where(IsFloor).Select(Floor).ToDictionary(f=>f.hierarchyPath);
            foreach(var old in snapshot.floors)
            {
                bool found=current.TryGetValue(old.hierarchyPath,out var now);
                results.Add(new Check { name="floor UV/scale "+old.hierarchyPath,passed=found&&old.pose==now.pose&&old.meshId==now.meshId&&old.geometryHash==now.geometryHash,detail=found?"world pose, mesh identity, UV0, vertices and indices":"missing floor renderer" });
            }
        }
        public static string ObjectId(UnityEngine.Object obj)
        {
            var id = GlobalObjectId.GetGlobalObjectIdSlow(obj);
            if (id.identifierType != 0) return id.ToString();
            // XR editor helpers can be unsaved. Do not collapse all of them to ID zero.
            if (obj is Component component)
                return "Transient:" + Hierarchy(component.transform) + "|" + component.GetType().FullName + "|" + Array.IndexOf(component.GetComponents(component.GetType()), component);
            if (obj is GameObject go) return "Transient:" + Hierarchy(go.transform);
            return "Transient:" + obj.GetType().FullName + "|" + obj.name;
        }
        public static string Hierarchy(Transform t) => t.parent == null ? t.name : Hierarchy(t.parent) + "/" + t.name;
        static Transform[] Objects() => EditorSceneManager.GetActiveScene().GetRootGameObjects()
            .SelectMany(r => r.GetComponentsInChildren<Transform>(true)).ToArray();
        static bool IsSkirting(string name) => name.IndexOf("skirting", StringComparison.OrdinalIgnoreCase) >= 0;
        static string Role(Transform t)
        {
            if (IsSkirting(t.name)) return "Skirting";
            if (new[] { "ClosedDoor", "OpenDoor", "RecessedDoorLeaf" }.Contains(t.name)) return "Door";
            if (t.name == "WindowGlass") return "Window";
            if (t.name.StartsWith("LiftDoor", StringComparison.Ordinal)) return "Elevator";
            if (t.name == "Elevator") return "Elevator";
            if (t.name.StartsWith("Stairwell", StringComparison.Ordinal)) return "Stairwell";
            return null;
        }
        static string Pose(Transform t) => t.position.ToString("R") + "|" + t.rotation.ToString("R") + "|" + t.lossyScale.ToString("R");
        static string SerializedState(Component component)
        {
            var values = new List<string>();
            using (var serialized = new SerializedObject(component))
            {
                var p = serialized.GetIterator();
                while (p.Next(true))
                {
                    if (p.propertyPath.StartsWith("m_", StringComparison.Ordinal) || p.propertyPath.EndsWith(".m_FileID", StringComparison.Ordinal)) continue;
                    string value;
                    switch (p.propertyType)
                    {
                        case SerializedPropertyType.ObjectReference: value = p.objectReferenceValue ? ObjectId(p.objectReferenceValue) : "null"; break;
                        case SerializedPropertyType.String: value = p.stringValue; break;
                        case SerializedPropertyType.Boolean: value = p.boolValue.ToString(); break;
                        case SerializedPropertyType.Integer: case SerializedPropertyType.Enum: value = p.intValue.ToString(); break;
                        case SerializedPropertyType.Float: value = p.doubleValue.ToString("R", System.Globalization.CultureInfo.InvariantCulture); break;
                        default: continue;
                    }
                    values.Add(p.propertyPath + "=" + value);
                }
            }
            return string.Join("\n", values);
        }
        // SerializedProperty can expose a reference's process-local instance backing ID.
        // Keep the stable parent ObjectReference identity; ignore only that backing integer.
        static string StableState(string state) => string.Join("\n", state.Split('\n').Where(line => !line.Split('=')[0].EndsWith(".m_FileID", StringComparison.Ordinal)));
        static Entry[] ProtectedState(Transform[] objects)
        {
            var entries = new List<Entry>();
            foreach (var t in objects)
            {
                string path = Hierarchy(t);
                bool keep = path.StartsWith("SharedSystems/", StringComparison.Ordinal) ||
                    path.Contains("/Navigation") || path.Contains("/Anchors") || path.Contains("/Characters") ||
                    path.Contains("/Effects") || path.Contains("/Interactables") || path.Contains("/Logic") || t.name == "OpenDoorHinge";
                if (keep) entries.Add(new Entry { objectId = ObjectId(t), hierarchyPath = path, role = "pose", state = Pose(t) });
                foreach (var script in t.GetComponents<MonoBehaviour>())
                    if (script) entries.Add(new Entry { objectId = ObjectId(script), hierarchyPath = path, role = "script", state = SerializedState(script) });
                foreach (var renderer in t.GetComponents<Renderer>())
                {
                    if (path.Contains("/Characters") || (renderer.sharedMaterials.Any(m => m && m.name.IndexOf("Floor", StringComparison.OrdinalIgnoreCase) >= 0)))
                    {
                        var mf = renderer.GetComponent<MeshFilter>();
                        var sk = renderer as SkinnedMeshRenderer;
                        string meshId = mf && mf.sharedMesh ? ObjectId(mf.sharedMesh) : sk && sk.sharedMesh ? ObjectId(sk.sharedMesh) : "";
                        var mats = renderer.sharedMaterials.Select(m => m ? ObjectId(m) + "|" + EditorJsonUtility.ToJson(m) : "null");
                        entries.Add(new Entry { objectId = ObjectId(renderer), hierarchyPath = path, role = "appearance", state = meshId + "\n" + string.Join("\n", mats) });
                    }
                }
                foreach(var floor in t.GetComponents<Renderer>().Where(IsFloor))
                {
                    var f=Floor(floor);
                    entries.Add(new Entry {objectId=ObjectId(floor)+"|floorGeometry",hierarchyPath=f.hierarchyPath,role="floorGeometry",state=f.pose+"\n"+f.meshId+"\n"+f.geometryHash});
                }
                if (Role(t) == "Door")
                    foreach (var collider in t.GetComponents<Collider>())
                        entries.Add(new Entry { objectId = ObjectId(collider), hierarchyPath = path, role = "collider", state = Pose(t) + "|" + EditorJsonUtility.ToJson(collider) });
            }
            return entries.ToArray();
        }
        static string DirectoryChecked(string path)
        {
            string full = Path.GetFullPath(path), root = Path.GetFullPath("Artifacts/Validation/VisualEvidence") + Path.DirectorySeparatorChar;
            if (!full.StartsWith(root, StringComparison.OrdinalIgnoreCase)) throw new ArgumentException("Evidence directory must be under Artifacts/Validation/VisualEvidence");
            if (EditorSceneManager.GetActiveScene().path != "Assets/_02LWS/Scenes/FireNotification.unity") throw new InvalidOperationException("Unexpected scene");
            if (Application.isPlaying) throw new InvalidOperationException("Use saved Edit Mode state for structural checks");
            Directory.CreateDirectory(full); return full;
        }
        public static string CaptureBaseline(string outputDirectory)
        {
            string dir = DirectoryChecked(outputDirectory), file = Path.Combine(dir, "baseline.json");
            if (File.Exists(file)) throw new InvalidOperationException("Baseline is immutable; choose a fresh directory");
            var objects = Objects();
            var baseline = new Baseline {
                scene = EditorSceneManager.GetActiveScene().path,
                inventory = objects.Where(t => Role(t) != null).Select(t => new Entry { objectId = ObjectId(t), hierarchyPath = Hierarchy(t), role = Role(t), action = IsSkirting(t.name) ? "remove_skirting" : "replace_visual", state = Pose(t) }).ToArray(),
                protectedState = ProtectedState(objects)
            };
            File.WriteAllText(file, JsonUtility.ToJson(baseline, true)); return file;
        }
        public static string ValidateAll(string baselinePath, string outputDirectory)
        {
            string dir = DirectoryChecked(outputDirectory);
            if (!File.Exists(baselinePath)) throw new FileNotFoundException("Missing baseline", baselinePath);
            var baseline = JsonUtility.FromJson<Baseline>(File.ReadAllText(baselinePath));
            if (baseline == null || baseline.scene != EditorSceneManager.GetActiveScene().path) throw new InvalidOperationException("Invalid baseline scene");
            var changes = new Changes(); string allowed = Path.Combine(dir, "allowed-changes.json");
            if (File.Exists(allowed))
            {
                changes = JsonUtility.FromJson<Changes>(File.ReadAllText(allowed));
                if (changes == null || changes.changes == null) throw new InvalidOperationException("Invalid allowed-changes.json");
                foreach (var change in changes.changes)
                {
                    var item = baseline.inventory.FirstOrDefault(e => e.objectId == change.objectId);
                    var original = baseline.protectedState.FirstOrDefault(e => e.objectId == change.objectId);
                    if (string.IsNullOrEmpty(change.objectId) || string.IsNullOrWhiteSpace(change.reason) || change.evidenceFiles == null || change.evidenceFiles.Length == 0 || change.allowedFields == null || change.allowedFields.Any(f => f != "position" && f != "collider") || !TryPosition(change.beforePose,out _) || !TryPosition(change.afterPose,out _))
                        throw new InvalidOperationException("Invalid change record");
                    string path = original != null ? original.hierarchyPath : item != null ? item.hierarchyPath : "";
                    if (path.Contains("/Anchors") || path.Contains("/Navigation") || path.Contains("/Characters") || path.Contains("/Effects") || (original != null && original.role != "collider"))
                        throw new InvalidOperationException("Protected state cannot be exempted: " + path);
                }
            }
            var objects = Objects(); var current = ProtectedState(objects).ToDictionary(e => e.objectId);
            var results = new List<Check>();
            foreach (var before in baseline.protectedState)
            {
                var relocation=changes.changes.FirstOrDefault(c=>c.objectId==before.objectId&&before.role=="collider"); bool exempt=relocation!=null;
                bool found = current.TryGetValue(before.objectId, out var after);
                // TMP regenerates fallback-font child renderers when the scene reopens.
                // Their parent text and UI behavior are checked separately; they are derived output.
                bool generatedText = before.hierarchyPath.Contains("/TMP SubMesh");
                if (!found && generatedText)
                {
                    string parentPath = before.hierarchyPath.Substring(0, before.hierarchyPath.LastIndexOf('/'));
                    var parent = objects.FirstOrDefault(t => Hierarchy(t) == parentPath);
                    if (parent && parent.GetComponent<TMPro.TMP_Text>())
                    {
                        results.Add(new Check { name="derived TMP renderer "+before.hierarchyPath, passed=true, detail="regenerated output; parent text exists, run UI text/layout checks" });
                        continue;
                    }
                }
                results.Add(new Check { name = "preserve " + before.role + " " + before.hierarchyPath, passed = found && (exempt ? RegisteredPosition(before,after,relocation) : StableState(before.state) == StableState(after.state)), detail = found ? (exempt ? "explicit position relocation, collider rotation/scale/properties retained" : "exact baseline comparison") : "missing protected object" });
            }
            results.Add(new Check { name = "no remaining skirting", passed = !objects.Any(t => IsSkirting(t.name)), detail = objects.Count(t => IsSkirting(t.name)).ToString() });
            foreach (var pair in new[] { ("SchoolDoorSolid",6), ("SchoolDoorGlazed",10), ("SchoolDoorOpen",1), ("SchoolWindowSliding",10), ("SchoolElevatorPortal",2), ("SchoolStairwellVisual",4) })
            {
                int count = objects.Count(t => t.name == pair.Item1);
                results.Add(new Check { name = "placement " + pair.Item1, passed = count == pair.Item2, detail = count + "/" + pair.Item2 });
            }
            int missing = objects.Sum(t => GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(t.gameObject));
            results.Add(new Check { name = "no missing scripts", passed = missing == 0, detail = missing.ToString() });
            string[] outside = AssetDatabase.GetDependencies(baseline.scene, true).Where(p => p.StartsWith("Assets/", StringComparison.Ordinal) && !p.StartsWith("Assets/_02LWS/", StringComparison.Ordinal)).ToArray();
            results.Add(new Check { name = "product references stay inside _02LWS", passed = outside.Length == 0, detail = string.Join("\n", outside) });
            foreach (var prefab in new[] { "Corridor", "CorridorFinish", "SmokeRoomFinish", "SchoolRouteA", "SchoolRouteB" })
            {
                string path = "Assets/_02LWS/Prefabs/Environment/" + prefab + ".prefab";
                var asset = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                if (!asset) { results.Add(new Check { name = "prefab " + prefab, passed = false, detail = "missing asset" }); continue; }
                var parts = asset.GetComponentsInChildren<Transform>(true);
                var expected = prefab == "Corridor" ? new[] {6,0,0,0,0,0} : prefab == "SmokeRoomFinish" ? new[] {0,0,1,0,0,0} : prefab == "SchoolRouteA" ? new[] {0,6,0,6,1,2} : prefab == "SchoolRouteB" ? new[] {0,4,0,4,1,2} : new[] {0,0,0,0,0,0};
                string[] names = { "SchoolDoorSolid", "SchoolDoorGlazed", "SchoolDoorOpen", "SchoolWindowSliding", "SchoolElevatorPortal", "SchoolStairwellVisual" };
                for (int i=0;i<names.Length;i++) { int count=parts.Count(t=>t.name==names[i]); results.Add(new Check { name="prefab placement "+prefab+" "+names[i], passed=count==expected[i], detail=count+"/"+expected[i] }); }
                int skirts=parts.Count(t=>IsSkirting(t.name));
                int scripts=parts.Sum(t=>GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(t.gameObject));
                results.Add(new Check { name="prefab integrity "+prefab, passed=skirts==0&&scripts==0, detail="skirting="+skirts+", missingScripts="+scripts });
            }
            string[] oldNames = { "ClosedDoor", "OpenDoor", "RecessedDoorLeaf", "WindowGlass", "LiftDoor01", "LiftDoor02" };
            int oldVisible = objects.Where(t=>oldNames.Contains(t.name)).SelectMany(t=>t.GetComponents<Renderer>()).Count(r=>r.enabled);
            results.Add(new Check { name="no duplicate old visual leaves", passed=oldVisible==0, detail=oldVisible.ToString() });
            CheckFloorSnapshot(dir,objects,results,baseline);
            var report = new Report { checks = results.ToArray(), failureCount = results.Count(c => !c.passed) };
            string resultPath = Path.Combine(dir, "checks.json"); File.WriteAllText(resultPath, JsonUtility.ToJson(report, true)); return resultPath;
        }
    }
}
#endif
