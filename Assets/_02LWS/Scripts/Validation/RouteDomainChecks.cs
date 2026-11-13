#if UNITY_EDITOR
using System;
using System.Linq;
using UnityEditor;
using UnityEngine;
using LWS.FireNotification.Domain.EvacuationRoute;

namespace LWS.FireNotification.Validation
{
    public static class RouteDomainChecks
    {
        public static RouteScenario Scenario(string id, bool guided)
        {
            string p=id+"-";
            return new RouteScenario { Id=id, IsGuided=guided, StartWaypointId=p+"start",
                Targets=new[]{new RouteTarget{Id=p+"left",CheckpointId=p+"wl",Kind=RouteTargetKind.Stair,State=guided?RouteTargetState.Available:RouteTargetState.SmokeBlocked},
                    new RouteTarget{Id=p+"right",CheckpointId=p+"wr",Kind=RouteTargetKind.Stair,State=RouteTargetState.Available},
                    new RouteTarget{Id=p+"lift",CheckpointId=p+"we",Kind=RouteTargetKind.Elevator,State=RouteTargetState.Unavailable}},
                Waypoints=new[]{new RouteWaypoint{Id=p+"start",ScenarioId=id,AdjacentIds=new[]{p+"wl",p+"wr",p+"we"}},
                    new RouteWaypoint{Id=p+"wl",ScenarioId=id,TargetId=p+"left",AdjacentIds=new[]{p+"start"}},
                    new RouteWaypoint{Id=p+"wr",ScenarioId=id,TargetId=p+"right",AdjacentIds=new[]{p+"start"}},
                    new RouteWaypoint{Id=p+"we",ScenarioId=id,TargetId=p+"lift",AdjacentIds=new[]{p+"start"}}} };
        }
        private sealed class ContractProxy
        {
            private readonly object value;
            public ContractProxy(object value) { this.value=value; }
            private T Get<T>(string name) => (T)value.GetType().GetProperty(name).GetValue(value);
            private bool Call(string name, params object[] args) => (bool)value.GetType().GetMethod(name).Invoke(value,args);
            public RouteStage Stage => Get<RouteStage>("Stage");
            public long Execution => Get<long>("Execution");
            public System.Collections.Generic.IReadOnlyList<RouteScenarioResult> Results => Get<System.Collections.Generic.IReadOnlyList<RouteScenarioResult>>("Results");
            public void Begin(long run, RouteScenario[] scenarios) => value.GetType().GetMethod("Begin").Invoke(value,new object[]{run,scenarios});
            public bool Ready(long r,long e)=>Call("Ready",r,e);
            public bool GuidanceDone(long r,long e)=>Call("GuidanceDone",r,e);
            public bool NextScenario(long r,long e)=>Call("NextScenario",r,e);
            public bool Arrive(long r,long e,string id)=>Call("Arrive",r,e,id);
            public bool LeaveCheckpoint(long r,long e,string id)=>Call("LeaveCheckpoint",r,e,id);
            public bool Choose(long r,long e,string id,RouteChoice c)=>Call("Choose",r,e,id,c);
        }
        [MenuItem("Tools/LWS/Validate Route Domain")]
        public static void Run()
        {
            // Reflection keeps the Editor compilable while the session contract is still unimplemented.
            var type=typeof(RouteScenario).Assembly.GetType("LWS.FireNotification.Domain.EvacuationRoute.RouteSession");
            if(type==null)throw new InvalidOperationException("FAIL: RouteSession behavior is not implemented");
            var s=new ContractProxy(Activator.CreateInstance(type));
            int count=0;
            Action<bool,string> check=(ok,name)=> {if(!ok)throw new InvalidOperationException("FAIL: "+name);count++;};
            s.Begin(1L,new[]{Scenario("a",true),Scenario("b",false)});
            check(s.Stage==RouteStage.AwaitingReady,"begin ready");
            check(s.Ready(1L,(long)s.Execution),"ready");
            check(!s.Ready(1L,(long)s.Execution),"duplicate ready");
            check(s.GuidanceDone(1L,(long)s.Execution),"intro ends");
            check(!s.Choose(1L,(long)s.Execution,"a-left",RouteChoice.Use),"remote choice rejected");
            check(s.Arrive(1L,(long)s.Execution,"a-we"),"lift arrival");
            long stale=s.Execution;
            check(s.Choose(1L,stale,"a-lift",RouteChoice.Use),"lift feedback");
            check(s.Stage==RouteStage.Feedback && !s.Results[0].Completed,"lift never completes");
            check(!s.Choose(1L,stale,"a-lift",RouteChoice.Use),"duplicate choice");
            check(s.GuidanceDone(1L,(long)s.Execution),"feedback ends");
            check(s.Choose(1L,(long)s.Execution,"a-lift",RouteChoice.FindAnotherRoute),"find another");
            check(!s.Arrive(1L,(long)s.Execution,"a-we"),"suppressed until exit");
            check(s.LeaveCheckpoint(1L,(long)s.Execution,"a-lift"),"exit suppression");
            check(s.Arrive(1L,(long)s.Execution,"a-wl"),"stair arrival");
            check(s.Choose(1L,(long)s.Execution,"a-left",RouteChoice.FindAnotherRoute),"safe stair explore");
            check(s.LeaveCheckpoint(1L,(long)s.Execution,"a-left"),"leave safe stair");
            check(s.Arrive(1L,(long)s.Execution,"a-wl"),"revisit safe stair");
            check(!s.Choose(1L,stale,"a-left",RouteChoice.Use),"stale input rejected");
            check(s.Choose(1L,(long)s.Execution,"a-left",RouteChoice.Use),"A completes");
            check(s.Results[0].Completed && s.Results[0].Decisions[0].FeedbackRequired,"first wrong retained");
            check(!s.NextScenario(1L,(long)s.Execution),"wait for completion cue");
            check(s.GuidanceDone(1L,(long)s.Execution),"completion cue ends");
            var real=(RouteSession)Activator.CreateInstance(type);
            for(int route=0;route<3;route++)
            {
                real.Begin(10+route,new[]{Scenario("a",true),Scenario("b",false)});
                real.Ready(real.RunId,real.Execution);real.GuidanceDone(real.RunId,real.Execution);
                real.Arrive(real.RunId,real.Execution,"a-wr");real.Choose(real.RunId,real.Execution,"a-right",RouteChoice.Use);
                real.GuidanceDone(real.RunId,real.Execution);real.NextScenario(real.RunId,real.Execution);
                long transition=real.Execution;
                real.SetBlockers(RouteBlocker.UserPause);
                check(!real.TransitionDone(real.RunId,transition),"blocked transition rejected");
                real.SetBlockers(RouteBlocker.None);
                check(real.TransitionDone(real.RunId,transition),"B transition accepted");
                check(!real.TransitionDone(real.RunId,transition),"duplicate transition rejected");
                real.GuidanceDone(real.RunId,real.Execution);
                if(route>0)
                {
                    real.Arrive(real.RunId,real.Execution,"b-wl");
                    if(route==2)
                    {
                        real.Choose(real.RunId,real.Execution,"b-left",RouteChoice.Use);
                        check(real.Stage==RouteStage.Feedback&&!real.Results[1].Completed,"blocked stair does not complete");
                        real.GuidanceDone(real.RunId,real.Execution);
                    }
                    real.Choose(real.RunId,real.Execution,"b-left",RouteChoice.FindAnotherRoute);
                    check(!real.Results[1].DetourPerformed,"no early detour credit");
                    real.LeaveCheckpoint(real.RunId,real.Execution,"b-left");
                    real.Arrive(real.RunId,real.Execution,"b-we");
                    real.Choose(real.RunId,real.Execution,"b-lift",RouteChoice.Use);real.GuidanceDone(real.RunId,real.Execution);
                    real.LeaveCheckpoint(real.RunId,real.Execution,"b-lift");
                }
                real.Arrive(real.RunId,real.Execution,"b-wr");
                long before=real.Execution;
                foreach(var blocker in new[]{RouteBlocker.UserPause,RouteBlocker.Lifecycle,RouteBlocker.Tracking,RouteBlocker.Configuration})
                {
                    real.SetBlockers(blocker);
                    check(!real.Choose(real.RunId,before,"b-right",RouteChoice.Use)&&real.Execution==before,"blocker prevents choice: "+blocker);
                }
                real.SetBlockers(RouteBlocker.None);
                check(real.Choose(real.RunId,real.Execution,"b-right",RouteChoice.Use),"B stair completes");
                check(real.Results[1].Completed&&real.Results[1].DetourPerformed==(route>0),"actual detour only");
                check(real.Results[0].Completed&&real.Results[0].Decisions.Count==1,"A result preserved");
                if(route==0)check(real.Results[1].Decisions.Count==1,"direct route requires no extra visits");
                if(route==2)check(real.Results[1].Decisions[0].FeedbackRequired,"first B wrong preserved");
                check(real.GuidanceDone(real.RunId,real.Execution)&&real.Stage==RouteStage.Result,"final guidance before result");
                long oldRun=real.RunId,oldExecution=real.Execution;
                real.Close();check(real.Results.Count==0&&real.Stage==RouteStage.Closed,"close clears records");
                real.Begin(100+route,new[]{Scenario("a",true),Scenario("b",false)});
                check(!real.TransitionDone(oldRun,oldExecution)&&!real.Ready(oldRun,oldExecution),"old run rejected after restart");
                check(real.Results.All(r=>!r.Completed&&r.Decisions.Count==0),"restart clears result");
            }
            Debug.Log("Route domain PASS "+count+" assertions");
        }
    }
}
#endif
