using System;
using System.Collections;
using UnityEngine;
using Unity.XR.CoreUtils;

namespace LWS.FireNotification.Runtime
{
    [Serializable]
    public sealed class ContentEntry
    {
        public string id;
        public GameObject root;
        public Transform anchor;
        public ContentExperienceAdapter adapter;
        public bool locomotionAllowed = true;
    }
    public sealed class ContentSceneNavigator : MonoBehaviour
    {
        public XROrigin origin;
        public GameObject menuRoot, contentRoot;
        public Transform menuAnchor, contentAnchor;
        public FireNotificationController controller; // Existing scene/tool compatibility.
        public TMPro.TMP_Text configurationErrorText;
        public ContentEntry[] contents = Array.Empty<ContentEntry>();
        public ContentEntry ActiveContent { get; private set; }
        public ContentExperienceAdapter ActiveAdapter => ActiveContent?.adapter;
        public event Action ExitConfirmationRequested;
        public bool Loading { get; private set; }
        public bool InContent => ActiveContent != null;
        public bool MovementAllowed => !Loading && controller != null && controller.lifecycle != null && !controller.lifecycle.Blocked &&
            (ActiveContent == null || (ActiveContent.locomotionAllowed && ActiveAdapter != null && !ActiveAdapter.Blocked));
        public string ConfigurationError { get; private set; }
        private void Awake()
        {
            if (contents.Length != 0) return;
            var adapter = contentRoot.GetComponent<FireNotificationAdapter>();
            if (adapter == null) adapter = contentRoot.AddComponent<FireNotificationAdapter>();
            adapter.controller = controller;
            contents = new[] { new ContentEntry { id="fire-notification", root=contentRoot, anchor=contentAnchor, adapter=adapter } };
        }
        private void Start()
        {
            foreach (var entry in contents) if (entry?.root != null) entry.root.SetActive(false);
            menuRoot.SetActive(true);
            MoveTo(menuAnchor);
        }
        public void StartContent() => StartContent("fire-notification");
        public void StartContent(string id)
        {
            if (Loading || InContent) return;
            var seen = new System.Collections.Generic.HashSet<string>();
            ContentEntry chosen = null;
            foreach (var entry in contents)
            {
                if (entry == null || string.IsNullOrWhiteSpace(entry.id) || !seen.Add(entry.id) || entry.root == null || entry.anchor == null || entry.adapter == null)
                { SetError("콘텐츠 연결을 확인할 수 없습니다. 선택 화면에서 다시 시도해 주세요."); return; }
                if (entry.id == id) chosen = entry;
            }
            if (chosen == null) { SetError("선택한 콘텐츠가 준비되지 않았습니다."); return; }
            SetError(null);
            StartCoroutine(Navigate(chosen));
        }
        public void LoadMenu() { if (!Loading && InContent) StartCoroutine(Navigate(null)); }
        private void RelayExit() => ExitConfirmationRequested?.Invoke();
        private void SetError(string message) { ConfigurationError=message;if(configurationErrorText!=null)configurationErrorText.text=message??""; }
        private IEnumerator Navigate(ContentEntry next)
        {
            Loading = true;
            yield return null; // Finish UI event before disabling its hierarchy.
            try
            {
            if (ActiveAdapter != null)
            {
                ActiveAdapter.MenuRequested -= LoadMenu;
                ActiveAdapter.ExitConfirmationRequested -= RelayExit;
                ActiveAdapter.Stop();
            }
            foreach (var provider in origin.GetComponentsInChildren<Interaction.MenuTeleportationProvider>(true)) provider.CancelPending();
            menuRoot.SetActive(false);
            foreach (var entry in contents) entry.root.SetActive(false);
            ActiveContent = next;
            MoveTo(next == null ? menuAnchor : next.anchor);
            if (next == null) menuRoot.SetActive(true);
            else
            {
                next.root.SetActive(true);
                next.adapter.MenuRequested += LoadMenu;
                next.adapter.ExitConfirmationRequested += RelayExit;
                next.adapter.Begin();
            }
            }
            catch (Exception error)
            {
                var failed=ActiveAdapter;
                if(failed!=null)
                {
                    failed.MenuRequested-=LoadMenu;failed.ExitConfirmationRequested-=RelayExit;
                    try { failed.Stop(); } catch(Exception cleanup) { Debug.LogWarning("Content cleanup: "+cleanup.Message); }
                }
                ActiveContent=null;
                foreach(var entry in contents)if(entry?.root!=null)entry.root.SetActive(false);
                menuRoot.SetActive(true);MoveTo(menuAnchor);
                SetError("체험을 시작할 수 없습니다. 콘텐츠 선택 화면에서 다시 시도해 주세요.");
                Debug.LogWarning("Content entry failed: "+error.Message);
            }
            finally { Loading=false; }
            var ui = SharedUIController.Active;
            if (ui != null) ui.ApplySettings();
        }
        private void MoveTo(Transform anchor)
        {
            origin.MatchOriginUpCameraForward(Vector3.up, anchor.forward);
            origin.MoveCameraToWorldLocation(anchor.position + Vector3.up * origin.CameraInOriginSpaceHeight);
        }
        private void OnDisable()
        {
            if (ActiveAdapter != null)
            {
                ActiveAdapter.MenuRequested -= LoadMenu;
                ActiveAdapter.ExitConfirmationRequested -= RelayExit;
            }
        }
    }
}
