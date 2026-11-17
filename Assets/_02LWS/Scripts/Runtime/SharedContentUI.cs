using System;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using LWS.FireNotification.Interaction;
using LWS.FireNotification.Presentation;

namespace LWS.FireNotification.Runtime
{
    public enum ContentStatusKind { Instruction, Warning }
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
        public RectTransform subtitleRoot;
        public Material subtitleMaterial;
        public FloorButtonPresenter floorButtons;
        public Transform actionRoot => floorButtons != null ? floorButtons.ActionRoot : null;
        public ContentExperienceAdapter Owner { get; private set; }
        public int ActionCount => actions.Length;
        public bool SubtitlesEnabled { get; private set; } = true;
        private ContentStatusKind statusKind;
        private ContentUIAction[] actions = Array.Empty<ContentUIAction>();
        private FloorButtonAnchor actionAnchor;
        private long generation, lastAccepted=-1;
        private long actionExecution=-1;
        private bool retryAfterExit;
        private bool Available => Owner != null && Owner.isActiveAndEnabled && !Owner.Blocked &&
            (SharedUIController.Active == null || (!SharedUIController.Active.IsOpen && !SharedUIController.Active.navigator.Loading && SharedUIController.Active.navigator.ActiveAdapter==Owner));
        public void Bind(ContentExperienceAdapter owner, string heading)
        {
            if (owner == null) throw new ArgumentNullException(nameof(owner));
            if (Owner != owner)
            {
                if (Owner != null) Owner.StateChanged -= RefreshAvailability;
                generation++; guidance.Cancel(); floorButtons.ResetDisplay(); actions = Array.Empty<ContentUIAction>(); actionAnchor=null;lastAccepted=-1;retryAfterExit=false;
                Owner = owner; status.text = ""; statusKind = ContentStatusKind.Instruction; layoutGeneration = -1;
                Owner.StateChanged += RefreshAvailability;

            }
            title.text = heading;
            panel.SetActive(true);
        }
        public void SetStatus(ContentExperienceAdapter owner, string text, ContentStatusKind kind = ContentStatusKind.Instruction)
        { if (Owner == owner && owner != null) { status.text = text; statusKind = kind; } }
        public void SetActions(ContentExperienceAdapter owner, params ContentUIAction[] next)
        {
            if(Owner!=owner||owner==null)return;
            if(next!=null&&next.Length>0)throw new ArgumentException("Nonempty floor actions require an anchor");
            SetActions(owner,null,next);
        }
        public void SetActions(ContentExperienceAdapter owner,FloorButtonAnchor anchor,params ContentUIAction[] next)
        {
            if(Owner!=owner||owner==null)return;
            next=next??Array.Empty<ContentUIAction>();
            if(next.Length>2)throw new ArgumentException("At most two shared floor actions");
            var ids=new HashSet<string>();
            foreach(var action in next)
                if(action==null||string.IsNullOrWhiteSpace(action.Id)||!ids.Add(action.Id)||string.IsNullOrWhiteSpace(action.Label)||action.Invoke==null)
                    throw new ArgumentException("Actions require unique IDs, labels and callbacks");
            if(next.Length>0&&anchor==null)throw new ArgumentException("Missing floor anchor");
            bool same=actionExecution==owner.ActionExecution&&actionAnchor==anchor&&actions.Length==next.Length;
            for(int i=0;same&&i<next.Length;i++)same=actions[i].Id==next[i].Id&&actions[i].Label==next[i].Label&&actions[i].Invoke==next[i].Invoke;
            if(same)return;
            generation++;actionExecution=owner.ActionExecution;actions=(ContentUIAction[])next.Clone();actionAnchor=next.Length>0?anchor:null;retryAfterExit=false;
            floorButtons.Present(owner,generation,anchor,actions,(ticket,id)=>Invoke(owner,ticket,id));
        }
        private void Invoke(ContentExperienceAdapter owner,long ticket,string id)
        {
            if(Owner!=owner||generation!=ticket||actionExecution!=owner.ActionExecution||!Available)return;
            foreach(var action in actions)if(action.Id==id)
            {
                lastAccepted=ticket;retryAfterExit=true;
                action.Invoke();return;
            }
        }
        public bool IsFloorExitComplete(ContentExperienceAdapter owner) => owner!=null&&Owner==owner&&lastAccepted>=0&&floorButtons.CompletedExitTicket==lastAccepted;
        public void ResetFloorDisplay()
        {
            generation++;lastAccepted=-1;actionExecution=-1;retryAfterExit=false;actions=Array.Empty<ContentUIAction>();actionAnchor=null;floorButtons.ResetDisplay();
        }
        public void Clear(ContentExperienceAdapter owner)
        {
            if (Owner != owner) return;
            if (Owner != null) Owner.StateChanged -= RefreshAvailability;
            ResetFloorDisplay(); Owner = null;
            guidance.Cancel(); title.text = status.text = ""; statusKind = ContentStatusKind.Instruction;
            status.enabled = false; guidance.subtitle.enabled = false; layoutGeneration = -1; panel.SetActive(false);
        }
        private string layoutStatus, layoutSubtitle;
        private float layoutStatusSize, layoutSubtitleSize;
        private bool layoutSubtitles, layoutStatusEnabled;
        private long layoutGeneration = -1;
        private void LateUpdate()
        {
            RefreshAvailability();
            bool warning = statusKind == ContentStatusKind.Warning && !string.IsNullOrWhiteSpace(status.text);
            bool narration = SubtitlesEnabled && guidance.IsPresenting;
            status.enabled = Owner != null && (warning || !narration);
            guidance.subtitle.enabled = Owner != null && !warning && narration;
            if (panel != null && panel.activeInHierarchy &&
                (layoutGeneration != generation || layoutStatus != status.text ||
                 layoutSubtitle != guidance.subtitle.text || layoutStatusSize != status.fontSize ||
                 layoutSubtitleSize != guidance.subtitle.fontSize || layoutSubtitles != guidance.subtitle.enabled || layoutStatusEnabled != status.enabled ||
                 status.color != Color.white || guidance.subtitle.color != Color.white))
                RefreshLayout();
        }

        // Narration, instructions and warnings share one unbacked, head-relative text area.
        public void RefreshLayout()
        {
            const float width = 1400, padding = 24;
            var rect = (RectTransform)panel.transform;
            rect.localScale = Vector3.one;
            rect.pivot = new Vector2(.5f, 1);
            rect.anchoredPosition = new Vector2(0, -180);
            rect.sizeDelta = new Vector2(width, 0);
            var background = panel.GetComponent<UnityEngine.UI.Image>();
            if (background != null) background.enabled = false;
            void TextRow(TMP_Text text, bool visible)
            {
                text.gameObject.SetActive(visible);
                if (!visible) return;
                float height = Mathf.Ceil(text.GetPreferredValues(text.text, width - 2 * padding, Mathf.Infinity).y);
                var row = text.rectTransform;
                row.anchorMin = row.anchorMax = new Vector2(.5f, 1);
                row.pivot = new Vector2(.5f, .5f);
                row.sizeDelta = new Vector2(width - 2 * padding, height);
                row.anchoredPosition = new Vector2(0, -padding - height / 2);
            }
            if (subtitleRoot != null)
            {
                const float ratio = 5f / 9f;
                var follow = GetComponentInParent<ComfortFollowUI>();
                subtitleRoot.anchorMin = subtitleRoot.anchorMax = new Vector2(.5f, 1);
                subtitleRoot.pivot = new Vector2(.5f, 1);
                subtitleRoot.sizeDelta = new Vector2(width, 0);
                subtitleRoot.localScale = Vector3.one * ratio;
                if (follow != null)
                {
                    var referenceHead = follow.transform.position - follow.transform.rotation * follow.offset;
                    subtitleRoot.localPosition = (1 - ratio) * rect.InverseTransformPoint(referenceHead);
                }
                TextRow(status, status.enabled && !string.IsNullOrWhiteSpace(status.text));
                TextRow(guidance.subtitle, guidance.subtitle.enabled && !string.IsNullOrWhiteSpace(guidance.subtitle.text));
                ApplySubtitlePreset();
            }
            layoutGeneration = generation; layoutStatus = status.text; layoutSubtitle = guidance.subtitle.text;
            layoutStatusSize = status.fontSize; layoutSubtitleSize = guidance.subtitle.fontSize;
            layoutSubtitles = guidance.subtitle.enabled;
            layoutStatusEnabled = status.enabled;
        }
        private void RefreshAvailability()
        {
            if(floorButtons==null)return;
            if(Owner!=null&&!Owner.isActiveAndEnabled){Clear(Owner);return;}
            floorButtons.SetBlocked(!Available);
            if(retryAfterExit&&floorButtons.CompletedExitTicket==lastAccepted&&actions.Length>0&&Available)
            {
                retryAfterExit=false;generation++;
                floorButtons.Present(Owner,generation,actionAnchor,actions,(ticket,id)=>Invoke(Owner,ticket,id));
            }
        }
        private void OnDisable(){if(Owner!=null)Clear(Owner);else floorButtons?.ResetDisplay();}
        public void ApplySettings(float voice, bool subtitles)
        {
            if (guidance.voice != null) guidance.voice.volume = voice;
            SubtitlesEnabled = subtitles;
            ApplySubtitlePreset();
        }
        private void ApplySubtitlePreset()
        {
            if (guidance == null || guidance.subtitle == null || subtitleMaterial == null) return;
            ApplyTextPreset(status);
            ApplyTextPreset(guidance.subtitle);
        }
        private void ApplyTextPreset(TMP_Text text)
        {
            if (text == null) return;
            if (text.fontSharedMaterial != subtitleMaterial)
            {
                text.fontSharedMaterial = subtitleMaterial;
                text.UpdateMeshPadding();
            }
            text.color = Color.white;
            text.raycastTarget = false;
        }
        private void OnEnable() { layoutGeneration = -1; ApplySubtitlePreset(); }
    }
}
