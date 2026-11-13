using System;
using System.Linq;
using UnityEngine;
using LWS.FireNotification.Domain;
using LWS.FireNotification.Domain.EvacuationRoute;
using LWS.FireNotification.Interaction;
using LWS.FireNotification.Presentation;
using Waypoint = LWS.FireNotification.Interaction.RouteWaypoint;

namespace LWS.FireNotification.Runtime
{
    [DefaultExecutionOrder(-300)]
    public sealed class EvacuationRouteController : ContentExperienceAdapter
    {
        public RouteScenarioAsset[] scenarios;
        public GameObject[] spaces;
        public Waypoint[] waypoints;
        public RouteGuidanceCatalog catalog;
        public ContentSceneNavigator navigator;
        public HeadPoseSampleProvider poses;
        public ExperienceLifecycle lifecycle;
        public MenuTeleportationProvider teleport;
        public RouteSmokePresenter smoke;
        public float arrivalRadius=.65f;
        public RouteSession Session { get; private set; }
        private static long nextRun;
        private bool userPaused, configurationError, recovery, wasBlocked, hadSystemBlock;
        private long shownExecution=-1;
        private string occupiedTarget;
        private PresentationRun playback;
        private Action pending;
        private GuidancePresenter Guidance => view!=null&&view.Owner==this?view.guidance:null;
        public override bool Blocked => Session==null||Session.Stage==RouteStage.Closed||Session.Blocked;
        public override bool LocomotionAllowed => !Blocked&&(Session.Stage==RouteStage.Explore||Session.Stage==RouteStage.Decision);
        public override bool ResultVisible => Session!=null&&Session.Stage==RouteStage.Result;
        public override string ResultText
        {
            get
            {
                if(Session==null)return "";
                return string.Join("\n",Session.Results.Select((r,i)=>
                    "상황 "+(i+1)+": "+(r.Completed?"대피 경로 선택 완료":"미완료")+
                    (r.DetourPerformed?"\n우회 수행":"")+"\n"+
                    "첫 선택: "+(r.Decisions.Count==0?"미수행":TargetName(r.Decisions[0].TargetId)+" / "+ChoiceName(r.Decisions[0].Choice))+
                    (r.Decisions.Any(d=>d.FeedbackRequired)?"\n피드백 후 완료":"")))+"\n실제 대피 완료는 아닙니다.";
            }
        }
        private string TargetName(string id)
        {
            var t=scenarios.SelectMany(s=>s.targets).FirstOrDefault(v=>v.Id==id);
            return t==null?id:t.Kind==RouteTargetKind.Elevator?"엘리베이터":id.EndsWith("left",StringComparison.Ordinal)?"계단 1":"계단 2";
        }
        private static string ChoiceName(RouteChoice c)=>c==RouteChoice.Use?"이용 선택":"다른 경로 탐색";
#if UNITY_EDITOR
        private bool? debugTracking;
        public void DebugTracking(bool value) { debugTracking=value; }
#endif
        private bool TrackingValid
        {
            get
            {
#if UNITY_EDITOR
                if(debugTracking.HasValue)return debugTracking.Value&&poses!=null&&poses.ConfigurationValid;
#endif
                return poses!=null&&poses.Read(lifecycle!=null&&!lifecycle.Blocked,0).positionTracked;
            }
        }
        private void OnEnable()
        {
            if(lifecycle!=null)lifecycle.Changed+=LifecycleChanged;
            if(teleport!=null)teleport.locomotionEnded+=TeleportEnded;
        }
        private void OnDisable()
        {
            if(lifecycle!=null)lifecycle.Changed-=LifecycleChanged;
            if(teleport!=null)teleport.locomotionEnded-=TeleportEnded;
            Stop();
        }
        private void TeleportEnded(UnityEngine.XR.Interaction.Toolkit.Locomotion.LocomotionProvider _) { CheckPosition(); }
        private void LifecycleChanged(bool blocked)
        {
            if(blocked) { recovery=true;CancelPresentation();teleport?.CancelPending(); }
            UpdateBlockers();
        }
        public string[] ConfigurationIssues()
        {
            var errors=new System.Collections.Generic.List<string>();
            try { RouteSession.Validate(scenarios.Select(s=>s.ToScenario()).ToArray());catalog.Validate(); }
            catch(Exception e) {errors.Add(e.Message);}
            if(view==null||navigator==null||navigator.origin==null||poses==null||!poses.ConfigurationValid||lifecycle==null||teleport==null||spaces==null||spaces.Length!=2||spaces.Any(s=>s==null)||smoke==null||!smoke.ConfigurationValid)errors.Add("Missing route references");
            if(waypoints==null||waypoints.Length==0)errors.Add("Missing route anchors");
            else
            {
                if(waypoints.Any(w=>w==null||w.anchor==null||w.anchor.teleportAnchorTransform==null||w.marker==null))errors.Add("Missing fixed anchor");
                foreach(var w in waypoints.Where(w=>w!=null&&w.anchor!=null&&w.anchor.teleportAnchorTransform!=null))
                    if(Vector3.Distance(w.transform.position,w.anchor.teleportAnchorTransform.position)>.01f)errors.Add("Destination differs from checkpoint: "+w.id);
                if(waypoints.Where(w=>w!=null).Select(w=>w.id).Distinct(StringComparer.Ordinal).Count()!=waypoints.Length)errors.Add("Duplicate anchor id");
                if(scenarios!=null)foreach(var asset in scenarios.Where(a=>a!=null))
                {
                    var local=waypoints.Where(w=>w!=null&&w.scenarioId==asset.id).ToArray();
                    foreach(var w in asset.waypoints)
                    {
                        var match=local.SingleOrDefault(p=>p.id==w.Id);
                        if(match==null||match.targetId!=w.TargetId||!match.adjacentIds.OrderBy(x=>x).SequenceEqual(w.AdjacentIds.OrderBy(x=>x)))errors.Add("Anchor/data mismatch: "+w.Id);
                    }
                    for(int i=0;i<local.Length;i++)for(int j=i+1;j<local.Length;j++)
                        if(Vector3.Distance(local[i].transform.position,local[j].transform.position)<2)errors.Add("Overlapping route anchors");
                }
            }
            if(arrivalRadius<=0||arrivalRadius>=1)errors.Add("Invalid arrival radius");
            return errors.ToArray();
        }
        public override void Begin()
        {
            if(Session!=null&&Session.Stage!=RouteStage.Closed)return;
            Session=new RouteSession();configurationError=false;userPaused=false;wasBlocked=false;hadSystemBlock=false;recovery=false;shownExecution=-1;occupiedTarget=null;
            view?.Bind(this,"안전한 대피 경로 판단");
            try
            {
                var issues=ConfigurationIssues();if(issues.Length>0)throw new InvalidOperationException(string.Join("; ",issues));
                Session.Begin(++nextRun,scenarios.Select(s=>s.ToScenario()).ToArray());
                spaces[0].SetActive(true);spaces[1].SetActive(false);smoke.Stop();
            }
            catch(Exception e) {configurationError=true;Debug.LogWarning("Route configuration: "+e.Message);}
            UpdateBlockers();Present();Publish();
        }
        private void UpdateBlockers()
        {
            if(Session==null)return;
            var b=RouteBlocker.None;
            if(userPaused)b|=RouteBlocker.UserPause;
            if(lifecycle==null||lifecycle.Blocked)b|=RouteBlocker.Lifecycle;
            if(!TrackingValid)b|=RouteBlocker.Tracking;
            if(configurationError)b|=RouteBlocker.Configuration;
            bool system=(b&(RouteBlocker.Lifecycle|RouteBlocker.Tracking))!=0;
            if(system&&!hadSystemBlock) {recovery=true;CancelPresentation();teleport?.CancelPending();}
            hadSystemBlock=system;
            Session.SetBlockers(b);
            if(wasBlocked!=Session.Blocked)
            {
                teleport?.CancelPending();Guidance?.SetPaused(Session.Blocked);smoke?.SetPaused(Session.Blocked);
                wasBlocked=Session.Blocked;Publish();
            }
            if(Session.Blocked)DisableAnchors();
        }
        // XRI's provider (-210) and body transformer (-205) consume requests in Update.
        private void Update() { if(Session!=null&&Session.Stage!=RouteStage.Closed)UpdateBlockers(); }
        private void LateUpdate()
        {
            if(Session==null)return;
            UpdateBlockers();
            if(configurationError) {view?.SetStatus(this,"체험 구성을 확인할 수 없습니다. 메뉴로 돌아가 주세요.");return;}
            if(Session.Stage==RouteStage.Closed)return;
            if(!Session.Blocked)
            {
                if(recovery) {recovery=false;shownExecution=-1;}
                var done=pending;pending=null;done?.Invoke();
                if(Session.Stage==RouteStage.Transition)Transition();
                if(LocomotionAllowed)CheckPosition();
            }
            if(shownExecution!=Session.Execution)Present();
            RefreshStatus();RefreshAnchors();
        }
        private void CheckPosition()
        {
            if(!LocomotionAllowed||!TrackingValid||navigator.origin==null)return;
            var p=navigator.origin.Camera.transform.position;p.y=0;
            var local=waypoints.Where(w=>w.scenarioId==Session.Scenario.Id).ToArray();
            if(occupiedTarget!=null)
            {
                var old=local.First(w=>w.targetId==occupiedTarget);var at=old.transform.position;at.y=0;
                if(Vector3.Distance(p,at)>arrivalRadius)
                {Session.LeaveCheckpoint(Session.RunId,Session.Execution,occupiedTarget);occupiedTarget=null;}
            }
            foreach(var w in local)
            {
                var at=w.transform.position;at.y=0;
                if(Vector3.Distance(p,at)>arrivalRadius)continue;
                if(Session.Arrive(Session.RunId,Session.Execution,w.id)&&!string.IsNullOrEmpty(w.targetId))occupiedTarget=w.targetId;
                break;
            }
        }
        private void RefreshAnchors()
        {
            if(!LocomotionAllowed) {DisableAnchors();return;}
            var current=waypoints.First(w=>w.id==Session.CurrentWaypointId);
            foreach(var w in waypoints)w.SetReachable(w.scenarioId==Session.Scenario.Id&&current.adjacentIds.Contains(w.id));
        }
        private void DisableAnchors() {if(waypoints!=null)foreach(var w in waypoints)if(w!=null)w.SetReachable(false);}
        private void CancelPresentation() {playback?.Dispose();playback=null;pending=null;Guidance?.Cancel();}
        private void Play(string key,Action done)
        {
            var session=Session;long run=session.RunId,execution=session.Execution;
            playback=new PresentationRun(execution,ExperienceStage.SituationGuidance,null);var ticket=playback;
            Guidance.Play(catalog.Get(key),ticket,()=> {if(playback==ticket)pending=()=> {if(Session==session&&!Blocked&&Session.RunId==run&&Session.Execution==execution)done?.Invoke();};});
            Guidance.SetPaused(Blocked);
        }
        private void Present()
        {
            CancelPresentation();shownExecution=Session.Execution;view?.SetActions(this);teleport?.CancelPending();
            if(configurationError||Session.Stage==RouteStage.Closed)return;
            long r=Session.RunId,e=Session.Execution;string target=Session.CurrentTargetId;
            switch(Session.Stage)
            {
                case RouteStage.AwaitingReady:
                    Play("ready",null);view.SetActions(this,new ContentUIAction("ready","준비 완료",()=> {UpdateBlockers();if(Session.Ready(r,e))Present();}));break;
                case RouteStage.Intro:Play(Session.ScenarioIndex==0?"intro-a":"intro-b",()=>Session.GuidanceDone(r,e));break;
                case RouteStage.Decision:
                    var t=Session.Scenario.Targets.First(v=>v.Id==target);
                    view.SetActions(this,new ContentUIAction("use",t.Kind==RouteTargetKind.Stair?"이 계단으로 대피한다":"이용한다",()=>Choose(r,e,target,RouteChoice.Use)),new ContentUIAction("another","다른 경로를 찾는다",()=>Choose(r,e,target,RouteChoice.FindAnotherRoute)));break;
                case RouteStage.Feedback:
                    Play(Session.Scenario.Targets.First(tg=>tg.Id==target).Kind==RouteTargetKind.Elevator?"elevator":"smoke",()=>Session.GuidanceDone(r,e));break;
                case RouteStage.ScenarioCompletion:
                    if(Session.CompletionGuidanceDone)view.SetActions(this,new ContentUIAction("next","다음 연습",()=> {UpdateBlockers();if(Session.NextScenario(r,e))Present();}));
                    else Play(Session.ScenarioIndex==0?"complete-a":"completion",()=>Session.GuidanceDone(r,e));break;
            }
            RefreshStatus();RefreshAnchors();Publish();
        }
        private void Choose(long r,long e,string target,RouteChoice choice)
        { UpdateBlockers();CheckPosition();if(Session.Choose(r,e,target,choice))Present(); }
        private void Transition()
        {
            try
            {
                teleport.CancelPending();spaces[0].SetActive(false);spaces[1].SetActive(true);
                var start=waypoints.Single(w=>w.id==scenarios[1].startWaypointId).transform;
                var origin=navigator.origin;origin.MatchOriginUpCameraForward(Vector3.up,start.forward);
                if(!origin.MoveCameraToWorldLocation(start.position+Vector3.up*origin.CameraInOriginSpaceHeight))throw new InvalidOperationException("B position failed");
                var follower=view.GetComponentInParent<ComfortFollowUI>();if(follower==null)follower=UnityEngine.Object.FindFirstObjectByType<ComfortFollowUI>();
                if(follower==null)throw new InvalidOperationException("Missing UI follower");follower.Recenter();
                var delta=origin.Camera.transform.position-start.position;delta.y=0;
                if(delta.magnitude>arrivalRadius||!spaces[1].activeInHierarchy)throw new InvalidOperationException("B transition not confirmed");
                if(Session.TransitionDone(Session.RunId,Session.Execution)) {occupiedTarget=null;smoke.Play();Present();}
            }
            catch(Exception error) {configurationError=true;UpdateBlockers();view.SetActions(this);view.SetStatus(this,"구역을 전환할 수 없습니다. 메뉴로 돌아가 주세요.");Debug.LogWarning("Route transition: "+error.Message);}
        }
        private void RefreshStatus()
        {
            if(view==null)return;
            string text="";
            if(configurationError)text="체험 구성을 확인할 수 없습니다. 메뉴로 돌아가 주세요.";
            else if((Session.Blockers&RouteBlocker.Tracking)!=0)text="머리 위치 추적이 복구될 때까지 잠시 기다리세요. 메뉴로 나갈 수 있습니다.";
            else if(Session.Stage==RouteStage.AwaitingReady)text="주변을 살피며 대피 경로를 선택하는 연습입니다.\n준비 완료를 선택하세요.";
            else if(Session.Stage==RouteStage.Explore)text="이동 지점을 가리켜 순간이동하세요. 입구 앞에서 주변 상태를 살펴보세요.";
            else if(Session.Stage==RouteStage.Decision||Session.Stage==RouteStage.Feedback)text=TargetName(Session.CurrentTargetId)+" 앞 확인 지점\n주변 상태를 살펴보고 행동을 선택하세요.";
            else if(Session.Stage==RouteStage.ScenarioCompletion)text="대피 경로 선택 연습 완료";
            view.SetStatus(this,text);
        }
        public override void Stop()
        {CancelPresentation();teleport?.CancelPending();DisableAnchors();smoke?.Stop();Session?.Close();occupiedTarget=null;view?.Clear(this);Publish();}
        public override void Restart()
        {
            Stop();Session=null;
            if(waypoints!=null&&scenarios!=null&&scenarios.Length>0)
            {
                var start=waypoints.Single(w=>w.id==scenarios[0].startWaypointId).transform;
                navigator.origin.MatchOriginUpCameraForward(Vector3.up,start.forward);
                navigator.origin.MoveCameraToWorldLocation(start.position+Vector3.up*navigator.origin.CameraInOriginSpaceHeight);
            }
            Begin();UnityEngine.Object.FindFirstObjectByType<ComfortFollowUI>()?.Recenter();
        }
        public override void SetUserPaused(bool value) {userPaused=value;UpdateBlockers();}
        public override void RequestReturn() {if(ResultVisible)ReturnToMenu();else RequestExit();}
        public override void ReturnToMenu() {Stop();RequestMenu();}
        public override void ApplySettings(float voice,float effects,bool subtitles)
        {if(Guidance!=null) {Guidance.voice.volume=voice;Guidance.subtitle.enabled=subtitles;}}
    }
}
