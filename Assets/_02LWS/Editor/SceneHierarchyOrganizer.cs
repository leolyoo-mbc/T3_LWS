#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace LWS.FireNotification.Editor
{
    // This tool deliberately stores its working data outside Assets.
    public static class SceneHierarchyOrganizer
    {
        public const string ScenePath = "Assets/_02LWS/Scenes/FireNotification.unity";
        public enum Scope { StructureOnly, NamesOnly, Full }
        [Serializable] public class Property { public string path, type, value; }
        [Serializable] public class ComponentRecord { public string id, type; public List<Property> properties = new(); }
        [Serializable] public class ObjectRecord
        {
            public string id, parent, path, name, prefab;
            public int sibling;
            public Vector3 position, scale;
            public Quaternion rotation;
            public Vector3[] corners;
            public List<ComponentRecord> components = new();
        }
        [Serializable] public class SourceHash { public string path, hash; }
        [Serializable] public class Baseline
        {
            public string scenePath, sceneGuid, sceneHash, capturedAt;
            public List<SourceHash> sources = new();
            public List<ObjectRecord> objects = new();
        }
        [Serializable] public class Entry
        {
            public string id, originalPath, parent, name, disposition, reason;
            public bool fixedParent, protectedObject;
        }
        [Serializable] public class Group { public string key, parent, name, id; }
        [Serializable] public class Plan
        {
            public Scope scope = Scope.Full;
            public string baselineHash, state = "Planned";
            public int savedSceneHandle, savedProcessId;
            public List<Entry> entries = new();
            public List<Group> groups = new();
        }
        [Serializable] public class Comparison
        {
            public string scope, result, naming, structure;
            public int existingObjects, currentObjects, approvedGroups;
            public List<string> errors = new();
        }
        static string SessionKey => "LWS.Hierarchy.Run." + Application.dataPath;
        static Dictionary<Object, string> referenceIds = new();
        public static string RunDirectory
        {
            get => EditorPrefs.GetString(SessionKey, "");
            set
            {
                var full = Path.GetFullPath(value);
                var root = Path.GetFullPath("Artifacts/Validation/VisualEvidence") + Path.DirectorySeparatorChar;
                if (!full.StartsWith(root, StringComparison.OrdinalIgnoreCase)) throw new InvalidOperationException("Evidence directory must be inside Artifacts/Validation/VisualEvidence");
                Directory.CreateDirectory(full);
                EditorPrefs.SetString(SessionKey, full);
            }
        }
        static string FilePath(string file)
        {
            if (string.IsNullOrEmpty(RunDirectory)) throw new InvalidOperationException("Capture Baseline first");
            return Path.Combine(RunDirectory, file);
        }
        static void Write(string file, object data) => File.WriteAllText(FilePath(file), JsonUtility.ToJson(data, true));
        static T Read<T>(string file) => JsonUtility.FromJson<T>(File.ReadAllText(FilePath(file)));
        public static string Id(Object obj)
        {
            if (obj == null) return "";
            if (referenceIds.TryGetValue(obj, out var cached)) return cached;
            var result = PersistentId(obj);
            referenceIds[obj] = result;
            return result;
        }
        static string PersistentId(Object obj)
        {
            var id = GlobalObjectId.GetGlobalObjectIdSlow(obj);
            if ((int)id.identifierType != 0) return id.ToString();
            // TMP's DontSave children have no persistent file ID. Anchor their identity to the owning saved object.
            if (obj is GameObject go && (go.GetComponent<TMPro.TMP_SubMesh>() != null || go.GetComponent<TMPro.TMP_SubMeshUI>() != null))
                return "generated:" + Id(go.transform.parent.gameObject) + ":" + go.name;
            if (obj is Component component) return Id(component.gameObject) + ":" + component.GetType().FullName;
            if (obj is Mesh mesh) return "generated-mesh:" + mesh.name;
            if (obj is Material material) return "generated-material:" + material.name + ":" + Id(material.shader) + ":" + Id(material.mainTexture);
            throw new InvalidOperationException("Reference has no persistent identity: " + obj.GetType().FullName + " " + obj.name);
        }
        public static string HierarchyPath(Transform t) => t.parent == null ? t.name : HierarchyPath(t.parent) + "/" + t.name;
        public static Transform[] Transforms(Scene scene) => scene.GetRootGameObjects().SelectMany(r => r.GetComponentsInChildren<Transform>(true)).ToArray();
        static string Hash(string path)
        {
            using var sha = SHA256.Create();
            return Convert.ToBase64String(sha.ComputeHash(File.ReadAllBytes(path)));
        }
        public static void Guard(string path, bool playing, bool compiling, bool dirty, bool writing)
        {
            if (path != ScenePath) throw new InvalidOperationException("Open FireNotification scene");
            if (playing || compiling) throw new InvalidOperationException("Edit Mode with no compilation required");
            if (writing && dirty) throw new InvalidOperationException("Unsaved scene changes: capture/apply refused");
        }
        static Scene Context(bool writing)
        {
            var scene = SceneManager.GetActiveScene();
            Guard(scene.path, EditorApplication.isPlayingOrWillChangePlaymode, EditorApplication.isCompiling, scene.isDirty, writing);
            return scene;
        }
        static string Value(SerializedProperty p)
        {
            if (p.propertyType == SerializedPropertyType.ObjectReference) return Id(p.objectReferenceValue);
            if (p.propertyType == SerializedPropertyType.ExposedReference) return Id(p.exposedReferenceValue);
            if (p.propertyType == SerializedPropertyType.String) return p.stringValue;
            if (p.propertyType == SerializedPropertyType.Generic || p.propertyType == SerializedPropertyType.ManagedReference) return p.type;
            if (p.propertyType == SerializedPropertyType.AnimationCurve)
            {
                var curve = p.animationCurveValue;
                return JsonUtility.ToJson(new CurveData { keys = curve.keys, pre = (int)curve.preWrapMode, post = (int)curve.postWrapMode });
            }
            if (p.propertyType == SerializedPropertyType.Gradient) return JsonUtility.ToJson(new GradientData { colors = p.gradientValue.colorKeys, alphas = p.gradientValue.alphaKeys, mode = (int)p.gradientValue.mode });
            var value = p.boxedValue;
            return value is IFormattable f ? f.ToString(null, CultureInfo.InvariantCulture) : value?.ToString() ?? "";
        }
        [Serializable] class CurveData { public Keyframe[] keys; public int pre, post; }
        [Serializable] class GradientData { public GradientColorKey[] colors; public GradientAlphaKey[] alphas; public int mode; }
        public static Baseline CaptureScene(Scene scene)
        {
            referenceIds.Clear();
            var baseline = new Baseline { scenePath = scene.path, sceneGuid = AssetDatabase.AssetPathToGUID(scene.path), sceneHash = Hash(scene.path), capturedAt = DateTimeOffset.Now.ToString("o") };
            foreach (var t in Transforms(scene))
            {
                var record = new ObjectRecord { id = Id(t.gameObject), parent = Id(t.parent == null ? null : t.parent.gameObject), name = t.name, path = HierarchyPath(t), sibling = t.GetSiblingIndex(), position = t.position, rotation = t.rotation, scale = t.lossyScale, prefab = PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(t.gameObject) };
                if (t is RectTransform rect) { record.corners = new Vector3[4]; rect.GetWorldCorners(record.corners); }
                foreach (var obj in new Object[] { t.gameObject }.Concat(t.GetComponents<Component>()))
                {
                    if (obj == null) throw new InvalidOperationException("Missing component: " + record.path);
                    var component = new ComponentRecord { id = Id(obj), type = obj.GetType().FullName };
                    using var serialized = new SerializedObject(obj);
                    var p = serialized.GetIterator();
                    bool enter = true;
                    while (p.Next(enter))
                    {
                        component.properties.Add(new Property { path = p.propertyPath, type = p.propertyType.ToString(), value = Value(p) });
                        enter = p.propertyType != SerializedPropertyType.ObjectReference && p.propertyType != SerializedPropertyType.ExposedReference && p.propertyType != SerializedPropertyType.String;
                    }
                    record.components.Add(component);
                }
                baseline.objects.Add(record);
            }
            foreach (var path in baseline.objects.Select(o => o.prefab).Where(p => !string.IsNullOrEmpty(p)).Distinct())
                baseline.sources.Add(new SourceHash { path = path, hash = Hash(path) });
            return baseline;
        }
        [MenuItem("Tools/LWS/Hierarchy/Capture Baseline")]
        public static void CaptureBaseline()
        {
            var scene = Context(true);
            RunDirectory = CaptureDirectory(RunDirectory);
            Write("baseline.json", CaptureScene(scene));
            File.WriteAllLines(FilePath("hierarchy-before.txt"), Transforms(scene).Select(HierarchyPath));
            Debug.Log("Hierarchy baseline captured: " + RunDirectory);
        }
        public static string CaptureDirectory(string current)
        {
            if (!string.IsNullOrEmpty(current) && !File.Exists(Path.Combine(current, "baseline.json"))) return current;
            var stem = "Artifacts/Validation/VisualEvidence/" + DateTime.Now.ToString("yyyyMMdd-HHmmss") + "-organize-scene-hierarchy";
            var candidate = stem;
            for (int suffix = 1; Directory.Exists(candidate) || File.Exists(candidate); suffix++) candidate = stem + "-" + suffix.ToString("00");
            return candidate;
        }
        public static void ValidatePlan(Baseline baseline, Plan plan)
        {
            if (plan.baselineHash != baseline.sceneHash) throw new InvalidOperationException("Manifest baseline mismatch");
            if (plan.entries.Count != baseline.objects.Count || plan.entries.Select(e => e.id).Distinct().Count() != plan.entries.Count || baseline.objects.Any(o => !plan.entries.Any(e => e.id == o.id))) throw new InvalidOperationException("Missing or duplicate classifications");
            var ids = baseline.objects.ToDictionary(o => o.id);
            var keys = new HashSet<string>(ids.Keys.Concat(plan.groups.Select(g => g.key))) { "" };
            if (plan.groups.Select(g => g.key).Distinct().Count() != plan.groups.Count || plan.groups.Any(g => ids.ContainsKey(g.key))) throw new InvalidOperationException("Duplicate groups");
            if (plan.groups.Any(g => !Regex.IsMatch(g.name, "^[A-Z][A-Za-z0-9]*$"))) throw new InvalidOperationException("Invalid group name");
            if (plan.scope == Scope.NamesOnly && plan.groups.Count != 0) throw new InvalidOperationException("NamesOnly cannot create groups");
            foreach (var e in plan.entries)
            {
                var old = ids[e.id];
                if (!keys.Contains(e.parent) || e.parent == e.id || string.IsNullOrEmpty(e.reason)) throw new InvalidOperationException("Unclassified or invalid parent: " + e.originalPath);
                if (e.disposition != "Protected" && e.disposition != "RenameOnly" && e.disposition != "MoveAndRename") throw new InvalidOperationException("Invalid disposition");
                if (e.fixedParent && e.parent != old.parent || e.protectedObject && e.name != old.name) throw new InvalidOperationException("Protection violation: " + e.originalPath);
                if (plan.scope == Scope.NamesOnly && e.parent != old.parent || plan.scope == Scope.StructureOnly && e.name != old.name) throw new InvalidOperationException("Scope mismatch");
                var seen = new HashSet<string> { e.id }; var parent = e.parent;
                while (parent != "")
                {
                    if (!seen.Add(parent)) throw new InvalidOperationException("Parent cycle");
                    parent = plan.entries.FirstOrDefault(v => v.id == parent)?.parent ?? plan.groups.Single(g => g.key == parent).parent;
                }
            }
            if (plan.groups.Any(g => !keys.Contains(g.parent) || !plan.entries.Any(e => e.parent == g.key) && !plan.groups.Any(v => v.parent == g.key))) throw new InvalidOperationException("Empty or invalid group");
            if (plan.scope != Scope.StructureOnly)
            {
                var duplicates = plan.entries.Where(e => !e.protectedObject).GroupBy(e => e.parent + "\n" + e.name).Where(g => g.Count() > 1);
                if (duplicates.Any()) throw new InvalidOperationException("Duplicate target names: " + string.Join(", ", duplicates.Select(g => g.First().name)));
                if (plan.entries.Any(e => !e.protectedObject && !Regex.IsMatch(e.name, "^[A-Z][A-Za-z0-9]*$"))) throw new InvalidOperationException("Invalid English PascalCase name");
            }
        }
        public static Comparison CompareScene(Baseline baseline, Plan plan, Scene scene)
        {
            ValidatePlan(baseline, plan);
            var now = CaptureScene(scene);
            var result = new Comparison { scope = plan.scope.ToString(), naming = plan.scope == Scope.StructureOnly ? "NotEvaluated" : "PASS", structure = plan.scope == Scope.NamesOnly ? "NotEvaluated" : "PASS", existingObjects = baseline.objects.Count, currentObjects = now.objects.Count, approvedGroups = plan.groups.Count };
            var current = now.objects.ToDictionary(o => o.id);
            var transformsById = Transforms(scene).ToDictionary(t => Id(t.gameObject));
            var entries = plan.entries.ToDictionary(e => e.id);
            var groupIds = plan.groups.ToDictionary(g => g.key, g => g.id ?? "");
            string Resolve(string key) => groupIds.TryGetValue(key, out var id) ? id : key;
            if (now.objects.Count != baseline.objects.Count + plan.groups.Count) result.errors.Add("Object count mismatch");
            foreach (var old in baseline.objects)
            {
                if (!current.TryGetValue(old.id, out var n)) { result.errors.Add("Missing object: " + old.path); continue; }
                var e = entries[old.id]; var moved = old.parent != e.parent;
                if (n.name != e.name || n.parent != Resolve(e.parent)) result.errors.Add("Unapproved name/parent: " + old.path);
                if (n.prefab != old.prefab) result.errors.Add("Prefab source changed: " + old.path);
                if (Vector3.Distance(old.position, n.position) > .0001f || Quaternion.Angle(old.rotation, n.rotation) > .01f || Vector3.Distance(old.scale, n.scale) > .0001f) result.errors.Add("World transform changed: " + old.path);
                if (old.corners != null && old.corners.Length == 4 && (n.corners == null || n.corners.Length != 4 || old.corners.Where((v, i) => Vector3.Distance(v, n.corners[i]) > .0001f).Any())) result.errors.Add("UI world corners changed: " + old.path);
                if (plan.scope == Scope.NamesOnly && n.sibling != old.sibling) result.errors.Add("Protected sibling order changed: " + old.path);
                var components = n.components.ToDictionary(c => c.id);
                if (old.components.Count != n.components.Count) result.errors.Add("Existing component count changed: " + old.path);
                foreach (var c in old.components)
                {
                    if (!components.TryGetValue(c.id, out var nc) || nc.type != c.type) { result.errors.Add("Missing/replaced component: " + old.path + " " + c.type); continue; }
                    var props = nc.properties.ToDictionary(p => p.path);
                    foreach (var p in c.properties)
                    {
                        bool transform = c.type == typeof(Transform).FullName || c.type == typeof(RectTransform).FullName;
                        if (c.type == typeof(GameObject).FullName && p.path == "m_Name") continue;
                        if (transform && (p.path.StartsWith("m_Children", StringComparison.Ordinal) || p.path == "m_Father" || p.path == "m_RootOrder")) continue;
                        if (transform && moved && (p.path.StartsWith("m_LocalPosition", StringComparison.Ordinal) || p.path.StartsWith("m_LocalRotation", StringComparison.Ordinal) || p.path.StartsWith("m_LocalScale", StringComparison.Ordinal) || p.path.StartsWith("m_LocalEulerAnglesHint", StringComparison.Ordinal))) continue;
                        if (!props.TryGetValue(p.path, out var np) || np.type != p.type || np.value != p.value) result.errors.Add("Property changed: " + old.path + " " + c.type + "." + p.path);
                    }
                    if (props.Keys.Except(c.properties.Select(p => p.path)).Any()) result.errors.Add("Unexpected serialized properties: " + old.path + " " + c.type);
                }
            }
            foreach (var g in plan.groups)
            {
                if (!current.TryGetValue(g.id ?? "", out var n)) { result.errors.Add("Missing approved group: " + g.key); continue; }
                var t = transformsById[g.id];
                if (n.name != g.name || n.parent != Resolve(g.parent) || n.components.Count != 2 || t.localPosition != Vector3.zero || t.localRotation != Quaternion.identity || t.localScale != Vector3.one || !t.gameObject.activeSelf || t.childCount == 0) result.errors.Add("Invalid group: " + g.key);
            }
            var approved = new HashSet<string>(baseline.objects.Select(o => o.id).Concat(plan.groups.Select(g => g.id)));
            if (now.objects.Any(o => !approved.Contains(o.id))) result.errors.Add("Unexpected objects");
            // Preserve ordering for retained children and the deterministic append order of moved objects/groups.
            foreach (var parent in new[] { "" }.Concat(plan.entries.Select(e => e.id)).Concat(plan.groups.Select(g => g.key)))
            {
                var retained = baseline.objects.Where(o => o.parent == parent && entries[o.id].parent == parent).OrderBy(o => o.sibling).Select(o => o.id);
                var addedGroups = plan.groups.Where(g => g.parent == parent).Select(g => g.id);
                var moved = plan.entries.Where(e => e.parent == parent && baseline.objects.Single(o => o.id == e.id).parent != parent).Select(e => e.id);
                var expected = retained.Concat(addedGroups).Concat(moved).ToArray();
                var actual = now.objects.Where(o => o.parent == Resolve(parent)).OrderBy(o => o.sibling).Select(o => o.id).ToArray();
                if (!expected.SequenceEqual(actual)) result.errors.Add("Child order differs from manifest: " + parent);
            }
            if (baseline.sources.Any(s => Hash(s.path) != s.hash)) result.errors.Add("Prefab source hash changed");
            if (plan.scope != Scope.StructureOnly)
            {
                var names = SceneHierarchyOrganizerChecks.CheckNames(scene);
                if (names != "") result.errors.Add(names);
            }
            if (plan.scope == Scope.Full)
            {
                var structure = SceneHierarchyOrganizerChecks.CheckStructure(scene);
                if (structure != "") result.errors.Add(structure);
                var ui = SceneHierarchyOrganizerChecks.CheckSharedUI(scene);
                if (ui != "") result.errors.Add(ui);
            }
            result.result = result.errors.Count == 0 ? "PASS" : "FAIL";
            return result;
        }
        public static Comparison ApplyToScene(Scene scene, Baseline baseline, Plan plan)
        {
            ValidatePlan(baseline, plan);
            var unchanged = IdentityPlan(baseline);
            if (CompareScene(baseline, unchanged, scene).errors.Count != 0) throw new InvalidOperationException("Scene differs from captured baseline");
            var map = Transforms(scene).ToDictionary(t => Id(t.gameObject));
            Undo.IncrementCurrentGroup(); var undo = Undo.GetCurrentGroup(); Undo.SetCurrentGroupName("Organize scene hierarchy");
            try
            {
                foreach (var g in plan.groups)
                {
                    var go = new GameObject(g.name); SceneManager.MoveGameObjectToScene(go, scene); Undo.RegisterCreatedObjectUndo(go, "Create hierarchy group");
                    if (g.parent != "") Undo.SetTransformParent(go.transform, map[g.parent], "Parent hierarchy group");
                    go.transform.localPosition = Vector3.zero; go.transform.localRotation = Quaternion.identity; go.transform.localScale = Vector3.one;
                    map.Add(g.key, go.transform); g.id = Id(go);
                }
                foreach (var e in plan.entries)
                {
                    var t = map[e.id]; var parent = e.parent == "" ? null : map[e.parent];
                    if (t.parent != parent) Undo.SetTransformParent(t, parent, "Group scene object");
                    if (t.name != e.name)
                    {
                        Undo.RecordObject(t.gameObject, "Rename scene object"); t.name = e.name;
                        if (PrefabUtility.IsPartOfPrefabInstance(t.gameObject)) PrefabUtility.RecordPrefabInstancePropertyModifications(t.gameObject);
                    }
                }
                plan.state = "AppliedUnsaved";
                var result = CompareScene(baseline, plan, scene);
                if (result.errors.Count != 0) throw new InvalidOperationException(string.Join("\n", result.errors.Take(30)));
                plan.state = "VerifiedUnsaved";
                Undo.CollapseUndoOperations(undo);
                return result;
            }
            catch { Undo.RevertAllDownToGroup(undo); plan.state = "Planned"; throw; }
        }
        public static Plan IdentityPlan(Baseline baseline) => new Plan { baselineHash = baseline.sceneHash, scope = Scope.StructureOnly, entries = baseline.objects.Select(o => new Entry { id = o.id, originalPath = o.path, parent = o.parent, name = o.name, disposition = "Protected", protectedObject = true, fixedParent = true, reason = "Identity preservation" }).ToList() };
        public static Plan BuildPlan(Baseline baseline, Scope scope) => new Classifier(baseline, scope).Build();
        sealed class Classifier
        {
            readonly Baseline baseline;
            readonly Plan plan;
            readonly Dictionary<string, Entry> entries;
            readonly Dictionary<string, ObjectRecord> records;
            readonly Dictionary<string, string> groups = new();
            readonly HashSet<string> assigned = new();
            static readonly Dictionary<string, string> Names = new()
            {
                {"SpaceNavigation","ContentNavigator"},{"SharedLifecycle","ExperienceLifecycle"},{"SharedHeadTracking","HeadTracking"},{"SharedNarration","Narration"},{"SharedDirectionalLight","DirectionalLight"},
                {"ContentMenuRuntime","ContentSelectionInput"},{"MenuStart","EntryAnchor"},{"ContentStart","EntryAnchor"},{"SmokeEntry","EntryAnchor"},{"BellApproach","BellApproachAnchor"},
                {"ContentPanel","ExperiencePanel"},{"ContentMenuPanel","ContentSelectionPanel"},{"SharedUIModal","ModalPanels"},{"ConfirmDialog","ConfirmationDialog"},
                {"Content1","FireNotificationButton"},{"SmokeContentButton","SmokePostureButton"},{"RouteContentButton","EvacuationRouteButton"},{"설정","SettingsButton"},
                {"계속하기","ResumeButton"},{"처음부터 다시","RestartButton"},{"시나리오 선택으로","ReturnToSelectionButton"},
                {"음성 줄이기","VoiceVolumeDownButton"},{"음성 키우기","VoiceVolumeUpButton"},{"효과음 줄이기","EffectsVolumeDownButton"},{"효과음 키우기","EffectsVolumeUpButton"},
                {"자막 켜기/끄기","ToggleSubtitlesButton"},{"글자 100%/150%","ToggleTextScaleButton"},{"표시 위치 재정렬","RecenterUIButton"},{"뒤로","BackButton"},{"실행","ConfirmButton"},
                {"다시 체험하기","RetryButton"},{"선택 화면으로","ReturnToSelectionButton"},{"Use","UsageText"},{"ContentConfigurationError","ConfigurationErrorText"},{"Narration","SubtitleText"},
                {"ActionButton1","Action01Button"},{"ActionButton2","Action02Button"},{"Moving button cap","ButtonCap"},{"Poke volume","PokeVolume"},{"Button surface","ButtonSurface"},{"Modeled housing","Housing"},
                {"Bystander_Dummy_1","Bystander01"},{"Bystander_Dummy_2","Bystander02"},{"Environment probes","LightProbes"},{"FixedSmokeLayer","SmokeLayer"},{"SmokeFireSource","FireSource"},
                {"A","ScenarioA"},{"B","ScenarioB"},{"West wing","WestCorridor"},{"North wing","NorthCorridor"},{"Stairwell 계단 1","Stairwell01"},{"Stairwell 계단 2","Stairwell02"},
                {"Destination","DestinationAnchor"},{"Neutral teleport marker","TeleportMarker"},{"fire-actions","FireActionsAnchor"},{"smoke-ready","SmokeReadyAnchor"},{"route-ready","RouteReadyAnchor"},
                {"route-a-left","Stairwell01ActionsAnchor"},{"route-b-left","Stairwell01ActionsAnchor"},{"route-a-lift","ElevatorActionsAnchor"},{"route-b-lift","ElevatorActionsAnchor"},{"route-a-right","Stairwell02ActionsAnchor"},{"route-b-right","Stairwell02ActionsAnchor"}
            };
            public Classifier(Baseline b, Scope scope)
            {
                baseline = b; records = b.objects.ToDictionary(o => o.id);
                plan = new Plan { scope = scope, baselineHash = b.sceneHash };
                foreach (var o in b.objects)
                {
                    var protectedObject = IsProtected(o);
                    plan.entries.Add(new Entry { id = o.id, originalPath = o.path, parent = o.parent, name = protectedObject ? o.name : Name(o), protectedObject = protectedObject, fixedParent = true, disposition = protectedObject ? "Protected" : "RenameOnly", reason = protectedObject ? "XR/Animator/external/TMP protected internals" : "Preserve internal parent; normalize project display name" });
                }
                entries = plan.entries.ToDictionary(e => e.id);
            }
            bool Has(ObjectRecord o, string type) => o.components.Any(c => c.type == type);
            bool IsProtected(ObjectRecord o)
            {
                if (Has(o, "TMPro.TMP_SubMesh") || Has(o, "TMPro.TMP_SubMeshUI")) return true;
                for (var parent = o.parent; parent != ""; parent = records[parent].parent)
                {
                    var p = records[parent];
                    if (Has(p, "Unity.XR.CoreUtils.XROrigin") || Has(p, "UnityEngine.Animator") || Has(p, "UnityEngine.EventSystems.EventSystem")) return true;
                    if (!string.IsNullOrEmpty(p.prefab) && !p.prefab.StartsWith("Assets/_02LWS/", StringComparison.Ordinal)) return true;
                }
                return false;
            }
            static string Pascal(string name) => string.Concat(Regex.Matches(name, "[A-Za-z0-9]+").Cast<Match>().Select(m => char.ToUpperInvariant(m.Value[0]) + m.Value.Substring(1)));
            string Name(ObjectRecord o)
            {
                var n = Names.TryGetValue(o.name, out var name) ? name : Pascal(o.name);
                if (o.name == "계속하기" && o.path.Contains("/ConfirmDialog/")) n = "CancelButton";
                if (o.name == "AdjacentFireSpace") n = "AdjacentFireRoom";
                if (Regex.IsMatch(o.name, "^[ab]-w[0-6]$")) n = "Waypoint0" + o.name[o.name.Length - 1];
                if ((Has(o, "TMPro.TextMeshProUGUI") || Has(o, "TMPro.TextMeshPro")) && !n.EndsWith("Text", StringComparison.Ordinal)) n += "Text";
                if (string.IsNullOrEmpty(n)) throw new InvalidOperationException("Unresolved semantic name: " + o.path);
                return n;
            }
            ObjectRecord At(string path) => baseline.objects.Single(o => o.path == path);
            ObjectRecord[] Children(ObjectRecord parent) => baseline.objects.Where(o => o.parent == parent.id).OrderBy(o => o.sibling).ToArray();
            string GroupUnder(string parent, string name)
            {
                string path = parent + "/" + name;
                if (groups.TryGetValue(path, out var found)) return found;
                var key = "group:" + path; groups.Add(path, key); plan.groups.Add(new Group { key = key, parent = parent, name = name }); return key;
            }
            string Under(string parent, string path)
            {
                foreach (var name in path.Split('/')) parent = GroupUnder(parent, name);
                return parent;
            }
            void Move(ObjectRecord o, string parent, string name = null)
            {
                if (!assigned.Add(o.id)) throw new InvalidOperationException("Duplicate role assignment: " + o.path);
                var e = entries[o.id];
                if (e.protectedObject) throw new InvalidOperationException("Cannot move protected internal: " + o.path);
                e.parent = parent; e.fixedParent = false; e.disposition = "MoveAndRename"; e.reason = "Area then role classification";
                if (name != null) e.name = name;
            }
            static string RoomRole(string name)
            {
                var n = name.ToLowerInvariant();
                if (n.Contains("luminaire") || n.Contains("diffuser") || n.Contains("illumination") || n.Contains("baked ceiling light")) return "Lighting";
                if (n.Contains("door") || n == "handle") return "Doors";
                if (n.Contains("window") && !n.Contains("wall") && !n.Contains("pier")) return "Windows";
                if (n.Contains("skirting") || n.Contains("class number")) return "Details";
                if (n.Contains("floor") || n.Contains("ceiling") || n.Contains("wall") || n.Contains("closure") || n.Contains("cornice") || n.Contains("window pier")) return "Structure";
                throw new InvalidOperationException("Unclassified room part: " + name);
            }
            static string StairRole(string name)
            {
                var n = name.ToLowerInvariant();
                if (n.Contains("step") || n.Contains("nosing")) return "Steps";
                if (n.Contains("handrail")) return "Railings";
                if (n.Contains("portal")) return "Entrance";
                if (n.Contains("sign") || n.Contains("stair id")) return "Signage";
                if (n.Contains("landing") || n.Contains("wall") || n.Contains("ceiling")) return "Structure";
                throw new InvalidOperationException("Unclassified stair part: " + name);
            }
            void Fire(ObjectRecord space)
            {
                var corridor = At(space.path + "/Corridor"); var finish = At(space.path + "/CorridorFinish");
                var environment = Under(space.id, "Environment"); Move(corridor, environment); Move(finish, corridor.id, "Structure");
                var doors = Children(corridor).Where(o => o.name == "Closed door").ToArray();
                if (doors.Length != 6) throw new InvalidOperationException("Expected six fire corridor doors");
                var doorGroups = doors.Select((o, i) => GroupUnder(Under(corridor.id, "Doors"), "Door" + (i + 1).ToString("00"))).ToArray();
                foreach (var o in Children(corridor))
                {
                    if (o.name == "FireVisual") Move(o, Under(space.id, "Effects"));
                    else if (o.name == "Closed door") Move(o, doorGroups[Array.IndexOf(doors, o)]);
                    else Move(o, finish.id);
                }
                foreach (var o in Children(finish))
                {
                    if (o.name.StartsWith("Door ", StringComparison.Ordinal))
                    {
                        var index = Enumerable.Range(0, doors.Length).OrderBy(i => Vector3.SqrMagnitude(o.position - doors[i].position)).ThenBy(i => i).First();
                        Move(o, Under(doorGroups[index], "Details"));
                    }
                    else if (RoomRole(o.name) == "Lighting") Move(o, Under(corridor.id, "Lighting"));
                }
                foreach (var o in Children(space))
                {
                    if (o.id == corridor.id || o.id == finish.id) continue;
                    var role = o.name switch
                    {
                        "TeleportSurface" or "BellApproach" => "Navigation",
                        "FireNotificationRuntime" => "Logic",
                        "FireAlarm" or "AlarmLabel" => "Interactables",
                        "Bystander_Dummy_1" or "Bystander_Dummy_2" => "Characters",
                        "ContentStart" or "FloorButtonAnchors" => "Anchors",
                        "Environment probes" => "Environment",
                        _ => throw new InvalidOperationException("Unknown fire root: " + o.path)
                    };
                    Move(o, Under(space.id, role));
                }
            }
            void Smoke(ObjectRecord space)
            {
                var room = Under(space.id, "Environment/SmokeRoom"); var finish = At(space.path + "/SmokeRoomFinish");
                Move(finish, room, "Structure");
                foreach (var o in Children(finish))
                {
                    var role = RoomRole(o.name);
                    if (role != "Structure" && role != "Details") Move(o, Under(room, role));
                }
                foreach (var o in Children(space))
                {
                    if (o.id == finish.id) continue;
                    if (o.name == "Floor" || o.name == "Ceiling" || o.name == "LeftWall" || o.name == "RightWall") Move(o, finish.id);
                    else if (o.name == "AdjacentFireSpace")
                    {
                        Move(o, Under(space.id, "Environment"));
                        foreach (var part in Children(o)) Move(part, part.name == "SmokeFireSource" ? Under(space.id, "Effects") : Under(o.id, "Structure"));
                    }
                    else if (o.name == "FixedSmokeLayer") Move(o, Under(space.id, "Effects"));
                    else if (o.name == "SmokeEntry" || o.name == "FloorButtonAnchors") Move(o, Under(space.id, "Anchors"));
                    else if (o.name == "Environment probes") Move(o, Under(space.id, "Environment"));
                    else throw new InvalidOperationException("Unknown smoke root: " + o.path);
                }
            }
            void CorridorParts(ObjectRecord[] parts, string corridor, int expected)
            {
                int classroom = 0, skirt = 0; string current = null;
                var doors = new List<(ObjectRecord door, string classroom)>();
                foreach (var o in parts)
                {
                    if (o.name == "Classroom wall pier") { if (current != null) throw new InvalidOperationException("Unterminated classroom"); classroom++; current = Under(corridor, "Classrooms/Classroom" + classroom.ToString("00")); skirt = 0; }
                    if (current != null)
                    {
                        var role = RoomRole(o.name);
                        Move(o, Under(current, role));
                        if (o.name == "Recessed door leaf") doors.Add((o, current));
                        if (o.name == "Skirting" && ++skirt == 2) current = null;
                    }
                    else if (o.name == "Door transom backing")
                    {
                        if (doors.Count == 0) throw new InvalidOperationException("Transom with no classroom");
                        var nearest = doors.OrderBy(d => Vector3.SqrMagnitude(o.position - d.door.position)).First();
                        Move(o, Under(nearest.classroom, "Doors"));
                    }
                    else Move(o, Under(corridor, RoomRole(o.name)));
                }
                if (current != null || classroom != expected || doors.Count != expected) throw new InvalidOperationException("Unexpected classroom block count: " + classroom + " expected " + expected);
            }
            void Route(ObjectRecord route)
            {
                foreach (var scenario in Children(route))
                {
                    if (scenario.name != "A" && scenario.name != "B") throw new InvalidOperationException("Unknown route scenario");
                    var corridorParts = new List<ObjectRecord>();
                    foreach (var o in Children(scenario))
                    {
                        if (o.name.StartsWith("Stairwell ", StringComparison.Ordinal))
                        {
                            Move(o, Under(scenario.id, "Environment"));
                            foreach (var p in Children(o)) Move(p, Under(o.id, StairRole(p.name)));
                        }
                        else if (o.name == "Elevator")
                        {
                            Move(o, Under(scenario.id, "Environment"));
                            foreach (var p in Children(o))
                            {
                                bool text = Has(p, "TMPro.TextMeshPro");
                                var role = p.name == "Lift door" ? "Doors" : text || p.name.Contains("panel") || p.name.Contains("indicator") ? "Details" : "Structure";
                                Move(p, Under(o.id, role));
                            }
                        }
                        else if (Regex.IsMatch(o.name, "^[ab]-w[0-6]$")) Move(o, Under(scenario.id, "Navigation"));
                        else if (o.name == "FloorButtonAnchors") Move(o, Under(scenario.id, "Anchors"));
                        else if (o.name == "Blocked stair smoke") Move(o, Under(scenario.id, "Effects"));
                        else if (o.name == "West wing" || o.name == "North wing")
                        {
                            Move(o, Under(scenario.id, "Environment")); CorridorParts(Children(o), o.id, 2);
                        }
                        else if (o.name.StartsWith("Corner ", StringComparison.Ordinal)) Move(o, Under(scenario.id, "Environment/Corner/Structure"));
                        else if (scenario.name == "A") corridorParts.Add(o);
                        else throw new InvalidOperationException("Unknown B part: " + o.path);
                    }
                    if (scenario.name == "A") CorridorParts(corridorParts.ToArray(), Under(scenario.id, "Environment/Corridor"), 6);
                }
            }
            void NumberNames()
            {
                foreach (var siblings in plan.entries.GroupBy(e => e.parent))
                {
                    foreach (var repeated in siblings.GroupBy(e => e.name).Where(g => g.Count() > 1))
                    {
                        if (repeated.Any(e => e.protectedObject)) throw new InvalidOperationException("Protected name collision: " + repeated.Key);
                        int i = 1;
                        foreach (var e in repeated)
                        {
                            var suffix = new[] { "Button", "Text", "Anchor" }.FirstOrDefault(s => e.name.EndsWith(s, StringComparison.Ordinal)) ?? "";
                            e.name = e.name.Substring(0, e.name.Length - suffix.Length) + (i++).ToString("00") + suffix;
                        }
                    }
                }
            }
            public Plan Build()
            {
                // Recapture of a completed scene still checks the final contracts before returning identity.
                if (baseline.objects.Any(o => o.path == "SharedSystems/UI/SharedCanvas"))
                {
                    if (SceneHierarchyOrganizerChecks.CheckStructure(SceneManager.GetActiveScene()) != "" || SceneHierarchyOrganizerChecks.CheckNames(SceneManager.GetActiveScene()) != "") throw new InvalidOperationException("Existing organized scene violates contracts");
                    foreach (var e in plan.entries) { e.name = records[e.id].name; e.protectedObject = true; e.disposition = "Protected"; }
                    return plan;
                }
                var shared = At("SharedSystems");
                foreach (var o in Children(shared))
                {
                    var group = o.name switch {
                        "LwsXRStarterRig" => "Player", "SpaceNavigation" => "Navigation", "LwsEventSystem" => "Input", "SharedNarration" => "Audio", "SharedDirectionalLight" => "Lighting", "SharedCanvas" or "SharedFloorButtons" => "UI", "SharedLifecycle" or "SharedHeadTracking" => "", _ => throw new InvalidOperationException("Unknown shared system: " + o.path)
                    };
                    if (group != "") Move(o, Under(shared.id, group));
                }
                var menu = At("Contents/MenuSpace");
                foreach (var o in Children(menu)) Move(o, Under(menu.id, o.name == "ContentMenuRuntime" ? "Logic" : o.name == "MenuStart" ? "Anchors" : throw new InvalidOperationException("Unknown menu object")));
                Fire(At("Contents/FireNotificationSpace")); Smoke(At("Contents/SmokePostureSpace")); Route(At("Contents/RouteSpace"));
                if (plan.scope == Scope.NamesOnly)
                {
                    plan.groups.Clear();
                    foreach (var e in plan.entries) { e.parent = records[e.id].parent; e.fixedParent = true; e.disposition = e.protectedObject ? "Protected" : "RenameOnly"; }
                }
                if (plan.scope == Scope.StructureOnly) foreach (var e in plan.entries) e.name = records[e.id].name;
                else NumberNames();
                var roles = new[] { "Player", "Navigation", "Input", "Logic", "Environment", "Interactables", "Characters", "Effects", "Anchors", "Audio", "Lighting", "UI", "Structure", "Doors", "Windows", "Steps", "Railings", "Entrance", "Signage", "Details" };
                var originalGroups = plan.groups.ToArray();
                plan.groups = originalGroups.OrderBy(g => g.key.Count(c => c == '/'))
                    .ThenBy(g => Array.FindIndex(originalGroups, other => other.parent == g.parent))
                    .ThenBy(g => { var rank = Array.IndexOf(roles, g.name); return rank < 0 ? roles.Length : rank; }).ToList();
                return plan;
            }
        }
        [MenuItem("Tools/LWS/Hierarchy/Dry Run")]
        static void DryRunMenu()
        {
            int selected = EditorUtility.DisplayDialogComplex("Hierarchy scope", "Choose validation scope", "Full", "StructureOnly", "NamesOnly");
            DryRun(selected == 0 ? Scope.Full : selected == 1 ? Scope.StructureOnly : Scope.NamesOnly);
        }
        public static Plan DryRun(Scope scope = Scope.Full)
        {
            var scene = Context(false); var baseline = Read<Baseline>("baseline.json");
            if (File.Exists(FilePath("organization-plan.json")))
            {
                var previous = Read<Plan>("organization-plan.json");
                if (TryUseAppliedPlan(baseline, previous, scope, scene))
                {
                    Debug.Log("NoChanges"); return previous;
                }
            }
            var identity = CompareScene(baseline, IdentityPlan(baseline), scene);
            if (identity.errors.Count != 0) throw new InvalidOperationException("Unapproved baseline differences: " + string.Join("\n", identity.errors.Take(10)));
            var plan = BuildPlan(baseline, scope); ValidatePlan(baseline, plan); Write("organization-plan.json", plan);
            return plan;
        }
        public static bool TryUseAppliedPlan(Baseline baseline, Plan previous, Scope requested, Scene scene)
        {
            if (previous.state != "VerifiedUnsaved" && previous.state != "Saved" && previous.state != "ReopenedVerified") return false;
            if (previous.state == "VerifiedUnsaved" && CompareScene(baseline, IdentityPlan(baseline), scene).errors.Count == 0)
            {
                previous.state = "Planned";
                return false;
            }
            var check = CompareScene(baseline, previous, scene);
            if (check.errors.Count != 0) throw new InvalidOperationException(string.Join("\n", check.errors.Take(20)));
            if (requested != previous.scope) throw new InvalidOperationException("Undo the partial application before changing scope");
            return true;
        }
        [MenuItem("Tools/LWS/Hierarchy/Apply Organization")]
        public static void ApplyOrganization()
        {
            var scene = Context(true); var baseline = Read<Baseline>("baseline.json"); var plan = Read<Plan>("organization-plan.json");
            if (Hash(scene.path) != baseline.sceneHash || baseline.sources.Any(s => Hash(s.path) != s.hash)) throw new InvalidOperationException("Stale scene/prefab baseline");
            var result = ApplyToScene(scene, baseline, plan); Write("organization-plan.json", plan); Write("comparison.json", result);
        }
        [MenuItem("Tools/LWS/Hierarchy/Verify Organization")]
        public static Comparison VerifyOrganization()
        {
            var scene = Context(false); var baseline = Read<Baseline>("baseline.json"); var plan = Read<Plan>("organization-plan.json");
            var result = CompareScene(baseline, plan, scene); Write("comparison.json", result);
            if (result.errors.Count != 0) throw new InvalidOperationException(string.Join("\n", result.errors.Take(30)));
            if (!scene.isDirty && plan.state == "Saved" && (scene.handle != plan.savedSceneHandle || System.Diagnostics.Process.GetCurrentProcess().Id != plan.savedProcessId)) { plan.state = "ReopenedVerified"; Write("organization-plan.json", plan); }
            File.WriteAllLines(FilePath("hierarchy-after.txt"), Transforms(scene).Select(HierarchyPath));
            return result;
        }
        public static void SaveVerified()
        {
            var plan = Read<Plan>("organization-plan.json");
            ValidateSave(plan);
            VerifyOrganization();
            var scene = Context(false);
            if (!EditorSceneManager.SaveScene(scene)) throw new InvalidOperationException("Scene save did not succeed");
            plan.state = "Saved"; plan.savedSceneHandle = scene.handle; plan.savedProcessId = System.Diagnostics.Process.GetCurrentProcess().Id; Write("organization-plan.json", plan);
        }
        public static void ValidateSave(Plan plan)
        {
            if (plan.scope != Scope.Full || plan.state != "VerifiedUnsaved") throw new InvalidOperationException("Only verified Full applications may be saved");
        }
        public static void ConfirmUndo()
        {
            var baseline = Read<Baseline>("baseline.json"); var result = CompareScene(baseline, IdentityPlan(baseline), Context(false));
            if (result.errors.Count != 0) throw new InvalidOperationException("Undo did not restore baseline: " + string.Join("\n", result.errors.Take(10)));
            var plan = Read<Plan>("organization-plan.json"); plan.state = "Planned"; Write("organization-plan.json", plan);
        }
    }
}
#endif
