using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit.Locomotion.Teleportation;
using UnityEngine.XR.Interaction.Toolkit.UI;
using TMPro;
using LWS.FireNotification.Domain;
using LWS.FireNotification.Presentation;
using LWS.FireNotification.Interaction;

namespace LWS.FireNotification.Runtime
{
    // UI owns menu state; the existing controller owns education and lifecycle.
    public sealed class SharedUIController : MonoBehaviour
    {
        public static SharedUIController Active { get; private set; }
        public ContentSceneNavigator navigator;
        public UITheme theme;
        public GameObject modalRoot, pausePanel, settingsPanel, confirmPanel, resultPanel, openMenuPanel;
        public TMP_Text confirmText, settingsText, resultText, inputText;
        public FreshPressButton cancelButton, executeButton;
        public CanvasGroup contentGroup;
        public GameObject menuPanel;
        public SharedContentUI contentUI;
        public ComfortFollowUI follow;
        public UnityEngine.UI.GraphicRaycaster desktopRaycaster;
        public Transform head;
        public bool IsOpen { get; private set; }
        public float VoiceVolume { get; private set; } = 1;
        public float EffectsVolume { get; private set; } = 1;
        public bool SubtitlesEnabled { get; private set; } = true;
        public float TextScale { get; private set; } = 1;
        private enum Page { Pause, Settings, Restart, Exit }
        private Page page;
        private bool settingsFromMenu;
        private TeleportationProvider[] teleporters;
        private ContentExperienceAdapter Controller => navigator.ActiveAdapter;
        private readonly Dictionary<UnityEngine.XR.Interaction.Toolkit.Locomotion.LocomotionProvider, bool> movementDefaults = new Dictionary<UnityEngine.XR.Interaction.Toolkit.Locomotion.LocomotionProvider, bool>();

        private void Awake()
        {
            Active = this;
            teleporters = navigator.origin.GetComponentsInChildren<TeleportationProvider>(true);
            foreach (var provider in navigator.origin.GetComponentsInChildren<UnityEngine.XR.Interaction.Toolkit.Locomotion.LocomotionProvider>(true)) movementDefaults[provider] = provider.enabled;
            modalRoot.SetActive(false); resultPanel.SetActive(false);
        }
        private void OnEnable()
        {
            Active = this;
            navigator.ExitConfirmationRequested += ConfirmExit;

            foreach (var teleporter in teleporters) teleporter.locomotionEnded += TeleportEnded;
        }
        private void OnDisable()
        {
            if (teleporters != null) foreach (var teleporter in teleporters) if (teleporter != null) teleporter.locomotionEnded -= TeleportEnded;
            var controller = navigator != null ? navigator.ActiveAdapter : null;
            if (navigator != null) navigator.ExitConfirmationRequested -= ConfirmExit;
            if (IsOpen)
            {
                IsOpen = false;
                if (modalRoot != null) modalRoot.SetActive(false);
                if (controller != null && controller.isActiveAndEnabled) controller.SetUserPaused(false);
                if (navigator != null) BlockWorld(false);
            }
            if (Active == this) Active = null;
        }
        private void Start() { ConfigureFollowing(); ApplySettings(); Refresh(); }
        private void TeleportEnded(UnityEngine.XR.Interaction.Toolkit.Locomotion.LocomotionProvider provider) => Recenter();
        private void ConfigureFollowing()
        {
            if (follow == null) return;
            follow.head = head; follow.origin = navigator.origin.transform;
            follow.Recenter();
        }
        private void Update()
        {
            foreach (var pair in movementDefaults)
                if (pair.Key != null) pair.Key.enabled = pair.Value && !IsOpen && navigator.MovementAllowed;
            if(desktopRaycaster != null) desktopRaycaster.enabled = !UnityEngine.XR.XRSettings.isDeviceActive;
            Refresh();
            if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
            {
                if (IsOpen) Back(); else if (navigator.InContent) OpenPause();
            }
            if (inputText != null)
            {

                var fresh = openMenuPanel.GetComponentInChildren<FreshPressButton>(true);
                var action = fresh != null && fresh.inputs != null ? fresh.inputs.FindAction("Right/Press") : null;
                inputText.text = UnityEngine.XR.XRSettings.isDeviceActive && action != null
                    ? action.GetBindingDisplayString() + " 선택 · 화면 메뉴 버튼"
                    : "마우스 왼쪽 버튼 선택 · Esc 메뉴";
            }
        }
        public void OpenPause()
        {
            if (!navigator.InContent || navigator.Loading) return;
            Open(Page.Pause);
        }
        public void OpenSettings()
        {
            settingsFromMenu = !navigator.InContent;
            Open(Page.Settings);
        }
        public void ConfirmRestart() { Open(Page.Restart); }
        public void ConfirmExit() { Open(Page.Exit); }
        private void Open(Page next)
        {
            bool first = !IsOpen;
            IsOpen = true; page = next;
            if (first)
            {
                Controller?.SetUserPaused(true);
                BlockWorld(true);
                Recenter();
            }
            modalRoot.SetActive(true);
            pausePanel.SetActive(next == Page.Pause);
            settingsPanel.SetActive(next == Page.Settings);
            confirmPanel.SetActive(next == Page.Restart || next == Page.Exit);
            if(executeButton != null) executeButton.GetComponentInChildren<TMP_Text>().text = next == Page.Restart ? "처음부터 다시" : "선택 화면으로";
            confirmText.text = next == Page.Restart ? "현재 체험을 처음부터 다시 시작할까요?" : "현재 체험을 끝내고 시나리오 선택으로 돌아갈까요?";
            if (cancelButton != null && confirmPanel.activeSelf && EventSystem.current != null)
                EventSystem.current.SetSelectedGameObject(cancelButton.gameObject);
            ApplySettings(); Refresh();
        }
        public void Back()
        {
            if (page == Page.Restart || page == Page.Exit || (page == Page.Settings && !settingsFromMenu)) Open(Page.Pause);
            else Continue();
        }
        public void Continue()
        {
            IsOpen = false;
            modalRoot.SetActive(false);
            BlockWorld(false);
            Controller?.SetUserPaused(false);
            Refresh();
        }
        public void ExecuteConfirmation()
        {
            var requested = page;
            IsOpen = false; modalRoot.SetActive(false); BlockWorld(false);
            if (requested == Page.Restart) Controller?.Restart();
            else if (requested == Page.Exit) Controller?.ReturnToMenu();
            Refresh();
        }
        public void Recenter() { if(follow != null) follow.Recenter(); }
        public void VoiceDown() { VoiceVolume = Mathf.Max(0, VoiceVolume - .1f); ApplySettings(); }
        public void VoiceUp() { VoiceVolume = Mathf.Min(1, VoiceVolume + .1f); ApplySettings(); }
        public void EffectsDown() { EffectsVolume = Mathf.Max(0, EffectsVolume - .1f); ApplySettings(); }
        public void EffectsUp() { EffectsVolume = Mathf.Min(1, EffectsVolume + .1f); ApplySettings(); }
        public void ToggleSubtitles() { SubtitlesEnabled = !SubtitlesEnabled; ApplySettings(); }
        public void ToggleTextSize() { TextScale = TextScale > 1 ? 1 : 1.5f; ApplySettings(); }
        public void ApplySettings()
        {
            foreach (var text in FindObjectsByType<ThemedText>(FindObjectsInactive.Include, FindObjectsSortMode.None)) text.Apply(TextScale);
            contentUI?.ApplySettings(VoiceVolume, SubtitlesEnabled);
            foreach (var entry in navigator.contents) if (entry?.adapter != null) entry.adapter.ApplySettings(VoiceVolume, EffectsVolume, SubtitlesEnabled);
            if (settingsText != null) settingsText.text = $"음성 {VoiceVolume:P0}    효과음 {EffectsVolume:P0}\n자막 {(SubtitlesEnabled ? "켬" : "끔")}    글자 {TextScale:P0}";
        }
        private void BlockWorld(bool value)
        {
            if(contentGroup != null)
            { contentGroup.blocksRaycasts = !value; contentGroup.interactable = !value; contentGroup.alpha = value ? 0 : 1; }
            if(value) foreach(var teleporter in teleporters)
                if(teleporter is MenuTeleportationProvider guarded) guarded.CancelPending();
        }
        private void Refresh()
        {
            bool complete = navigator.InContent && Controller != null && Controller.ResultVisible && !Controller.Blocked &&
                (contentUI == null || contentUI.guidance == null || !contentUI.guidance.IsPresenting);
            if (resultPanel != null) resultPanel.SetActive(complete && !IsOpen && !Controller.Blocked);
            if (resultText != null) resultText.text = Controller?.ResultText ?? "";
            if (openMenuPanel != null) openMenuPanel.SetActive(navigator.InContent && !IsOpen && !complete);
            if(complete && resultPanel != null && resultText != null)
            {
                var panel=(RectTransform)resultPanel.transform;
                var text=resultText.rectTransform;
                float height=Mathf.Max(330,resultText.GetPreferredValues(resultText.text,936,Mathf.Infinity).y+16);
                panel.sizeDelta=new Vector2(1000,height+290);
                text.sizeDelta=new Vector2(936,height);
                text.anchoredPosition=new Vector2(0,90);
                foreach(var button in resultPanel.GetComponentsInChildren<FreshPressButton>(true))
                {
                    var rect=(RectTransform)button.transform;
                    rect.anchoredPosition=new Vector2(rect.anchoredPosition.x,-height*.5f-55);
                }
            }
            if(menuPanel != null) menuPanel.SetActive(!navigator.InContent && !navigator.Loading && !IsOpen);
            if(contentUI != null && contentUI.panel != null) contentUI.panel.SetActive(navigator.InContent && !complete && contentUI.Owner == Controller);
        }
        public void CompletedReturn() { Controller?.ReturnToMenu(); }
        public void CompletedRestart() { Controller?.Restart(); }
    }
}
