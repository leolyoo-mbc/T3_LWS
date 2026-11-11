using System;
using System.Collections.Generic;
using UnityEngine;
using LWS.FireNotification.Interaction;
using LWS.FireNotification.Presentation;

namespace LWS.FireNotification.Runtime
{
    public static class SharedUIContractAudit
    {
        public static List<string> Validate(ContentSceneNavigator navigator)
        {
            var errors = new List<string>();
            if (navigator == null) { errors.Add("Missing navigator"); return errors; }
            var scene = navigator.gameObject.scene;
            int canvases = 0, voices = 0, followers = 0;
            foreach (var root in scene.GetRootGameObjects())
            {
                canvases += root.GetComponentsInChildren<Canvas>(true).Length;
                voices += root.GetComponentsInChildren<GuidancePresenter>(true).Length;
                followers += root.GetComponentsInChildren<ComfortFollowUI>(true).Length;
            }
            if (canvases != 1) errors.Add("Expected 1 shared Canvas, found " + canvases);
            if (voices != 1) errors.Add("Expected 1 shared guidance presenter, found " + voices);
            if (followers != 1) errors.Add("Expected 1 shared UI follower, found " + followers);
            var view = navigator.contentUI;
            if (view == null || view.panel == null || view.title == null || view.status == null ||
                view.guidance == null || view.guidance.voice == null || view.guidance.subtitle == null ||
                view.actionRoot == null || view.actionTemplate == null)
                errors.Add("Missing shared content UI references");
            if (navigator.lifecycle == null) errors.Add("Missing shared lifecycle");
            if (navigator.menuRoot != null && navigator.menuRoot.GetComponentsInChildren<Canvas>(true).Length != 0)
                errors.Add("Menu space owns a Canvas");
            foreach (var entry in navigator.contents)
            {
                if (entry == null || entry.root == null || entry.adapter == null) { errors.Add("Missing content registration"); continue; }
                if (entry.adapter.view != view || view == null) errors.Add(entry.id + ": use the shared content view");
                if (entry.root.GetComponentsInChildren<Canvas>(true).Length != 0) errors.Add(entry.id + ": content owns a Canvas");
                if (entry.root.GetComponentsInChildren<GuidancePresenter>(true).Length != 0) errors.Add(entry.id + ": content owns a guidance presenter");
                if (entry.root.GetComponentsInChildren<NextInput>(true).Length != 0 ||
                    entry.root.GetComponentsInChildren<ActionChoiceInput>(true).Length != 0 ||
                    entry.root.GetComponentsInChildren<ReturnToMenuInput>(true).Length != 0 ||
                    entry.root.GetComponentsInChildren<FreshPressButton>(true).Length != 0)
                    errors.Add(entry.id + ": content owns legacy UI input");
                if (entry.root.GetComponentsInChildren<HeadPoseSampleProvider>(true).Length != 0)
                    errors.Add(entry.id + ": move tracking provider to SharedSystems");
                if (view != null && (view.transform.IsChildOf(entry.root.transform) || view.guidance != null && view.guidance.transform.IsChildOf(entry.root.transform)))
                    errors.Add(entry.id + ": shared presentation lives inside content");
            }
            return errors;
        }
        public static void CheckScene()
        {
            var navigator = UnityEngine.Object.FindFirstObjectByType<ContentSceneNavigator>(FindObjectsInactive.Include);
            var errors = Validate(navigator);
            if (errors.Count != 0) throw new InvalidOperationException(string.Join("\n", errors));
            Debug.Log("Shared UI contract PASS: one Canvas, one narrator, content UI duplication zero");
        }
#if UNITY_EDITOR
        [UnityEditor.MenuItem("Tools/LWS/Validate Shared UI")]
        private static void MenuCheck() => CheckScene();
#endif
    }
}
