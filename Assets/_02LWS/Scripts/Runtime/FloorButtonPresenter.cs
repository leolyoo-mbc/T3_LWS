using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using LWS.FireNotification.Interaction;
using LWS.FireNotification.Presentation;

namespace LWS.FireNotification.Runtime
{
    public enum FloorButtonPhase { Hidden, Rising, Ready, Lowering }

    // One immutable outgoing display and at most one replacement request.
    public sealed class FloorButtonPresenter : MonoBehaviour
    {
        public Canvas floorCanvas;
        public RectTransform viewport, movingPanel;
        public CanvasGroup group;
        public FreshPressButton[] buttons;
        public GraphicRaycaster desktopRaycaster;
        public FloorButtonPhase Phase { get; private set; }
        public float Progress { get; private set; }
        public bool Blocked { get; private set; }
        public long CompletedExitTicket { get; private set; }=-1;
        public long AcceptedTicket { get; private set; }=-1;
        public long DisplayTicket => display?.ticket??-1;
        public FloorButtonAnchor DisplayAnchor => display?.anchor;
        public Transform ActionRoot => movingPanel;
        private sealed class Request
        {
            public ContentExperienceAdapter owner;
            public long ticket;
            public FloorButtonAnchor anchor;
            public ContentUIAction[] actions;
            public Action<long,string> select;
        }
        private Request display, pending;
        private bool consumed;
        private float height, raised, hidden, lowerStart;
        private Vector3 floorPosition;
        private Quaternion panelRotation;
        private const float UnitsPerMetre=1000;
        private float textScale=1;
        public void Present(ContentExperienceAdapter owner,long ticket,FloorButtonAnchor anchor,ContentUIAction[] actions,Action<long,string> select)
        {
            if(display!=null&&display.owner!=owner)ResetDisplay();
            Request next=null;
            if(actions!=null&&actions.Length>0)
            {
                if(owner==null||anchor==null||select==null||!anchor.gameObject.activeInHierarchy)throw new ArgumentException("Floor request needs an active owner, anchor and callback");
                var errors=anchor.ConfigurationIssues();if(errors.Length>0)throw new ArgumentException(string.Join("; ",errors));
                if(actions.Length>2)throw new ArgumentException("At most two floor actions");
                next=new Request{owner=owner,ticket=ticket,anchor=anchor,actions=(ContentUIAction[])actions.Clone(),select=select};
            }
            if(display!=null&&next!=null&&display.ticket==ticket&&display.owner==owner)return;
            pending=next;
            if(Phase==FloorButtonPhase.Hidden){if(pending!=null)ShowPending();return;}
            if(Phase!=FloorButtonPhase.Lowering)StartLowering(false);
        }
        public void SetBlocked(bool value)
        {
            if(Blocked!=value){Blocked=value;InvalidatePress();}
            RefreshAvailability();
        }
        public void ResetDisplay()
        {
            display=pending=null;consumed=false;AcceptedTicket=CompletedExitTicket=-1;
            Phase=FloorButtonPhase.Hidden;Progress=0;
            if(buttons!=null)foreach(var button in buttons)if(button!=null){button.InvalidatePress();button.selected=new UnityEngine.Events.UnityEvent();button.gameObject.SetActive(false);}
            RefreshAvailability();
        }
        private void OnDisable()=>ResetDisplay();
        private void Awake()=>ResetDisplay();
        private void Update()
        {
            SetBlocked(RequestBlocked());
            if(display!=null&&display.anchor!=null&&!display.anchor.gameObject.activeInHierarchy){ResetDisplay();return;}
            if(!Blocked&&display!=null)
            {
                if(Phase==FloorButtonPhase.Rising)
                {
                    Progress=Mathf.Min(1,Progress+Time.unscaledDeltaTime/display.anchor.riseSeconds);
                    if(Progress>=1){Phase=FloorButtonPhase.Ready;InvalidatePress();}
                }
                else if(Phase==FloorButtonPhase.Lowering)
                {
                    Progress=Mathf.Min(1,Progress+Time.unscaledDeltaTime/display.anchor.lowerSeconds);
                    if(Progress>=1)
                    {
                        if(consumed)CompletedExitTicket=display.ticket;
                        display=null;Phase=FloorButtonPhase.Hidden;Progress=0;
                        foreach(var b in buttons)b.gameObject.SetActive(false);
                        if(pending!=null)ShowPending();
                    }
                }
                ApplyPosition();
            }
            RefreshAvailability();
        }
        private void ShowPending()
        {
            display=pending;pending=null;consumed=false;
            AcceptedTicket=CompletedExitTicket=-1;
            Phase=FloorButtonPhase.Rising;Progress=0;
            floorPosition=display.anchor.transform.position;panelRotation=display.anchor.transform.rotation;
            ConfigureLayout();
            for(int i=0;i<buttons.Length;i++)
            {
                var b=buttons[i];b.selected=new UnityEngine.Events.UnityEvent();
                b.gameObject.SetActive(i<display.actions.Length);
                b.CanSelect=()=>Phase==FloorButtonPhase.Ready&&!consumed&&!Blocked&&!RequestBlocked();
                if(i<display.actions.Length)
                {
                    long ticket=display.ticket;string id=display.actions[i].Id;
                    b.selected.AddListener(()=>Select(ticket,id));
                }
                b.InvalidatePress();
            }
            ApplyPosition();RefreshAvailability();
        }
        private void ConfigureLayout()
        {
            if(display==null)return;
            var a=display.anchor;int count=display.actions.Length;
            float width=(count==1?a.singleWidth:(a.twoTotalWidth-a.gap)*.5f)*UnitsPerMetre;
            height=a.minimumHeight*UnitsPerMetre;
            for(int i=0;i<count;i++)
            {
                var label=buttons[i].GetComponentInChildren<TMP_Text>(true);label.text=display.actions[i].Label;
                foreach(var t in buttons[i].GetComponentsInChildren<ThemedText>(true))t.Apply(textScale);
                height=Mathf.Max(height,label.GetPreferredValues(label.text,width-48,Mathf.Infinity).y+48);
            }
            float fullWidth=(count==1?a.singleWidth:a.twoTotalWidth)*UnitsPerMetre;
            // The mask stays vertical in the world; only the panel receives the authored tilt.
            var yaw=Quaternion.Euler(0,panelRotation.eulerAngles.y,0);
            var tilt=Quaternion.Inverse(yaw)*panelRotation;
            float minY=float.PositiveInfinity,maxY=float.NegativeInfinity,maxX=0;
            foreach(float x in new[]{-fullWidth*.5f,fullWidth*.5f})foreach(float y in new[]{0f,height})
            {
                var corner=tilt*new Vector3(x,y,0);
                minY=Mathf.Min(minY,corner.y);maxY=Mathf.Max(maxY,corner.y);maxX=Mathf.Max(maxX,Mathf.Abs(corner.x));
            }
            hidden=-maxY;
            raised=Mathf.Max(0,a.raisedBottomHeight-a.floorClearance)*UnitsPerMetre-minY;
            floorCanvas.transform.SetPositionAndRotation(floorPosition+Vector3.up*a.floorClearance,yaw);
            floorCanvas.transform.localScale=Vector3.one/UnitsPerMetre;
            var canvasRect=(RectTransform)floorCanvas.transform;
            canvasRect.pivot=new Vector2(.5f,0);
            canvasRect.sizeDelta=new Vector2(maxX*2+40,raised+maxY+80);
            viewport.anchorMin=viewport.anchorMax=viewport.pivot=new Vector2(.5f,0);
            viewport.localRotation=Quaternion.identity;
            viewport.anchoredPosition=Vector2.zero;viewport.sizeDelta=canvasRect.sizeDelta;
            movingPanel.anchorMin=movingPanel.anchorMax=movingPanel.pivot=new Vector2(.5f,0);
            movingPanel.sizeDelta=new Vector2(fullWidth,height);
            movingPanel.localRotation=tilt;
            for(int i=0;i<count;i++)
            {
                var r=(RectTransform)buttons[i].transform;r.anchorMin=r.anchorMax=new Vector2(.5f,0);r.pivot=new Vector2(.5f,.5f);
                r.sizeDelta=new Vector2(width,height);r.anchoredPosition=new Vector2((i-(count-1)*.5f)*(width+a.gap*UnitsPerMetre),height*.5f);
                var label=buttons[i].GetComponentInChildren<TMP_Text>(true).rectTransform;
                label.anchorMin=Vector2.zero;label.anchorMax=Vector2.one;label.offsetMin=new Vector2(24,24);label.offsetMax=new Vector2(-24,-24);
            }
        }
        private void ApplyPosition()
        {
            if(display==null||movingPanel==null)return;
            float y=Phase==FloorButtonPhase.Lowering?Mathf.Lerp(lowerStart,hidden,Mathf.SmoothStep(0,1,Progress)):
                Mathf.Lerp(hidden,raised,Mathf.SmoothStep(0,1,Progress));
            movingPanel.anchoredPosition=new Vector2(0,y);
        }
        private void StartLowering(bool accepted)
        {
            lowerStart=movingPanel.anchoredPosition.y;Phase=FloorButtonPhase.Lowering;Progress=0;consumed=accepted;
            InvalidatePress();RefreshAvailability();
        }
        private void Select(long ticket,string id)
        {
            if(display==null||display.ticket!=ticket||Phase!=FloorButtonPhase.Ready||consumed||Blocked||RequestBlocked())return;
            var request=display;AcceptedTicket=ticket;StartLowering(true);
            request.select(ticket,id);
        }
        private bool RequestBlocked()
        {
            if(display==null)return false;
            var ui=SharedUIController.Active;
            return display.owner==null||!display.owner.isActiveAndEnabled||display.owner.Blocked||
                (ui!=null&&(ui.IsOpen||ui.navigator.Loading||ui.navigator.ActiveAdapter!=display.owner));
        }
        public void ApplySettings(float scale)
        {
            textScale=scale;ConfigureLayout();ApplyPosition();
            if(desktopRaycaster!=null)desktopRaycaster.enabled=!UnityEngine.XR.XRSettings.isDeviceActive;
        }
        private void InvalidatePress(){if(buttons!=null)foreach(var b in buttons)if(b!=null)b.InvalidatePress();}
        private void RefreshAvailability()
        {
            bool visible=display!=null&&Phase!=FloorButtonPhase.Hidden&&!Blocked;
            if(group!=null){group.alpha=visible?1:0;group.blocksRaycasts=visible&&Phase==FloorButtonPhase.Ready&&!consumed;group.interactable=group.blocksRaycasts;}
            if(buttons!=null)foreach(var b in buttons)if(b!=null)b.GetComponent<ThemedButton>()?.SetAvailable(visible&&Phase==FloorButtonPhase.Ready&&!consumed);
            if(desktopRaycaster!=null)desktopRaycaster.enabled=!UnityEngine.XR.XRSettings.isDeviceActive;
        }
#if UNITY_EDITOR
        public void Preview(FloorButtonAnchor anchor,bool raisedState,bool two,float scale)
        {
            if(Application.isPlaying)throw new InvalidOperationException("Preview is Edit Mode only");
            textScale=scale;
            display=new Request{anchor=anchor,actions=two?new[]{new ContentUIAction("left","이 계단으로 대피한다",()=>{}),new ContentUIAction("right","다른 경로를 찾는다",()=>{})}:new[]{new ContentUIAction("ready","준비 완료",()=>{})}};
            floorPosition=anchor.transform.position;panelRotation=anchor.transform.rotation;
            ConfigureLayout();Phase=raisedState?FloorButtonPhase.Ready:FloorButtonPhase.Rising;Progress=raisedState?1:0;
            for(int i=0;i<buttons.Length;i++){buttons[i].gameObject.SetActive(i<display.actions.Length);buttons[i].enabled=false;}
            ApplyPosition();group.alpha=1;group.blocksRaycasts=false;group.interactable=false;
            foreach(var r in GetComponentsInChildren<UnityEngine.EventSystems.BaseRaycaster>(true))r.enabled=false;
        }
#endif
    }
}
