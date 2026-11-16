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
        public FloorButtonPresenter floorButtons;
        public Transform actionRoot => floorButtons != null ? floorButtons.ActionRoot : null;
        public ContentExperienceAdapter Owner { get; private set; }
        public int ActionCount => actions.Length;
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
                Owner = owner; status.text = "";
                Owner.StateChanged += RefreshAvailability;

            }
            title.text = heading;
            panel.SetActive(true);
        }
        public void SetStatus(ContentExperienceAdapter owner, string text)
        { if (Owner == owner && owner != null) status.text = text; }
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
            rect.sizeDelta = new Vector2(width, hasContent ? cursor + padding : 0);
            var background = panel.GetComponent<UnityEngine.UI.Image>();
            if (background != null) background.enabled = hasContent;
            layoutGeneration = generation; layoutStatus = status.text; layoutSubtitle = guidance.subtitle.text;
            layoutStatusSize = status.fontSize; layoutSubtitleSize = guidance.subtitle.fontSize;
            layoutSubtitles = guidance.subtitle.enabled;
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
            if (guidance.subtitle != null) guidance.subtitle.enabled = subtitles;
        }
    }
}
