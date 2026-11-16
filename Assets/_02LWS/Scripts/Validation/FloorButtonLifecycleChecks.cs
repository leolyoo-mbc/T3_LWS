#if UNITY_EDITOR
using System;
using System.Linq;
using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using LWS.FireNotification.Runtime;
using LWS.FireNotification.Domain.EvacuationRoute;

namespace LWS.FireNotification.Validation
{
    public static partial class FloorButtonChecks
    {
        private static IEnumerator RunLifecycleCoverage(ContentSceneNavigator nav)
        {
            var route=(EvacuationRouteController)nav.contents.Single(e=>e.id=="safe-evacuation-route").adapter;
            var outsider=(FireNotificationAdapter)nav.contents.Single(e=>e.id=="fire-notification").adapter;
            var view=nav.contentUI;var panel=view.floorButtons;var ui=SharedUIController.Active;
            Keyboard keyboard=null;var previousKeyboard=Keyboard.current;int calls=0;
            try
            {
                route.DebugTracking(true);nav.StartContent("safe-evacuation-route");
                yield return Wait(()=>nav.ActiveAdapter==route&&!nav.Loading,"coverage route entry");
                view.ResetFloorDisplay();
                nav.origin.MatchOriginUpCameraForward(Vector3.up,-route.readyAnchor.transform.forward);
                view.SetActions(route,route.readyAnchor,new ContentUIAction("owner","준비 완료",()=>calls++));
                Check(Vector3.Dot(nav.origin.Camera.transform.forward,route.readyAnchor.transform.forward)<-.9f,"opposite direction before request starts rising");
                Check(panel.Phase==FloorButtonPhase.Rising,"opposite entry still starts rising");
                yield return Wait(()=>panel.Phase==FloorButtonPhase.Ready,"opposite entry reaches Ready");
                long ticket=panel.DisplayTicket;
                view.SetActions(outsider,outsider.choiceAnchor,new ContentUIAction("outsider","다음",()=>calls++));view.Clear(outsider);
                Check(view.Owner==route&&panel.DisplayTicket==ticket&&view.ActionCount==1&&calls==0,"non-owner SetActions and Clear ignored");
                var oldOwnerEvent=panel.buttons[0].selected;
                route.ReturnToMenu();yield return Wait(()=>!nav.InContent&&!nav.Loading,"owner swap menu");
                nav.StartContent("smoke-low-posture");yield return Wait(()=>nav.InContent&&!nav.Loading,"owner swap smoke");
                var smoke=(SmokePostureController)nav.ActiveAdapter;smoke.DebugPose(1.7);
                yield return Wait(()=>panel.Phase==FloorButtonPhase.Ready,"new owner Ready");
                oldOwnerEvent.Invoke();
                Check(calls==0&&view.Owner==smoke&&!smoke.Session.BaselineHeight.HasValue,"previous owner listener cannot execute new owner action");
                smoke.ReturnToMenu();yield return Wait(()=>!nav.InContent&&!nav.Loading,"leave swapped owner");
                route.DebugTracking(true);nav.StartContent("safe-evacuation-route");yield return Wait(()=>nav.ActiveAdapter==route&&!nav.Loading,"Esc route entry");
                yield return Wait(()=>panel.Phase==FloorButtonPhase.Ready,"Esc panel Ready");
                keyboard=InputSystem.AddDevice<Keyboard>();
                InputSystem.QueueStateEvent(keyboard,new KeyboardState(Key.Escape));yield return null;yield return null;
                Check(ui.IsOpen&&route.Blocked&&panel.group.alpha==0,"InputSystem Escape opens pause");
                InputSystem.QueueStateEvent(keyboard,new KeyboardState());yield return null;
                InputSystem.QueueStateEvent(keyboard,new KeyboardState(Key.Escape));yield return null;yield return null;
                Check(!ui.IsOpen&&!route.Blocked&&panel.Phase==FloorButtonPhase.Ready,"InputSystem Escape resumes current state");
                InputSystem.QueueStateEvent(keyboard,new KeyboardState());yield return null;
                foreach(bool exit in new[]{false,true})foreach(var phase in new[]{FloorButtonPhase.Rising,FloorButtonPhase.Ready,FloorButtonPhase.Lowering})
                {
                    if(!nav.InContent){route.DebugTracking(true);nav.StartContent("safe-evacuation-route");yield return Wait(()=>nav.ActiveAdapter==route&&!nav.Loading,"phase entry");}
                    view.ResetFloorDisplay();
                    view.SetActions(route,route.readyAnchor,new ContentUIAction("phase","준비 완료",()=>{calls++;view.SetActions(route);}));
                    if(phase!=FloorButtonPhase.Rising){yield return Wait(()=>panel.Phase==FloorButtonPhase.Ready,"phase action Ready");yield return null;}
                    if(phase==FloorButtonPhase.Lowering)ClickFloor(panel,0);
                    Check(panel.Phase==phase,"cleanup precondition "+phase+" exit="+exit);
                    CaptureRequested?.Invoke("cleanup-"+phase+"-"+(exit?"exit":"restart"));
                    var stale=panel.buttons[0].selected;int before=calls;
                    if(exit)route.ReturnToMenu();else route.Restart();
                    yield return null;stale.Invoke();
                    if(exit)yield return Wait(()=>!nav.InContent&&!nav.Loading,"exit phase cleanup");
                    else yield return Wait(()=>panel.Phase==FloorButtonPhase.Ready,"restart phase initial Ready");
                    Check(calls==before,"old event rejected after "+phase+" "+(exit?"exit":"restart"));
                    Check(exit?panel.Phase==FloorButtonPhase.Hidden&&view.Owner==null:route.Session.Stage==RouteStage.AwaitingReady&&route.Session.ScenarioIndex==0,"current display matches cleanup result "+phase);
                }
                foreach(bool exit in new[]{false,true})
                {
                    route.DebugTracking(true);nav.StartContent("safe-evacuation-route");yield return Wait(()=>nav.ActiveAdapter==route&&!nav.Loading,"next cancellation route entry");
                    yield return Wait(()=>panel.Phase==FloorButtonPhase.Ready,"next cancellation ready");
                    yield return PrepareNext(nav,route);yield return null;
                    ClickFloor(panel,0);
                    Check(panel.Phase==FloorButtonPhase.Lowering&&route.Session.Stage==RouteStage.Transition&&route.spaces[0].activeInHierarchy,"real next click accepted before cancellation");
                    var stale=panel.buttons[0].selected;
                    CaptureRequested?.Invoke(exit?"next-cancel-exit":"next-cancel-restart");
                    if(exit)route.ReturnToMenu();else route.Restart();
                    yield return new WaitForSecondsRealtime(route.readyAnchor.lowerSeconds+.15f);stale.Invoke();
                    Check(route.Session.ScenarioIndex==0&&!route.spaces[1].activeInHierarchy,"pending B move canceled without preliminary display reset exit="+exit);
                    if(exit)yield return Wait(()=>!nav.InContent&&!nav.Loading,"canceled next exit finished");
                    else
                    {
                        Check(route.Session.Stage==RouteStage.AwaitingReady,"canceled next restarts A");
                        route.ReturnToMenu();yield return Wait(()=>!nav.InContent&&!nav.Loading,"canceled next restart leave");
                    }
                }
            }
            finally
            {
                if(keyboard!=null)InputSystem.RemoveDevice(keyboard);
                previousKeyboard?.MakeCurrent();
                if(ui.IsOpen)ui.Continue();
            }
        }
        private static void ClickFloor(FloorButtonPresenter panel,int index)
        {
            var data=new UnityEngine.XR.Interaction.Toolkit.UI.TrackedDeviceEventData(EventSystem.current){pointerId=968};
            var button=panel.buttons[index];button.OnPointerDown(data);button.OnPointerClick(data);button.OnPointerUp(data);
        }
    }
}
#endif
