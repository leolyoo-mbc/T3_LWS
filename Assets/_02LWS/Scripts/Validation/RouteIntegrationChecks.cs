#if UNITY_EDITOR
using System;
using System.IO;
using System.Linq;
using System.Collections;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.XR.Interaction.Toolkit.Locomotion.Teleportation;
using LWS.FireNotification.Runtime;
using LWS.FireNotification.Interaction;
using LWS.FireNotification.Domain.EvacuationRoute;

namespace LWS.FireNotification.Validation
{
    public static class RouteIntegrationChecks
    {
        public static string EvidenceDirectory;
        public static Action<string> CaptureRequested;
        public static bool Running { get; private set; }
        public static int Passed { get; private set; }
        public static int Failed { get; private set; }
        [MenuItem("Tools/LWS/Validate Route Configuration")]
        public static void ValidateConfiguration()
        {
            var nav=UnityEngine.Object.FindFirstObjectByType<ContentSceneNavigator>();
            var c=nav.contents.Single(e=>e.id=="safe-evacuation-route").adapter as EvacuationRouteController;
            var errors=c.ConfigurationIssues().Concat(SharedUIContractAudit.Validate(nav)).ToArray();
            if(errors.Length>0)throw new InvalidOperationException(string.Join("\n",errors));
            Debug.Log("Route configuration PASS");
        }
        [MenuItem("Tools/LWS/Validate Route Integration")]
        public static void RunPlayMode()
        {
            if(!Application.isPlaying||Running)throw new InvalidOperationException("Start Play Mode with no route check running");
            ValidateConfiguration();Passed=0;Failed=0;Running=true;
            if(string.IsNullOrEmpty(EvidenceDirectory))EvidenceDirectory="Artifacts/Validation/VisualEvidence/"+DateTime.Now.ToString("yyyyMMdd-HHmmss")+"-safe-evacuation-route-us3";
            Directory.CreateDirectory(EvidenceDirectory);File.WriteAllText(EvidenceDirectory+"/integration.txt","Editor synthetic tracking and button/teleport inputs, actual scene and audio playback\n");
            var nav=UnityEngine.Object.FindFirstObjectByType<ContentSceneNavigator>();nav.StartCoroutine(Guard(Run(nav)));
        }
        private static IEnumerator Guard(IEnumerator root)
        {
            var stack=new System.Collections.Generic.Stack<IEnumerator>();stack.Push(root);
            while(stack.Count>0)
            {
                var next=stack.Peek();bool moved=false;object current=null;Exception failure=null;
                try {moved=next.MoveNext();if(moved)current=next.Current;}
                catch(Exception error) {failure=error;}
                if(failure!=null)
                {
                    Check(false,"runner exception: "+failure.Message);
                    while(stack.Count>0)(stack.Pop() as IDisposable)?.Dispose();Running=false;yield break;
                }
                if(!moved) {stack.Pop();(next as IDisposable)?.Dispose();continue;}
                if(current is IEnumerator nested)stack.Push(nested);else yield return current;
            }
        }
        private static void Check(bool ok,string name)
        {
            if(ok)Passed++;else Failed++;
            File.AppendAllText(EvidenceDirectory+"/integration.txt",(ok?"PASS ":"FAIL ")+name+"\n");
            if(!ok)Debug.LogError("Route integration: "+name);
        }
        private static IEnumerator Frames(int count=4) {for(int i=0;i<count;i++)yield return null;}
        private static IEnumerator Wait(Func<bool> condition,string name,float timeout=40)
        {
            float end=Time.realtimeSinceStartup+timeout;
            while(!condition()&&Time.realtimeSinceStartup<end)yield return null;
            Check(condition(),name);
            if(!condition())throw new InvalidOperationException("Route integration timed out: "+name);
        }
        private static void Click(SharedContentUI view,string label)
        {
            var b=view.actionRoot.GetComponentsInChildren<FreshPressButton>(true).Single(v=>v.gameObject.activeInHierarchy&&v.GetComponentInChildren<TMPro.TMP_Text>().text==label);
            PointerEventData e=UnityEngine.XR.XRSettings.isDeviceActive ? new UnityEngine.XR.Interaction.Toolkit.UI.TrackedDeviceEventData(EventSystem.current){pointerId=812} : new PointerEventData(EventSystem.current){pointerId=-1};
            b.OnPointerDown(e);b.OnPointerUp(e);b.OnPointerClick(e);
        }
        private static IEnumerator Move(EvacuationRouteController c,string id)
        {
            var w=c.waypoints.Single(p=>p.id==id);Check(w.anchor.enabled,"adjacent anchor enabled: "+id);
            Check(c.teleport.QueueTeleportRequest(new TeleportRequest{destinationPosition=w.anchor.teleportAnchorTransform.position,destinationRotation=w.anchor.teleportAnchorTransform.rotation,matchOrientation=MatchOrientation.TargetUpAndForward}),"teleport queued: "+id);
            yield return Wait(()=>c.Session.CurrentWaypointId==id,"actual arrival: "+id,8);
            yield return Frames();
        }
        private static void Layout(SharedContentUI view,string stage)
        {
            Canvas.ForceUpdateCanvases();view.RefreshLayout();
            foreach(var t in view.panel.GetComponentsInChildren<TMPro.TMP_Text>())
            {
                if(!t.enabled||string.IsNullOrWhiteSpace(t.text))continue;
                float needed=t.GetPreferredValues(t.text,t.rectTransform.rect.width,Mathf.Infinity).y;
                Check(needed<=t.rectTransform.rect.height+2,"text fits "+stage+": "+t.name);
            }
        }
        private static IEnumerator Run(ContentSceneNavigator nav)
        {
            try
            {
                var c=(EvacuationRouteController)nav.contents.Single(e=>e.id=="safe-evacuation-route").adapter;
                var ui=SharedUIController.Active;var view=nav.contentUI;
                nav.lifecycle.DebugIsolateLifecycle(true);c.DebugTracking(true);
                if(nav.InContent) {nav.ActiveAdapter.ReturnToMenu();yield return Wait(()=>!nav.InContent&&!nav.Loading,"menu reset");}
                Check(SharedUIContractAudit.Validate(nav).Count==0,"one shared UI contract");
                nav.StartContent("safe-evacuation-route");yield return Wait(()=>nav.ActiveAdapter==c&&!nav.Loading,"menu entry");yield return Frames();
                Check(c.Session.Stage==RouteStage.AwaitingReady&&!nav.MovementAllowed,"ready locks locomotion without blocking actions");
                Layout(view,"ready");CaptureRequested?.Invoke("01-a-start");yield return Frames(2);
                Click(view,"준비 완료");yield return Wait(()=>c.Session.Stage==RouteStage.Explore,"A intro actual playback ends");
                Check(!c.Session.Choose(c.Session.RunId,c.Session.Execution,"a-left",RouteChoice.Use),"remote choice rejected");
                yield return Move(c,"a-w4");Check(c.Session.CurrentTargetId=="a-lift","lift target after arrival");CaptureRequested?.Invoke("02-a-elevator");yield return Frames(2);
                var before=nav.origin.Camera.transform.position;Click(view,"이용한다");
                Check(c.Session.Stage==RouteStage.Feedback&&!nav.MovementAllowed,"lift feedback locks movement");
                yield return Wait(()=>c.Session.Stage==RouteStage.Decision,"elevator actual feedback ends");Check(Vector3.Distance(before,nav.origin.Camera.transform.position)<.05f,"no elevator entry");
                Click(view,"다른 경로를 찾는다");yield return Frames();Check(c.Session.Stage==RouteStage.Explore&&view.ActionCount==0,"find another suppresses same checkpoint");
                yield return Move(c,"a-w5");yield return Move(c,"a-w6");
                Layout(view,"A decision");CaptureRequested?.Invoke("03-a-stair");yield return Frames(2);
                var previousChoice=view.actionRoot.GetComponentsInChildren<FreshPressButton>().First().selected;
                long stale=c.Session.Execution;Click(view,"다른 경로를 찾는다");yield return Move(c,"a-w5");yield return Move(c,"a-w6");
                Check(!c.Session.Choose(c.Session.RunId,stale,"a-right",RouteChoice.Use),"stale target action rejected");
                Click(view,"이 계단으로 대피한다");yield return Wait(()=>c.Session.CompletionGuidanceDone,"A completion actual playback ends");
                Check(!c.ResultVisible,"A has no final result modal");Click(view,"다음 연습");
                yield return Wait(()=>c.Session.ScenarioIndex==1&&c.Session.Stage==RouteStage.Intro,"B transition before intro");
                Check(!c.spaces[0].activeSelf&&c.spaces[1].activeInHierarchy,"only B space active");
                var start=c.waypoints.Single(w=>w.id=="b-w1").transform.position;var delta=nav.origin.Camera.transform.position-start;delta.y=0;
                Check(delta.magnitude<.1f,"B actual start position");CaptureRequested?.Invoke("04-b-start");yield return Frames(2);
                yield return Wait(()=>c.Session.Stage==RouteStage.Explore,"B intro actual playback ends");
                yield return Move(c,"b-w0");CaptureRequested?.Invoke("05-b-blocked-stair");yield return Frames(2);
                ui.ToggleTextSize();yield return Frames();Layout(view,"150 percent blocked stair");
                ui.ToggleSubtitles();yield return Frames();Check(view.ActionCount==2&&!view.guidance.subtitle.enabled&&!string.IsNullOrEmpty(view.status.text),"subtitles off preserves actions and target");
                ui.ToggleSubtitles();before=nav.origin.Camera.transform.position;Click(view,"이 계단으로 대피한다");yield return Frames(10);
                Check(c.Session.Stage==RouteStage.Feedback&&!nav.MovementAllowed,"smoke feedback locks movement");
                ui.OpenPause();yield return Frames();float elapsed=view.guidance.Elapsed;yield return Frames(8);
                Check(c.Blocked&&Mathf.Abs(view.guidance.Elapsed-elapsed)<.001f,"user pause preserves voice position");ui.Continue();
                yield return Wait(()=>c.Session.Stage==RouteStage.Decision,"smoke actual feedback ends");Check(Vector3.Distance(before,nav.origin.Camera.transform.position)<.05f,"no smoke entry");
                Click(view,"다른 경로를 찾는다");Check(!c.Session.Results[1].DetourPerformed,"no credit before actual detour");
                yield return Move(c,"b-w1");yield return Move(c,"b-w2");
                var p=nav.origin.Camera.transform.position;
                var w3=c.waypoints.Single(w=>w.id=="b-w3");c.teleport.QueueTeleportRequest(new TeleportRequest{destinationPosition=w3.transform.position});
                ui.OpenPause();yield return Frames(10);ui.Continue();yield return Frames(10);
                Check(Vector3.Distance(p,nav.origin.Camera.transform.position)<.05f,"queued teleport canceled on pause");
                float delay=c.teleport.delayTime;c.teleport.delayTime=0;
                Check(c.teleport.QueueTeleportRequest(new TeleportRequest{destinationPosition=w3.transform.position}),"zero-delay teleport queued before tracking loss");
                c.DebugTracking(false);yield return Frames(6);
                Check(c.Blocked&&Vector3.Distance(p,nav.origin.Camera.transform.position)<.05f,"tracking loss cancels request before XRI Update");
                c.DebugTracking(true);c.teleport.delayTime=delay;yield return Frames();
                nav.lifecycle.DebugSuspend(true);yield return Frames();Check(c.Blocked&&!nav.MovementAllowed,"lifecycle blocks movement");nav.lifecycle.DebugSuspend(false);yield return Frames();
                yield return Move(c,"b-w3");yield return Move(c,"b-w4");CaptureRequested?.Invoke("06-b-corridor");yield return Frames(2);
                yield return Move(c,"b-w5");yield return Move(c,"b-w6");CaptureRequested?.Invoke("07-b-available-stair");yield return Frames(2);Layout(view,"B 150 decision");
                Click(view,"이 계단으로 대피한다");yield return Wait(()=>c.ResultVisible,"B completion actual playback ends");yield return Frames();
                Check(c.Session.Results[1].Completed&&c.Session.Results[1].DetourPerformed,"actual detour recorded");
                Check(c.Session.Results[1].Decisions[0].FeedbackRequired&&c.ResultText.Contains("피드백 후 완료"),"first wrong preserved in result");
                Check(ui.resultText.text.Contains("실제 대피 완료는 아닙니다"),"shared result limitation");CaptureRequested?.Invoke("08-result");yield return Frames(2);
                Check(!ui.openMenuPanel.activeSelf,"result has no overlapping menu shortcut");
                Check(ui.resultText.GetPreferredValues(ui.resultText.text,ui.resultText.rectTransform.rect.width,Mathf.Infinity).y<=ui.resultText.rectTransform.rect.height,"150 percent result fits allocated height");
                c.DebugTracking(false);yield return Frames();
                Check(ui.openMenuPanel.activeSelf&&view.panel.activeSelf&&view.status.text.Contains("추적"),"result tracking loss retains menu and recovery instruction");
                c.DebugTracking(true);yield return Frames();
                Check(ui.resultPanel.activeSelf&&!ui.openMenuPanel.activeSelf,"tracking recovery restores unobstructed result");
                ui.ToggleTextSize();ui.CompletedRestart();yield return Frames();
                previousChoice.Invoke();yield return Frames();
                Check(c.Session.Stage==RouteStage.AwaitingReady&&c.Session.Results.All(r=>r.Decisions.Count==0),"restart resets both results and A space");
                c.ReturnToMenu();yield return Wait(()=>!nav.InContent&&!nav.Loading,"menu return");
                Check(!view.guidance.voice.isPlaying&&c.waypoints.All(w=>!w.anchor.enabled),"stop cleans voice and anchors");
                // Independent real scene paths: a direct completion and a wrong choice followed by
                // leaving the blocked checkpoint without requesting another route (no detour credit).
                for(int attempt=0;attempt<2;attempt++)
                {
                    nav.StartContent("safe-evacuation-route");yield return Wait(()=>nav.ActiveAdapter==c&&!nav.Loading,"independent route entry "+attempt);yield return Frames();
                    if(attempt==0)
                    {
                        for(int volume=0;volume<10;volume++)ui.VoiceDown();
                        Check(view.guidance.voice.volume<.001f,"voice zero setting");
                        for(int volume=0;volume<10;volume++)ui.VoiceUp();
                    }
                    Click(view,"준비 완료");yield return Wait(()=>c.Session.Stage==RouteStage.Explore,"independent A intro "+attempt);
                    yield return Move(c,"a-w4");yield return Move(c,"a-w5");yield return Move(c,"a-w6");
                    var staleAction=view.actionRoot.GetComponentsInChildren<FreshPressButton>().First().selected;
                    Click(view,"이 계단으로 대피한다");staleAction.Invoke();
                    Check(c.Session.Results[0].Decisions.Count==1,"rapid duplicate choice counted once");
                    yield return Wait(()=>c.Session.CompletionGuidanceDone,"independent A completion "+attempt);
                    Click(view,"다음 연습");yield return Wait(()=>c.Session.ScenarioIndex==1&&c.Session.Stage==RouteStage.Explore,"independent B intro "+attempt);
                    if(attempt==1)
                    {
                        yield return Move(c,"b-w0");Click(view,"이 계단으로 대피한다");
                        yield return Wait(()=>c.Session.Stage==RouteStage.Decision,"independent smoke feedback");
                        // Walking/teleporting away is allowed in Decision; it is not the FindAnotherRoute action.
                        yield return Move(c,"b-w1");
                    }
                    yield return Move(c,"b-w2");yield return Move(c,"b-w3");yield return Move(c,"b-w4");yield return Move(c,"b-w5");yield return Move(c,"b-w6");
                    Click(view,"이 계단으로 대피한다");yield return Wait(()=>c.ResultVisible,"independent result "+attempt);
                    Check(!c.Session.Results[1].DetourPerformed,"no detour credit without full action sequence "+attempt);
                    Check(c.Session.Results[1].Decisions.Any(d=>d.FeedbackRequired)==(attempt==1),"direct versus feedback result "+attempt);
                    if(attempt==0)Check(c.Session.Results[1].Decisions.Count==1,"no mandatory blocked stair or lift visit");
                    ui.CompletedReturn();yield return Wait(()=>!nav.InContent&&!nav.Loading,"independent menu "+attempt);
                }
                nav.StartContent("safe-evacuation-route");yield return Wait(()=>nav.ActiveAdapter==c&&!nav.Loading,"transition fault test entry");yield return Frames();
                c.Session.Ready(c.Session.RunId,c.Session.Execution);c.Session.GuidanceDone(c.Session.RunId,c.Session.Execution);
                c.Session.Arrive(c.Session.RunId,c.Session.Execution,"a-w6");c.Session.Choose(c.Session.RunId,c.Session.Execution,"a-right",RouteChoice.Use);c.Session.GuidanceDone(c.Session.RunId,c.Session.Execution);
                var savedB=c.spaces[1];c.spaces[1]=null;c.Session.NextScenario(c.Session.RunId,c.Session.Execution);yield return Frames();
                Check(c.Session.Stage==RouteStage.Transition&&(c.Session.Blockers&RouteBlocker.Configuration)!=0,"failed transition does not start B guidance");
                Check(view.status.text.Contains("메뉴"),"transition failure has menu recovery instruction");
                c.spaces[1]=savedB;c.ReturnToMenu();yield return Wait(()=>!nav.InContent&&!nav.Loading,"failed transition menu recovery");
                Check(SharedUIContractAudit.Validate(nav).Count==0,"final shared contract");
            }
            finally
            {
                Running=false;File.AppendAllText(EvidenceDirectory+"/integration.txt","DONE passed="+Passed+" failed="+Failed+" frame="+Time.frameCount+"\n");
            }
        }
    }
}
#endif
