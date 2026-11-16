#if UNITY_EDITOR
using System;
using System.IO;
using System.Linq;
using System.Collections;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.XR.Interaction.Toolkit.Interactors;
using UnityEngine.XR.Interaction.Toolkit.Inputs.Readers;
using LWS.FireNotification.Interaction;
using LWS.FireNotification.Domain.EvacuationRoute;
using LWS.FireNotification.Runtime;

namespace LWS.FireNotification.Validation
{
    public static partial class FloorButtonChecks
    {
        public static bool Running { get; private set; }
        public static int Passed { get; private set; }
        public static int Failed { get; private set; }
        public static string EvidenceDirectory;
        public static Action<string> CaptureRequested;
        [MenuItem("Tools/LWS/Validate Floor Buttons")]
        public static void ValidateConfiguration()
        {
            var nav=UnityEngine.Object.FindFirstObjectByType<ContentSceneNavigator>();
            if(nav==null)throw new InvalidOperationException("Missing navigator");
            var canvases=nav.gameObject.scene.GetRootGameObjects().SelectMany(r=>r.GetComponentsInChildren<Canvas>(true)).Where(c=>(c.hideFlags&HideFlags.DontSave)==0).ToArray();
            if(canvases.Length!=2)throw new InvalidOperationException("Expected two shared Canvases, found "+canvases.Length);
            var errors=SharedUIContractAudit.Validate(nav);
            foreach(var root in nav.gameObject.scene.GetRootGameObjects())foreach(var anchor in root.GetComponentsInChildren<FloorButtonAnchor>(true))errors.AddRange(anchor.ConfigurationIssues());
            foreach(var entry in nav.contents)
            {
                if(entry.adapter is FireNotificationAdapter f&&(f.choiceAnchor==null||f.nextAnchor==null))errors.Add("Missing fire floor anchors");
                if(entry.adapter is SmokePostureController s&&s.readyAnchor==null)errors.Add("Missing smoke floor anchor");
                if(entry.adapter is EvacuationRouteController r)errors.AddRange(r.ConfigurationIssues());
            }
            if(errors.Count>0)throw new InvalidOperationException(string.Join("\n",errors));
        }
        [MenuItem("Tools/LWS/Run Floor Button Play Mode Checks")]
        public static void RunPlayMode()
        {
            if(!Application.isPlaying||Running)throw new InvalidOperationException("Start Play Mode with no floor checks running");
            EvidenceDirectory=ValidationEvidence.CreateRunDirectory(EvidenceDirectory,"floor-choice-buttons");
            Passed=Failed=0;Running=true;
            File.WriteAllText(EvidenceDirectory+"/floor-checks.txt","Editor synthetic tracking, real floor presenter and actual XR UI ray pipeline. Device acceptance separate.\n");
            UnityEngine.Object.FindFirstObjectByType<ContentSceneNavigator>().StartCoroutine(Guard(Run()));
        }
        private static IEnumerator Guard(IEnumerator root)
        {
            var stack=new System.Collections.Generic.Stack<IEnumerator>();stack.Push(root);
            try
            {
                while(stack.Count>0)
                {
                    var next=stack.Peek();bool moved=false;object current=null;Exception error=null;
                    try {moved=next.MoveNext();if(moved)current=next.Current;}catch(Exception e){error=e;}
                    if(error!=null){Check(false,error.ToString());yield break;}
                    if(!moved){(stack.Pop() as IDisposable)?.Dispose();continue;}
                    if(current is IEnumerator nested)stack.Push(nested);else yield return current;
                }
            }
            finally {while(stack.Count>0)(stack.Pop() as IDisposable)?.Dispose();Running=false;}
        }
        private static void Check(bool pass,string name)
        {
            if(pass)Passed++;else Failed++;
            File.AppendAllText(EvidenceDirectory+"/floor-checks.txt",(pass?"PASS ":"FAIL ")+name+"\n");
            if(!pass)Debug.LogError("Floor checks: "+name);
        }
        private static IEnumerator Wait(Func<bool> condition,string name,float seconds=15)
        {
            float end=Time.realtimeSinceStartup+seconds;
            while(!condition()&&Time.realtimeSinceStartup<end)yield return null;
            Check(condition(),name);if(!condition())throw new TimeoutException(name);
        }
        private static IEnumerator Run()
        {
            ValidateConfiguration();
            var nav=UnityEngine.Object.FindFirstObjectByType<ContentSceneNavigator>();
            var route=(EvacuationRouteController)nav.contents.Single(e=>e.id=="safe-evacuation-route").adapter;
            var ui=SharedUIController.Active;var view=nav.contentUI;var p=view.floorButtons;
            nav.lifecycle.DebugIsolateLifecycle(true);route.DebugTracking(true);
            if(nav.InContent){nav.ActiveAdapter.ReturnToMenu();yield return Wait(()=>!nav.InContent&&!nav.Loading,"reset to menu");}
            nav.StartContent("safe-evacuation-route");yield return Wait(()=>nav.ActiveAdapter==route&&!nav.Loading,"route entry");
            yield return Wait(()=>p.Phase==FloorButtonPhase.Ready,"route ready rises");yield return null;
            Check(view.ActionCount==1&&p.buttons[0].gameObject.activeInHierarchy&&!p.buttons[1].gameObject.activeSelf,"single ready button");
            int calls=0;Action callback=()=>calls++;
            var first=new ContentUIAction("test-left","이 계단으로 대피한다",callback);
            var second=new ContentUIAction("test-right","다른 경로를 찾는다",callback);
            view.SetActions(route,route.readyAnchor,first,second);
            yield return Wait(()=>p.Phase==FloorButtonPhase.Rising,"two choices begin rising");
            var risingEvent=p.buttons[0].selected;risingEvent.Invoke();
            Check(calls==0,"Rising listener rejected");
            float before=p.Progress;view.SetActions(route,route.readyAnchor,first,second);
            Check(p.Progress==before,"same request does not restart rise");
            yield return Wait(()=>p.Phase==FloorButtonPhase.Ready,"two choices ready");yield return null;
            var floorEdge=p.viewport.TransformPoint(new Vector3(0,p.viewport.rect.yMin,0));
            Check(Mathf.Abs(floorEdge.y-(p.DisplayAnchor.transform.position.y+p.DisplayAnchor.floorClearance))<.001f,"viewport lower edge at floor clearance");
            Check(Mathf.Abs(p.movingPanel.TransformPoint(Vector3.zero).y-(p.DisplayAnchor.transform.position.y+p.DisplayAnchor.raisedBottomHeight))<.001f,"raised panel lower edge matches anchor height");
            var buttonCorners=new Vector3[4];((RectTransform)p.buttons[0].transform).GetWorldCorners(buttonCorners);
            Check(Mathf.Abs(buttonCorners[0].y-(p.DisplayAnchor.transform.position.y+p.DisplayAnchor.raisedBottomHeight))<.001f,"actual button lower edge matches anchor height");
            var position=p.floorCanvas.transform.position;var rotation=p.floorCanvas.transform.rotation;
            var head=nav.origin.Camera.transform;var headRotation=head.localRotation;head.localRotation=Quaternion.Euler(0,180,0);yield return null;
            Check(p.floorCanvas.transform.position==position&&p.floorCanvas.transform.rotation==rotation,"head rotation cannot move floor UI");head.localRotation=headRotation;
            foreach(float scale in new[]{1f,1.5f})
            {
                p.ApplySettings(scale);Canvas.ForceUpdateCanvases();
                foreach(var b in p.buttons)
                {
                    var text=b.GetComponentInChildren<TMPro.TMP_Text>();
                    Check(text.GetPreferredValues(text.text,text.rectTransform.rect.width,Mathf.Infinity).y<=text.rectTransform.rect.height+1,"full text fits at "+scale);
                }
            }
            p.ApplySettings(ui.TextScale);
            route.Session.SetBlockers(RouteBlocker.UserPause);
            p.buttons[0].selected.Invoke();
            Check(p.Phase==FloorButtonPhase.Ready&&calls==0,"blocked owner cannot consume display before frame refresh");
            route.Session.SetBlockers(RouteBlocker.None);
            var acceptedEvent=p.buttons[0].selected;acceptedEvent.Invoke();p.buttons[1].selected.Invoke();acceptedEvent.Invoke();
            Check(calls==1&&p.Phase==FloorButtonPhase.Lowering,"entire group atomic lock, callback immediate once");
            Check(!view.IsFloorExitComplete(route),"accepted action not completed before lower");
            float progress=p.Progress;ui.OpenPause();yield return new WaitForSecondsRealtime(.12f);
            Check(p.Phase==FloorButtonPhase.Lowering&&Mathf.Abs(p.Progress-progress)<.001f&&p.group.alpha==0,"pause hides and freezes outgoing panel");
            ui.Continue();yield return Wait(()=>p.Phase==FloorButtonPhase.Rising,"unchanged rejected request is offered again");
            Check(calls==1,"animation does not execute callback");
            yield return Wait(()=>p.Phase==FloorButtonPhase.Ready,"retry ready");yield return null;
            acceptedEvent.Invoke();Check(calls==1,"old ticket rejected after retry");
            Action reentrant=()=>
            {
                calls++;
                view.SetActions(route);
                view.SetActions(route,route.readyAnchor,new ContentUIAction("discard","중간 요청",callback));
                view.SetActions(route,route.readyAnchor,new ContentUIAction("latest","마지막 요청",callback));
            };
            view.SetActions(route,route.readyAnchor,new ContentUIAction("reentrant","준비 완료",reentrant));
            yield return Wait(()=>p.Phase==FloorButtonPhase.Ready&&p.buttons[0].GetComponentInChildren<TMPro.TMP_Text>().text=="준비 완료","reentrant action ready");yield return null;
            p.buttons[0].selected.Invoke();
            Check(calls==2&&p.buttons[0].GetComponentInChildren<TMPro.TMP_Text>().text=="준비 완료","reentrant updates preserve outgoing label");
            yield return Wait(()=>p.Phase==FloorButtonPhase.Ready&&p.buttons[0].GetComponentInChildren<TMPro.TMP_Text>().text=="마지막 요청","only latest pending request rises");
            var stale=p.buttons[0].selected;view.ResetFloorDisplay();stale.Invoke();
            Check(p.Phase==FloorButtonPhase.Hidden&&!view.IsFloorExitComplete(route)&&calls==2,"reset cancels callbacks and never marks exit complete");
            route.Restart();yield return Wait(()=>p.Phase==FloorButtonPhase.Ready,"restart initial ready");yield return null;
            yield return PrepareNext(nav,route);
            Check(p.Phase==FloorButtonPhase.Ready,"next practice offered");
            p.buttons[0].selected.Invoke();
            yield return null;
            Check(route.Session.Stage==RouteStage.Transition&&route.spaces[0].activeInHierarchy&&!route.spaces[1].activeSelf,"next practice keeps A active until lower complete");
            ui.OpenPause();progress=p.Progress;yield return new WaitForSecondsRealtime(.12f);
            Check(route.spaces[0].activeInHierarchy&&p.Progress==progress,"next practice pause preserves A and progress");ui.Continue();
            yield return Wait(()=>route.Session.ScenarioIndex==1,"next practice moves after normal lower");
            Check(!route.spaces[0].activeSelf&&route.spaces[1].activeSelf&&view.ActionCount==0,"B has no new ready or next button");
            route.Restart();yield return Wait(()=>p.Phase==FloorButtonPhase.Ready,"restart for reset transition test");
            yield return PrepareNext(nav,route);p.buttons[0].selected.Invoke();view.ResetFloorDisplay();yield return new WaitForSecondsRealtime(.5f);
            Check(route.spaces[0].activeInHierarchy&&route.Session.Stage==RouteStage.Transition,"forced reset is not a successful lowering signal");
            route.Restart();yield return Wait(()=>p.Phase==FloorButtonPhase.Ready,"restart cancels waiting transition");
            Check(route.Session.Stage==RouteStage.AwaitingReady&&route.spaces[0].activeSelf&&!route.spaces[1].activeSelf,"restart cancels pending B movement");
            route.ReturnToMenu();yield return Wait(()=>!nav.InContent&&!nav.Loading,"floor checks return to menu");
            Check(p.Phase==FloorButtonPhase.Hidden&&view.ActionCount==0,"exit removes floor actions");
            yield return RunRaycasts(nav);
            yield return RunContentChecks(nav);
            yield return RunInputChecks(nav);
            yield return RunLifecycleCoverage(nav);
            yield return RunTiltCoverage(nav);
            File.AppendAllText(EvidenceDirectory+"/floor-checks.txt","DONE passed="+Passed+" failed="+Failed+"\n");
        }
private static IEnumerator RunRaycasts(ContentSceneNavigator nav)
{
 var ep=EvidenceDirectory;
 var view=nav.contentUI;var p=view.floorButtons;var route=(EvacuationRouteController)nav.contents.Single(e=>e.id=="safe-evacuation-route").adapter;
 int passed=0,failed=0,calls=0;GameObject rayGO=null,wall=null;
 void Check(bool ok,string name){FloorButtonChecks.Check(ok,name);if(ok)passed++;else failed++;File.AppendAllText(ep+"/xr-rays.txt",(ok?"PASS ":"FAIL ")+name+"\n");}
 IEnumerator Frames(int n=4){for(int i=0;i<n;i++)yield return null;}
 IEnumerator Wait(Func<bool> condition){float end=Time.realtimeSinceStartup+10;while(!condition()&&Time.realtimeSinceStartup<end)yield return null;if(!condition())throw new Exception("Ray test Ready timeout");yield return Frames();}
 try
 {
  nav.lifecycle.DebugIsolateLifecycle(true);route.DebugTracking(true);
  if(nav.InContent){nav.ActiveAdapter.ReturnToMenu();yield return Wait(()=>!nav.InContent&&!nav.Loading);}
  nav.StartContent("safe-evacuation-route");yield return Wait(()=>nav.ActiveAdapter==route&&!nav.Loading);
  File.WriteAllText(ep+"/xr-rays.txt","Actual XRRayInteractor + TrackedDeviceGraphicRaycaster + existing XRUIInputModule. Manual UI input reader, synthetic hand pose. Not physical controller.\n");
  foreach(int count in new[]{1,2})foreach(int hand in new[]{0,1})
  {
   view.ResetFloorDisplay();
   var a=new ContentUIAction("one","준비 완료",()=>calls++);var b=new ContentUIAction("two","다른 경로를 찾는다",()=>calls++);
   view.SetActions(route,route.readyAnchor,count==1?new[]{a}:new[]{a,b});
   yield return Wait(()=>p.Phase==FloorButtonPhase.Ready);
   rayGO=new GameObject(hand==0?"Floor validation left ray":"Floor validation right ray");rayGO.SetActive(false);
   var ray=rayGO.AddComponent<XRRayInteractor>();ray.enableUIInteraction=true;ray.raycastMask=Physics.DefaultRaycastLayers;ray.maxRaycastDistance=10;
   ray.uiPressInput.inputSourceMode=XRInputButtonReader.InputSourceMode.ManualValue;
   var target=p.buttons[hand==1&&count==2?1:0].transform.position;
   var origin=nav.origin.Camera.transform.position+new Vector3(hand==0?-.2f:.2f,-.2f,0);
   rayGO.transform.SetPositionAndRotation(origin,Quaternion.LookRotation(target-origin));rayGO.SetActive(true);
   yield return Frames(8);
   bool hit=ray.TryGetCurrentUIRaycastResult(out var result);
   Check(hit&&result.gameObject==p.buttons[hand==1&&count==2?1:0].gameObject,"UI ray hit count="+count+" hand="+hand+" actual="+(hit?result.gameObject.name:"none"));
   int previous=calls;ray.uiPressInput.QueueManualState(true,1);yield return Frames(3);ray.uiPressInput.QueueManualState(false,0);yield return Frames(3);
   Check(calls==previous+1,"event module invoked exactly once count="+count+" hand="+hand);
   ray.uiPressInput.QueueManualState(true,1);yield return Frames(2);ray.uiPressInput.QueueManualState(false,0);yield return Frames(2);
   Check(calls==previous+1,"lowering cannot execute again");
   UnityEngine.Object.Destroy(rayGO);rayGO=null;
  }
  view.ResetFloorDisplay();view.SetActions(route,route.readyAnchor,new ContentUIAction("wall","준비 완료",()=>calls++));yield return Wait(()=>p.Phase==FloorButtonPhase.Ready);
  rayGO=new GameObject("Floor wall occlusion ray");rayGO.SetActive(false);var blockedRay=rayGO.AddComponent<XRRayInteractor>();blockedRay.enableUIInteraction=true;blockedRay.raycastMask=Physics.DefaultRaycastLayers;blockedRay.uiPressInput.inputSourceMode=XRInputButtonReader.InputSourceMode.ManualValue;
  var goal=p.buttons[0].transform.position;var source=nav.origin.Camera.transform.position;
  rayGO.transform.SetPositionAndRotation(source,Quaternion.LookRotation(goal-source));rayGO.SetActive(true);
  wall=GameObject.CreatePrimitive(PrimitiveType.Cube);wall.name="Temporary floor occlusion test wall";wall.transform.position=Vector3.Lerp(source,goal,.5f);wall.transform.localScale=Vector3.one*.4f;
  Physics.SyncTransforms();yield return Frames(8);
  bool blocked=blockedRay.TryGetCurrentUIRaycastResult(out var blockedHit);Check(!blocked||blockedHit.gameObject!=p.buttons[0].gameObject,"3D wall blocks tracked UI hit");
  int old=calls;blockedRay.uiPressInput.QueueManualState(true,1);yield return Frames(3);blockedRay.uiPressInput.QueueManualState(false,0);yield return Frames(3);Check(calls==old,"wall prevents callback");
 }
 finally
 {
  if(rayGO!=null)UnityEngine.Object.Destroy(rayGO);if(wall!=null)UnityEngine.Object.Destroy(wall);
  view.ResetFloorDisplay();File.AppendAllText(ep+"/xr-rays.txt","DONE passed="+passed+" failed="+failed+"\n");
 }
}

private static IEnumerator RunInputChecks(ContentSceneNavigator nav){
 var ep=EvidenceDirectory;
 var view=nav.contentUI;var p=view.floorButtons;var ui=SharedUIController.Active;var route=(EvacuationRouteController)nav.contents.Single(e=>e.id=="safe-evacuation-route").adapter;
 InputDevice left=null,right=null;int passed=0,failed=0,calls=0;
 void Check(bool ok,string name){FloorButtonChecks.Check(ok,name);if(ok)passed++;else failed++;File.AppendAllText(ep+"/fresh-input.txt",(ok?"PASS ":"FAIL ")+name+"\n");}
 void Press(ButtonControl control,float value){using(StateEvent.From(control.device,out var ev)){control.WriteValueIntoEvent(value,ev);InputSystem.QueueEvent(ev);}}
 IEnumerator Frames(int n=4){for(int i=0;i<n;i++)yield return null;}
 IEnumerator Wait(Func<bool> f){float end=Time.realtimeSinceStartup+10;while(!f()&&Time.realtimeSinceStartup<end)yield return null;if(!f())throw new Exception("Fresh input timeout");yield return Frames(2);}
 void Pointer(){var e=new UnityEngine.XR.Interaction.Toolkit.UI.TrackedDeviceEventData(EventSystem.current){pointerId=952};p.buttons[0].OnPointerDown(e);p.buttons[0].OnPointerClick(e);p.buttons[0].OnPointerUp(e);}
 try{
  File.WriteAllText(ep+"/fresh-input.txt","Actual InputSystem OculusTouchController trigger/menu state events, synthetic pointer calls tested separately from actual XR rays.\n");
  nav.lifecycle.DebugIsolateLifecycle(true);route.DebugTracking(true);
  if(nav.InContent){nav.ActiveAdapter.ReturnToMenu();yield return Wait(()=>!nav.InContent&&!nav.Loading);}
  nav.StartContent("safe-evacuation-route");yield return Wait(()=>nav.ActiveAdapter==route&&!nav.Loading);
  left=InputSystem.AddDevice("OculusTouchController");InputSystem.SetDeviceUsage(left,"LeftHand");right=InputSystem.AddDevice("OculusTouchController");InputSystem.SetDeviceUsage(right,"RightHand");
  foreach(var d in new[]{left,right}){
   var trigger=(ButtonControl)d["triggerPressed"];view.ResetFloorDisplay();Press(trigger,1);yield return Frames();
   view.SetActions(route,route.readyAnchor,new ContentUIAction("fresh","준비 완료",()=>{calls++;view.SetActions(route);}));yield return Wait(()=>p.Phase==FloorButtonPhase.Ready);
   int before=calls;Pointer();yield return Frames(2);Check(calls==before,"held through rise rejected "+d.usages[0]);
   Press(trigger,0);yield return Frames();Press(trigger,1);yield return Frames(2);Pointer();yield return Frames(2);Check(calls==before+1,"release then new press accepted "+d.usages[0]);Press(trigger,0);yield return Frames();
  }
  view.ResetFloorDisplay();view.SetActions(route,route.readyAnchor,new ContentUIAction("pause","준비 완료",()=>{calls++;view.SetActions(route);}));yield return Wait(()=>p.Phase==FloorButtonPhase.Ready);
  var lt=(ButtonControl)left["triggerPressed"];ui.OpenPause();Press(lt,1);yield return Frames();ui.Continue();yield return Frames();int old=calls;Pointer();yield return Frames(2);Check(calls==old,"held through pause resume rejected");Press(lt,0);yield return Frames();Press(lt,1);yield return Frames(2);Pointer();yield return Frames(2);Check(calls==old+1,"fresh press after pause accepted");Press(lt,0);yield return Frames();
  var menu=(ButtonControl)left["menu"];Press(menu,0);yield return Frames();
  foreach(var phase in new[]{FloorButtonPhase.Rising,FloorButtonPhase.Ready,FloorButtonPhase.Lowering}){
   view.ResetFloorDisplay();view.SetActions(route,route.readyAnchor,new ContentUIAction("menu","준비 완료",()=>view.SetActions(route)));
   if(phase!=FloorButtonPhase.Rising)yield return Wait(()=>p.Phase==FloorButtonPhase.Ready);
   if(phase==FloorButtonPhase.Lowering)p.buttons[0].selected.Invoke();
   Check(p.Phase==phase,"requested phase before Menu "+phase);
   Press(menu,1);yield return Frames(2);float progress=p.Progress;var actual=p.Phase;Check(actual==phase,"Menu reached requested phase "+phase);yield return Frames(4);Check(ui.IsOpen&&p.Phase==actual&&p.Progress==progress&&p.group.alpha==0,"left Menu freezes "+phase);
   Press(menu,0);yield return Frames(2);Press(menu,1);yield return Frames(2);Check(!ui.IsOpen,"left Menu resumes "+phase);Press(menu,0);yield return Frames(2);
  }
  nav.lifecycle.DebugSuspend(true);yield return Frames();var blockedPhase=p.Phase;float bp=p.Progress;yield return Frames();Check(p.Phase==blockedPhase&&p.Progress==bp&&p.group.alpha==0,"lifecycle blocker freezes floor");nav.lifecycle.DebugSuspend(false);yield return Frames();
  route.ReturnToMenu();yield return Wait(()=>!nav.InContent&&!nav.Loading);
 }finally{if(left!=null)InputSystem.RemoveDevice(left);if(right!=null)InputSystem.RemoveDevice(right);if(ui.IsOpen)ui.Continue();view.ResetFloorDisplay();File.AppendAllText(ep+"/fresh-input.txt","DONE passed="+passed+" failed="+failed+"\n");}
}

        private static IEnumerator PrepareNext(ContentSceneNavigator nav,EvacuationRouteController route)
        {
            var s=route.Session;s.Ready(s.RunId,s.Execution);s.GuidanceDone(s.RunId,s.Execution);
            var waypoint=route.waypoints.Single(w=>w.id=="a-w6");
            nav.origin.MoveCameraToWorldLocation(waypoint.transform.position+Vector3.up*nav.origin.CameraInOriginSpaceHeight);
            s.Arrive(s.RunId,s.Execution,"a-w6");s.Choose(s.RunId,s.Execution,"a-right",RouteChoice.Use);s.GuidanceDone(s.RunId,s.Execution);
            yield return Wait(()=>nav.contentUI.floorButtons.Phase==FloorButtonPhase.Ready&&nav.contentUI.floorButtons.buttons[0].GetComponentInChildren<TMPro.TMP_Text>().text=="다음 연습","next practice rises at completed stair");
            yield return null;
        }
        private static IEnumerator RunContentChecks(ContentSceneNavigator nav)
        {
            var view=nav.contentUI;var p=view.floorButtons;var ui=SharedUIController.Active;
            if(nav.InContent){nav.ActiveAdapter.ReturnToMenu();yield return Wait(()=>!nav.InContent&&!nav.Loading,"leave ray test content");}
            nav.StartContent("fire-notification");yield return Wait(()=>nav.InContent&&!nav.Loading,"fire entry");
            var fire=(FireNotificationAdapter)nav.ActiveAdapter;
            foreach(var stage in new[]{Domain.ExperienceStage.AwaitAction,Domain.ExperienceStage.AwaitNext})
            {
                fire.controller.DebugEnter(stage);
                yield return Wait(()=>p.Phase==FloorButtonPhase.Ready,"fire panel ready "+stage);
                var phase=p.Phase;float progress=p.Progress;int count=view.ActionCount;
                ui.OpenPause();yield return new WaitForSecondsRealtime(.08f);
                Check(p.Phase==phase&&p.Progress==progress&&view.ActionCount==count&&p.group.alpha==0,"fire pause preserves phase and request "+stage);
                ui.Continue();yield return null;
                Check(p.Phase==phase&&view.ActionCount==count,"fire resume preserves request "+stage);
            }
            fire.controller.DebugEnter(Domain.ExperienceStage.AwaitAction);
            yield return Wait(()=>p.Phase==FloorButtonPhase.Ready&&view.ActionCount==2,"fire choices ready for execution reset");
            var stale=p.buttons[0].selected;
            fire.controller.DebugEnter(Domain.ExperienceStage.AwaitAction);
            yield return Wait(()=>p.Phase==FloorButtonPhase.Ready,"same stage new execution ready");
            stale.Invoke();yield return null;yield return null;
            Check(fire.controller.Session.Stage==Domain.ExperienceStage.AwaitAction,"old fire listener cannot control new execution of same stage");
            fire.Restart();yield return null;fire.controller.DebugEnter(Domain.ExperienceStage.AwaitAction);
            yield return Wait(()=>p.Phase==FloorButtonPhase.Ready,"fire wrong answer choices ready");
            p.buttons[1].selected.Invoke();yield return null;yield return null;
            Check(fire.controller.Session.FeedbackActive,"fire wrong answer still plays feedback");
            yield return Wait(()=>!fire.controller.Session.FeedbackActive&&p.Phase==FloorButtonPhase.Ready,"fire wrong answer choices rise again",30);
            p.buttons[0].selected.Invoke();yield return null;yield return null;
            Check(fire.controller.Session.Stage==Domain.ExperienceStage.ShoutResult,"fire correct choice preserves shout result");
            fire.controller.DebugEnter(Domain.ExperienceStage.AwaitNext);
            yield return Wait(()=>p.Phase==FloorButtonPhase.Ready&&view.ActionCount==1,"fire next ready");
            p.buttons[0].selected.Invoke();yield return null;yield return null;
            Check(fire.controller.Session.Stage!=Domain.ExperienceStage.AwaitNext,"fire next progresses immediately");
            fire.ReturnToMenu();yield return Wait(()=>!nav.InContent&&!nav.Loading,"leave fire");
            nav.StartContent("smoke-low-posture");yield return Wait(()=>nav.InContent&&!nav.Loading,"smoke entry");
            var smoke=(SmokePostureController)nav.ActiveAdapter;smoke.DebugPose(1.7);
            yield return Wait(()=>p.Phase==FloorButtonPhase.Ready,"smoke ready rises without saving height");
            Check(!smoke.Session.BaselineHeight.HasValue,"rising never records ready height");
            ui.OpenPause();float elapsed=view.guidance.Elapsed;float phaseProgress=p.Progress;yield return new WaitForSecondsRealtime(.08f);
            Check(p.Phase==FloorButtonPhase.Ready&&p.Progress==phaseProgress&&p.group.alpha==0&&Mathf.Abs(view.guidance.Elapsed-elapsed)<.001f,"smoke pause preserves panel and narration");
            ui.Continue();yield return null;smoke.DebugPose(1.55);p.buttons[0].selected.Invoke();
            Check(smoke.Session.BaselineHeight.HasValue&&Math.Abs(smoke.Session.BaselineHeight.Value-1.55)<.0001&&p.Phase==FloorButtonPhase.Lowering,"smoke height saved immediately at actual click");
            smoke.ReturnToMenu();yield return Wait(()=>!nav.InContent&&!nav.Loading,"leave smoke");
            Check(p.Phase==FloorButtonPhase.Hidden,"owner exit clears panel");
        }
    }
}
#endif
