using System;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using LWS.FireNotification.Interaction;
using LWS.FireNotification.Presentation;

namespace LWS.FireNotification.Runtime
{
    public sealed class ContentUIAction
    {
        public readonly string Id, Label;
        public readonly Action Invoke;
        public ContentUIAction(string id, string label, Action invoke)
        { Id = id; Label = label; Invoke = invoke; }
    }

    // Owns presentation only. Content supplies text and actions, never its stage machine.
    [DefaultExecutionOrder(1000)]
    public sealed class SharedContentUI : MonoBehaviour
    {
        public GameObject panel;
        public TMP_Text title, status;
        public GuidancePresenter guidance;
        public Transform actionRoot;
        public FreshPressButton actionTemplate;
        public ContentExperienceAdapter Owner { get; private set; }
        public int ActionCount => actions.Length;
        private readonly List<FreshPressButton> buttons = new List<FreshPressButton>();
        private ContentUIAction[] actions = Array.Empty<ContentUIAction>();
        private long generation;
        private bool wasAvailable;
        private bool Available => Owner != null && Owner.isActiveAndEnabled && !Owner.Blocked &&
            (SharedUIController.Active == null || !SharedUIController.Active.IsOpen);

        public void Bind(ContentExperienceAdapter owner, string heading)
        {
            if (owner == null) throw new ArgumentNullException(nameof(owner));
            if (Owner != owner)
            {
                if (Owner != null) Owner.StateChanged -= RefreshAvailability;
                generation++; guidance.Cancel(); HideActions(); actions = Array.Empty<ContentUIAction>();
                Owner = owner; status.text = "";
                Owner.StateChanged += RefreshAvailability;
                wasAvailable = Available;
            }
            title.text = heading;
            panel.SetActive(true);
        }
        public void SetStatus(ContentExperienceAdapter owner, string text)
        { if (Owner == owner && owner != null) status.text = text; }
        public void SetActions(ContentExperienceAdapter owner, params ContentUIAction[] next)
        {
            if (Owner != owner || owner == null) return;
            next = next ?? Array.Empty<ContentUIAction>();
            if (next.Length > 2) throw new ArgumentException("The shared action row supports at most two choices; extend the shared layout before adding more");
            bool same = actions.Length == next.Length;
            for (int i = 0; same && i < next.Length; i++)
                same = actions[i].Id == next[i].Id && actions[i].Label == next[i].Label && actions[i].Invoke == next[i].Invoke;
            if (same) return;
            var ids = new HashSet<string>();
            foreach (var action in next)
                if (action == null || string.IsNullOrWhiteSpace(action.Id) || !ids.Add(action.Id) ||
                    string.IsNullOrWhiteSpace(action.Label) || action.Invoke == null)
                    throw new ArgumentException("Shared UI actions need unique IDs, labels and callbacks");
            generation++; HideActions(); actions = (ContentUIAction[])next.Clone();
            for (int i = 0; i < next.Length; i++)
            {
                if (i == buttons.Count)
                {
                    var button = Instantiate(actionTemplate, actionRoot);
                    button.name = "ActionButton" + (i + 1);
                    button.selected = new UnityEngine.Events.UnityEvent();
                    button.CanSelect = () => Available;
                    buttons.Add(button);
                }
                var current = buttons[i]; var action = next[i]; long ticket = generation;
                current.selected = new UnityEngine.Events.UnityEvent();
                current.selected.AddListener(() => Invoke(owner, ticket, action.Id));
                current.GetComponentInChildren<TMP_Text>(true).text = action.Label;
                var rect = (RectTransform)current.transform;
                float width = next.Length == 1 ? 660 : 666;
                rect.sizeDelta = new Vector2(width, next.Length == 1 ? 112 : 180);
                rect.anchoredPosition = new Vector2((i - (next.Length - 1) * .5f) * (width + 20), 0);
                current.gameObject.SetActive(true);
                var themed = current.GetComponent<ThemedButton>();
                if (themed != null) themed.SetAvailable(Available);
                var ui = SharedUIController.Active;
                foreach (var text in current.GetComponentsInChildren<ThemedText>(true)) text.Apply(ui != null ? ui.TextScale : 1);
            }
        }
        private void Invoke(ContentExperienceAdapter owner, long ticket, string id)
        {
            if (Owner != owner || generation != ticket || !Available) return;
            foreach (var action in actions) if (action.Id == id) { action.Invoke(); return; }
        }
        private void HideActions()
        { foreach (var button in buttons) if (button != null) button.gameObject.SetActive(false); }
        public void Clear(ContentExperienceAdapter owner)
        {
            if (Owner != owner) return;
            if (Owner != null) Owner.StateChanged -= RefreshAvailability;
            generation++; HideActions(); actions = Array.Empty<ContentUIAction>(); Owner = null;
            guidance.Cancel(); title.text = status.text = ""; panel.SetActive(false);
        }
        private string layoutStatus, layoutSubtitle;
        private float layoutStatusSize, layoutSubtitleSize;
        private bool layoutSubtitles;
        private long layoutGeneration = -1;
        private void LateUpdate()
        {
            RefreshAvailability();
            if (panel != null && panel.activeInHierarchy &&
                (layoutGeneration != generation || layoutStatus != status.text ||
                 layoutSubtitle != guidance.subtitle.text || layoutStatusSize != status.fontSize ||
                 layoutSubtitleSize != guidance.subtitle.fontSize || layoutSubtitles != guidance.subtitle.enabled))
                RefreshLayout();
        }

        // Top stays below the observation area; empty text never reserves an opaque rectangle.
        // All content types share this layout, including enlarged text and tracking warnings.
        public void RefreshLayout()
        {
            const float width = 1400, padding = 24, gap = 16;
            var rect = (RectTransform)panel.transform;
            rect.localScale = Vector3.one;
            rect.pivot = new Vector2(.5f, 1);
            rect.anchoredPosition = new Vector2(0, -180);
            float cursor = padding;
            bool hasContent = false;
            void TextRow(TMP_Text text, bool visible)
            {
                text.gameObject.SetActive(visible);
                if (!visible) return;
                if (hasContent) cursor += gap;
                float height = Mathf.Ceil(text.GetPreferredValues(text.text, width - 2 * padding, Mathf.Infinity).y);
                var row = text.rectTransform;
                row.anchorMin = row.anchorMax = new Vector2(.5f, 1);
                row.pivot = new Vector2(.5f, .5f);
                row.sizeDelta = new Vector2(width - 2 * padding, height);
                row.anchoredPosition = new Vector2(0, -cursor - height / 2);
                cursor += height; hasContent = true;
            }
            TextRow(status, !string.IsNullOrWhiteSpace(status.text));
            TextRow(guidance.subtitle, guidance.subtitle.enabled && !string.IsNullOrWhiteSpace(guidance.subtitle.text));
            if (actions.Length > 0)
            {
                if (hasContent) cursor += gap;
                float height = actions.Length == 1 ? 112 : 180;
                var row = (RectTransform)actionRoot;
                row.anchorMin = row.anchorMax = new Vector2(.5f, 1);
                row.pivot = new Vector2(.5f, .5f);
                row.sizeDelta = new Vector2(width - 2 * padding, height);
                row.anchoredPosition = new Vector2(0, -cursor - height / 2);
                cursor += height; hasContent = true;
            }
            rect.sizeDelta = new Vector2(width, hasContent ? cursor + padding : 0);
            var background = panel.GetComponent<UnityEngine.UI.Image>();
            if (background != null) background.enabled = hasContent;
            layoutGeneration = generation; layoutStatus = status.text; layoutSubtitle = guidance.subtitle.text;
            layoutStatusSize = status.fontSize; layoutSubtitleSize = guidance.subtitle.fontSize;
            layoutSubtitles = guidance.subtitle.enabled;
        }
        private void RefreshAvailability()
        {
            bool available = Available;
            if (available != wasAvailable)
            {
                foreach (var button in buttons) if (button != null) button.InvalidatePress();
                wasAvailable = available;
            }
            foreach (var button in buttons)
                if (button != null && button.gameObject.activeSelf)
                {
                    var style = button.GetComponent<ThemedButton>();
                    if (style != null) style.SetAvailable(available, button.Processing);
                }
        }
        public void ApplySettings(float voice, bool subtitles)
        {
            if (guidance.voice != null) guidance.voice.volume = voice;
            if (guidance.subtitle != null) guidance.subtitle.enabled = subtitles;
        }
    }
}
