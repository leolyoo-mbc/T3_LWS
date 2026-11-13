using System;
using System.Collections.Generic;
using System.Linq;

namespace LWS.FireNotification.Domain.EvacuationRoute
{
    public sealed class RouteSession
    {
        public long RunId { get; private set; }
        public long Execution { get; private set; }
        public RouteStage Stage { get; private set; } = RouteStage.Closed;
        public int ScenarioIndex { get; private set; }
        public string CurrentTargetId { get; private set; }
        public string CurrentWaypointId { get; private set; }
        public RouteBlocker Blockers { get; private set; }
        public bool Blocked => Blockers!=RouteBlocker.None;
        public IReadOnlyList<RouteScenarioResult> Results => Array.AsReadOnly(results);
        public RouteScenario Scenario => scenarios[ScenarioIndex];
        public string DetourOriginId { get; private set; }
        private RouteScenario[] scenarios=Array.Empty<RouteScenario>();
        private RouteScenarioResult[] results=Array.Empty<RouteScenarioResult>();
        private string suppressedTarget;
        private bool completionGuidanceDone, detourLeft;
        private bool Accept(long run,long execution) => Stage!=RouteStage.Closed&&!Blocked&&RunId==run&&Execution==execution;
        private void Enter(RouteStage stage) { Stage=stage;Execution++; }
        public void Begin(long runId,RouteScenario[] values)
        {
            Validate(values);
            // Own the configuration snapshot so callers cannot change safety rules during a run.
            scenarios=values.Select(s=>new RouteScenario {Id=s.Id,IsGuided=s.IsGuided,StartWaypointId=s.StartWaypointId,
                Targets=s.Targets.Select(t=>new RouteTarget{Id=t.Id,Kind=t.Kind,State=t.State,CheckpointId=t.CheckpointId}).ToArray(),
                Waypoints=s.Waypoints.Select(w=>new RouteWaypoint{Id=w.Id,ScenarioId=w.ScenarioId,TargetId=w.TargetId,AdjacentIds=(string[])w.AdjacentIds.Clone()}).ToArray()}).ToArray();
            RunId=runId;Execution++;ScenarioIndex=0;Blockers=RouteBlocker.None;
            results=scenarios.Select(s=>new RouteScenarioResult{ScenarioId=s.Id}).ToArray();
            ResetScenario();Enter(RouteStage.AwaitingReady);
        }
        private void ResetScenario()
        {
            CurrentTargetId=null;suppressedTarget=null;DetourOriginId=null;detourLeft=false;completionGuidanceDone=false;
            CurrentWaypointId=Scenario.StartWaypointId;
        }
        public bool Ready(long run,long execution)
        { if(!Accept(run,execution)||Stage!=RouteStage.AwaitingReady)return false;Enter(RouteStage.Intro);return true; }
        public bool Arrive(long run,long execution,string waypointId)
        {
            if(!Accept(run,execution)||(Stage!=RouteStage.Explore&&Stage!=RouteStage.Decision))return false;
            var waypoint=Array.Find(Scenario.Waypoints,w=>w.Id==waypointId);
            if(waypoint==null||(!string.IsNullOrEmpty(waypoint.TargetId)&&waypoint.TargetId==suppressedTarget))return false;
            if(CurrentWaypointId==waypointId&&CurrentTargetId==waypoint.TargetId)return false;
            CurrentWaypointId=waypointId;CurrentTargetId=waypoint.TargetId;
            Enter(string.IsNullOrEmpty(CurrentTargetId)?RouteStage.Explore:RouteStage.Decision);return true;
        }
        public bool LeaveCheckpoint(long run,long execution,string targetId)
        {
            if(!Accept(run,execution)||string.IsNullOrEmpty(targetId)||(targetId!=CurrentTargetId&&targetId!=suppressedTarget))return false;
            if(Stage!=RouteStage.Explore&&Stage!=RouteStage.Decision)return false;
            if(targetId==DetourOriginId)detourLeft=true;
            suppressedTarget=null;CurrentTargetId=null;Enter(RouteStage.Explore);return true;
        }
        public bool Choose(long run,long execution,string targetId,RouteChoice choice)
        {
            if(!Accept(run,execution)||Stage!=RouteStage.Decision||CurrentTargetId!=targetId||!Enum.IsDefined(typeof(RouteChoice),choice))return false;
            var target=Array.Find(Scenario.Targets,t=>t.Id==targetId);
            bool wrong=choice==RouteChoice.Use&&(target.Kind!=RouteTargetKind.Stair||target.State!=RouteTargetState.Available);
            var result=results[ScenarioIndex];
            result.History.Add(new RouteDecision{ScenarioId=Scenario.Id,TargetId=targetId,Choice=choice,Sequence=result.History.Count+1,FeedbackRequired=wrong});
            if(choice==RouteChoice.FindAnotherRoute)
            {
                if(target.State==RouteTargetState.SmokeBlocked) {DetourOriginId=targetId;detourLeft=false;}
                suppressedTarget=targetId;CurrentTargetId=null;Enter(RouteStage.Explore);
            }
            else if(wrong)Enter(RouteStage.Feedback);
            else
            {
                result.Completed=true;result.ChosenStairId=targetId;
                result.DetourPerformed=DetourOriginId!=null&&detourLeft&&DetourOriginId!=targetId;
                Enter(RouteStage.ScenarioCompletion);
            }
            return true;
        }
        public bool GuidanceDone(long run,long execution)
        {
            if(!Accept(run,execution))return false;
            if(Stage==RouteStage.Intro)Enter(RouteStage.Explore);
            else if(Stage==RouteStage.Feedback)Enter(CurrentTargetId==null?RouteStage.Explore:RouteStage.Decision);
            else if(Stage==RouteStage.ScenarioCompletion&&!completionGuidanceDone)
            { completionGuidanceDone=true;Enter(ScenarioIndex==scenarios.Length-1?RouteStage.Result:RouteStage.ScenarioCompletion); }
            else return false;
            return true;
        }
        public bool CompletionGuidanceDone => completionGuidanceDone;
        public bool NextScenario(long run,long execution)
        {
            if(!Accept(run,execution)||Stage!=RouteStage.ScenarioCompletion||!completionGuidanceDone||ScenarioIndex>=scenarios.Length-1)return false;
            Enter(RouteStage.Transition);return true;
        }
        public bool TransitionDone(long run,long execution)
        {
            if(!Accept(run,execution)||Stage!=RouteStage.Transition)return false;
            ScenarioIndex++;ResetScenario();Enter(RouteStage.Intro);return true;
        }
        public void SetBlockers(RouteBlocker blockers) { Blockers=blockers; }
        public void Close()
        {
            results=Array.Empty<RouteScenarioResult>();CurrentTargetId=null;CurrentWaypointId=null;suppressedTarget=null;DetourOriginId=null;
            Blockers=RouteBlocker.None;Enter(RouteStage.Closed);
        }
        public static void Validate(RouteScenario[] values)
        {
            if(values==null||values.Length!=2)throw new ArgumentException("Exactly two route scenarios required");
            var all=new HashSet<string>(StringComparer.Ordinal);
            for(int index=0;index<values.Length;index++)
            {
                var s=values[index];
                if(s==null||string.IsNullOrWhiteSpace(s.Id)||!all.Add(s.Id)||s.IsGuided!=(index==0)||s.Targets==null||s.Waypoints==null)throw new ArgumentException("Invalid scenario");
                foreach(var t in s.Targets)
                    if(t==null||string.IsNullOrWhiteSpace(t.Id)||!t.Id.StartsWith(s.Id+"-",StringComparison.Ordinal)||!all.Add(t.Id)||
                        (t.Kind==RouteTargetKind.Stair?t.State!=RouteTargetState.Available&&t.State!=RouteTargetState.SmokeBlocked:t.Kind!=RouteTargetKind.Elevator||t.State!=RouteTargetState.Unavailable))throw new ArgumentException("Invalid route target");
                if(s.Targets.Count(t=>t.Kind==RouteTargetKind.Stair)!=2||s.Targets.Count(t=>t.Kind==RouteTargetKind.Elevator)!=1||s.Targets.Count(t=>t.State==RouteTargetState.Available)!=(index==0?2:1))throw new ArgumentException("Invalid stair availability");
                var map=new Dictionary<string,RouteWaypoint>(StringComparer.Ordinal);
                foreach(var w in s.Waypoints)
                {
                    if(w==null||string.IsNullOrWhiteSpace(w.Id)||!w.Id.StartsWith(s.Id+"-",StringComparison.Ordinal)||!all.Add(w.Id)||w.ScenarioId!=s.Id||w.AdjacentIds==null)throw new ArgumentException("Invalid waypoint");
                    map.Add(w.Id,w);
                }
                if(!map.ContainsKey(s.StartWaypointId)||!string.IsNullOrEmpty(map[s.StartWaypointId].TargetId))throw new ArgumentException("Missing route start");
                foreach(var w in s.Waypoints)
                {
                    if(w.AdjacentIds.Distinct().Count()!=w.AdjacentIds.Length||w.AdjacentIds.Any(id=>id==w.Id||!map.ContainsKey(id)||!map[id].AdjacentIds.Contains(w.Id)))throw new ArgumentException("Invalid bidirectional connection");
                    if(!string.IsNullOrEmpty(w.TargetId)&&!s.Targets.Any(t=>t.Id==w.TargetId&&t.CheckpointId==w.Id))throw new ArgumentException("Invalid waypoint target");
                }
                foreach(var t in s.Targets)if(!map.ContainsKey(t.CheckpointId)||map[t.CheckpointId].TargetId!=t.Id)throw new ArgumentException("Missing checkpoint");
                var visited=new HashSet<string>();var queue=new Queue<string>();queue.Enqueue(s.StartWaypointId);
                while(queue.Count>0) {var id=queue.Dequeue();if(!visited.Add(id))continue;foreach(var neighbor in map[id].AdjacentIds)queue.Enqueue(neighbor);}
                if(visited.Count!=map.Count)throw new ArgumentException("Unreachable waypoint");
            }
        }
    }
}
