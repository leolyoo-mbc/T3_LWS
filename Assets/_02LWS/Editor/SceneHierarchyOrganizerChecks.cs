#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using static LWS.FireNotification.Editor.SceneHierarchyOrganizer;
using Group = LWS.FireNotification.Editor.SceneHierarchyOrganizer.Group;

namespace LWS.FireNotification.Editor
{
    public static class SceneHierarchyOrganizerChecks
    {
        static void Require(bool condition, string message)
        {
            if (!condition) throw new InvalidOperationException(message);
        }
        static void Reject(Action action, string label, List<string> results)
        {
            bool rejected = false;
            try { action(); } catch (InvalidOperationException) { rejected = true; }
            Require(rejected, "Accepted invalid operation: " + label);
            results.Add("PASS reject " + label);
        }
        public static string RunSafety()
        {
            var results = new List<string>();
            results.Add(RunCaptureDirectoryCheck());
            Reject(() => Guard("Assets/Other.unity", false, false, false, false), "wrong scene", results);
            Reject(() => Guard(ScenePath, true, false, false, false), "Play Mode", results);
            Reject(() => Guard(ScenePath, false, true, false, false), "compiling", results);
            Reject(() => Guard(ScenePath, false, false, true, true), "dirty write", results);
            Guard(ScenePath, false, false, true, false); results.Add("PASS readonly dirty context");
            var active = SceneManager.GetActiveScene();
            const string fixturePath = "Assets/_02LWS/Editor/HierarchyOrganizerFixture.unity";
            if (File.Exists(fixturePath)) throw new InvalidOperationException("Fixture path occupied; refusing overwrite");
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Additive);
            try
            {
                SceneManager.SetActiveScene(scene);
                var root = new GameObject("FixtureRoot");
                var first = new GameObject("First"); first.transform.SetParent(root.transform);
                var second = new GameObject("Second"); second.transform.SetParent(root.transform);
                var light = first.AddComponent<Light>();
                var joint = first.AddComponent<FixedJoint>(); joint.connectedBody = second.AddComponent<Rigidbody>();
                var rectGO = new GameObject("Panel", typeof(RectTransform)); rectGO.transform.SetParent(root.transform);
                var rect = (RectTransform)rectGO.transform; rect.sizeDelta = new Vector2(120, 90);
                var rotated = new GameObject("RotatedParent"); rotated.transform.SetParent(root.transform); rotated.transform.localRotation = Quaternion.Euler(0, 0, 45); rotated.transform.localScale = new Vector3(2, 1, 1);
                EditorSceneManager.SaveScene(scene, fixturePath);
                var baseline = JsonUtility.FromJson<Baseline>(JsonUtility.ToJson(CaptureScene(scene)));
                var identity = IdentityPlan(baseline);
                Require(CompareScene(baseline, identity, scene).errors.Count == 0, "Identity snapshot differs"); results.Add("PASS identity snapshot");
                Plan Copy() => JsonUtility.FromJson<Plan>(JsonUtility.ToJson(identity));
                var missing = Copy(); missing.entries.RemoveAt(0); Reject(() => ValidatePlan(baseline, missing), "missing classification", results);
                var duplicate = Copy(); duplicate.entries.Add(duplicate.entries[0]); Reject(() => ValidatePlan(baseline, duplicate), "duplicate classification", results);
                var stale = Copy(); stale.baselineHash = "stale"; Reject(() => ValidatePlan(baseline, stale), "stale manifest", results);
                var protectedPlan = Copy(); protectedPlan.entries[1].name = "Changed"; Reject(() => ValidatePlan(baseline, protectedPlan), "protected name", results);
                var wrongParent = Copy(); wrongParent.entries[1].parent = ""; Reject(() => ValidatePlan(baseline, wrongParent), "protected parent", results);
                var scope = Copy(); scope.scope = Scope.NamesOnly; scope.entries[1].fixedParent = false; scope.entries[1].parent = ""; Reject(() => ValidatePlan(baseline, scope), "scope mismatch", results);
                Reject(() => ValidateSave(new Plan { scope = Scope.NamesOnly, state = "VerifiedUnsaved" }), "NamesOnly save", results);
                Reject(() => ValidateSave(new Plan { scope = Scope.StructureOnly, state = "VerifiedUnsaved" }), "StructureOnly save", results);
                Reject(() => ValidateSave(new Plan { scope = Scope.Full, state = "AppliedUnsaved" }), "unverified Full save", results);
                void Difference(Action mutate, Action restore, string label)
                {
                    mutate(); Require(CompareScene(baseline, identity, scene).errors.Count > 0, "Missed " + label); restore();
                    Require(CompareScene(baseline, identity, scene).errors.Count == 0, "Fixture restore failed: " + label); results.Add("PASS detect " + label);
                }
                Difference(() => joint.connectedBody = null, () => joint.connectedBody = second.GetComponent<Rigidbody>(), "reference loss");
                float intensity = light.intensity;
                Difference(() => light.intensity = intensity + 1, () => light.intensity = intensity, "serialized setting change");
                Difference(() => first.SetActive(false), () => first.SetActive(true), "active state change");
                Difference(() => first.transform.localPosition = Vector3.one, () => first.transform.localPosition = Vector3.zero, "world transform change");
                Difference(() => rect.sizeDelta = Vector2.one, () => rect.sizeDelta = new Vector2(120, 90), "RectTransform layout change");
                Difference(() => rect.SetParent(first.transform), () => { rect.SetParent(root.transform); rect.SetSiblingIndex(2); }, "UI parent change");
                Difference(() => second.transform.SetSiblingIndex(0), () => second.transform.SetSiblingIndex(1), "child order change");
                BoxCollider extra = null;
                Difference(() => extra = first.AddComponent<BoxCollider>(), () => UnityEngine.Object.DestroyImmediate(extra), "unexpected component");
                Undo.IncrementCurrentGroup(); int removeGroup = Undo.GetCurrentGroup();
                Undo.DestroyObjectImmediate(light);
                Require(CompareScene(baseline, identity, scene).errors.Count > 0, "Missed component removal");
                Undo.RevertAllDownToGroup(removeGroup); results.Add("PASS detect existing component removal");
                var plan = Copy();
                plan.groups.Add(new Group { key = "group:Fixture", parent = Id(root), name = "Group" });
                var moved = plan.entries.Single(e => e.id == Id(second)); moved.fixedParent = false; moved.protectedObject = false; moved.disposition = "MoveAndRename"; moved.parent = "group:Fixture";
                EditorSceneManager.SaveScene(scene, fixturePath);
                ApplyToScene(scene, baseline, plan);
                Require(scene.isDirty, "Apply saved scene unexpectedly");
                Require(CompareScene(baseline, plan, scene).errors.Count == 0, "Approved Transform growth failed"); results.Add("PASS approved group Transform count and preserved IDs");
                Undo.PerformUndo();
                Require(CompareScene(baseline, identity, scene).errors.Count == 0, "Undo did not restore IDs/properties"); results.Add("PASS one-step Undo restores baseline");
                Require(!TryUseAppliedPlan(baseline, plan, Scope.NamesOnly, scene) && plan.state == "Planned", "Undo must permit the next DryRun scope without ConfirmUndo"); results.Add("PASS ordinary Undo automatically unlocks next DryRun scope");
                var names = Copy(); names.scope = Scope.NamesOnly;
                var renamed = names.entries.Single(e => e.id == Id(first)); renamed.protectedObject = false; renamed.name = "RenamedFirst"; renamed.disposition = "RenameOnly";
                ApplyToScene(scene, baseline, names);
                Require(CompareScene(baseline, names, scene).errors.Count == 0, "NamesOnly changed more than the name");
                Undo.PerformUndo(); Require(CompareScene(baseline, identity, scene).errors.Count == 0, "Name Undo did not restore baseline"); results.Add("PASS NamesOnly rename and Undo preserve serialized strings");
                var shear = Copy();
                var panelEntry = shear.entries.Single(e => e.id == Id(rectGO)); panelEntry.fixedParent = false; panelEntry.disposition = "MoveAndRename"; panelEntry.parent = Id(rotated);
                Reject(() => ApplyToScene(scene, baseline, shear), "post-mutation UI deformation", results);
                Require(CompareScene(baseline, identity, scene).errors.Count == 0, "Failed apply did not roll back the fixture"); results.Add("PASS failed apply automatically rolls back IDs, layout and parents");
                var failed = Copy(); failed.groups.Add(new Group { key = "bad", parent = Id(root), name = "Group" }); failed.entries.Single(e => e.id == Id(second)).fixedParent = false; failed.entries.Single(e => e.id == Id(second)).parent = "bad";
                // An invalid classified target must be rejected before any scene mutation.
                failed.entries.Single(e => e.id == Id(second)).parent = "missing";
                Reject(() => ApplyToScene(scene, baseline, failed), "invalid apply", results);
                Require(CompareScene(baseline, identity, scene).errors.Count == 0, "Rejected apply changed scene");
                var text = string.Join("\n", results);
                File.WriteAllText(Path.Combine(RunDirectory, "organizer-safety.txt"), text);
                return text;
            }
            finally
            {
                SceneManager.SetActiveScene(active);
                EditorSceneManager.CloseScene(scene, true);
                AssetDatabase.DeleteAsset(fixturePath);
            }
        }
        public static string RunCaptureDirectoryCheck()
        {
            var occupied = Path.Combine(RunDirectory, "capture-directory-fixture"); Directory.CreateDirectory(occupied);
            var baseline = Path.Combine(occupied, "baseline.json"); File.WriteAllText(baseline, "preserve-evidence");
            var next = CaptureDirectory(occupied);
            Require(Path.GetFullPath(next) != Path.GetFullPath(occupied), "A fresh baseline must select a new run");
            Require(File.ReadAllText(baseline) == "preserve-evidence", "Previous evidence was overwritten");
            Directory.CreateDirectory(next); File.WriteAllText(Path.Combine(next, "baseline.json"), "collision-fixture");
            try { Require(Path.GetFullPath(CaptureDirectory(next)) != Path.GetFullPath(next), "Timestamp collision must select another run"); }
            finally { File.Delete(Path.Combine(next, "baseline.json")); Directory.Delete(next); }
            return "PASS fresh capture and timestamp collision preserve old evidence";
        }
        public static string CheckNames(Scene scene)
        {
            var errors = new List<string>();
            foreach (var t in Transforms(scene))
            {
                if (Protected(t)) continue;
                if (!Regex.IsMatch(t.name, "^[A-Z][A-Za-z0-9]*$")) errors.Add("PascalCase: " + HierarchyPath(t));
                if ((t.GetComponent<UnityEngine.UI.Button>() != null || t.GetComponent<LWS.FireNotification.Interaction.FreshPressButton>() != null) && !t.name.EndsWith("Button", StringComparison.Ordinal)) errors.Add("Button suffix: " + HierarchyPath(t));
                if (t.GetComponent<TMPro.TMP_Text>() != null && !t.name.EndsWith("Text", StringComparison.Ordinal)) errors.Add("Text suffix: " + HierarchyPath(t));
                if (t.GetComponent<LWS.FireNotification.Runtime.FloorButtonAnchor>() != null && !t.name.EndsWith("Anchor", StringComparison.Ordinal)) errors.Add("Anchor suffix: " + HierarchyPath(t));
                if (t.parent != null && t.parent.Cast<Transform>().Count(c => c.name == t.name) > 1) errors.Add("Sibling duplicate: " + HierarchyPath(t));
            }
            return string.Join("\n", errors);
        }
        public static bool Protected(Transform t)
        {
            for (var p = t.parent; p != null; p = p.parent)
                if (p.GetComponent<Unity.XR.CoreUtils.XROrigin>() != null || p.GetComponent<Animator>() != null || p.GetComponent<UnityEngine.EventSystems.EventSystem>() != null) return true;
            return t.GetComponent<TMPro.TMP_SubMesh>() != null || t.GetComponent<TMPro.TMP_SubMeshUI>() != null;
        }
        public static string CheckStructure(Scene scene)
        {
            var paths = new HashSet<string>(Transforms(scene).Select(HierarchyPath));
            var required = new[] {
                "SharedSystems/Player/LwsXRStarterRig", "SharedSystems/Navigation/ContentNavigator", "SharedSystems/Input/LwsEventSystem", "SharedSystems/Audio/Narration", "SharedSystems/Lighting/DirectionalLight", "SharedSystems/UI/SharedCanvas", "SharedSystems/UI/SharedFloorButtons/FloorCanvas",
                "Contents/FireNotificationSpace/Interactables/FireAlarm", "Contents/FireNotificationSpace/Characters/Bystander01", "Contents/FireNotificationSpace/Characters/Bystander02", "Contents/SmokePostureSpace/Effects/SmokeLayer",
                "Contents/FireNotificationSpace/Anchors/FloorButtonAnchors", "Contents/SmokePostureSpace/Anchors/FloorButtonAnchors",
                "Contents/RouteSpace/ScenarioA/Environment/Stairwell01", "Contents/RouteSpace/ScenarioA/Environment/Stairwell02", "Contents/RouteSpace/ScenarioB/Environment/Stairwell01", "Contents/RouteSpace/ScenarioB/Environment/Stairwell02",
                "Contents/RouteSpace/ScenarioA/Navigation", "Contents/RouteSpace/ScenarioB/Navigation", "Contents/RouteSpace/ScenarioA/Anchors/FloorButtonAnchors", "Contents/RouteSpace/ScenarioB/Anchors/FloorButtonAnchors"
            };
            return string.Join("\n", required.Where(p => !paths.Contains(p)).Select(p => "Missing path: " + p));
        }
        public static string CheckSharedUI(Scene scene)
        {
            var navigator = Transforms(scene).Select(t => t.GetComponent<LWS.FireNotification.Runtime.ContentSceneNavigator>()).FirstOrDefault(n => n != null);
            if (navigator == null) return "Missing scene navigator";
            return string.Join("\n", LWS.FireNotification.Runtime.SharedUIContractAudit.Validate(navigator));
        }
    }
}
#endif
