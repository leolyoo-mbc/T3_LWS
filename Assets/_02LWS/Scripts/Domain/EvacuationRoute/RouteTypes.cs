using System;
using System.Collections.Generic;

namespace LWS.FireNotification.Domain.EvacuationRoute
{
    public enum RouteStage { Closed, AwaitingReady, Intro, Explore, Decision, Feedback, ScenarioCompletion, Transition, Result }
    public enum RouteTargetKind { Stair, Elevator }
    public enum RouteTargetState { Available, SmokeBlocked, Unavailable }
    public enum RouteChoice { Use, FindAnotherRoute }
    [Flags] public enum RouteBlocker { None=0, UserPause=1, Lifecycle=2, Tracking=4, Configuration=8 }
    [Serializable] public sealed class RouteTarget
    {
        public string Id, CheckpointId;
        public RouteTargetKind Kind;
        public RouteTargetState State;
    }
    [Serializable] public sealed class RouteWaypoint
    {
        public string Id, ScenarioId, TargetId;
        public string[] AdjacentIds = Array.Empty<string>();
    }
    [Serializable] public sealed class RouteScenario
    {
        public string Id, StartWaypointId;
        public bool IsGuided;
        public RouteTarget[] Targets = Array.Empty<RouteTarget>();
        public RouteWaypoint[] Waypoints = Array.Empty<RouteWaypoint>();
    }
    public sealed class RouteDecision
    {
        public string ScenarioId { get; internal set; }
        public string TargetId { get; internal set; }
        public RouteChoice Choice { get; internal set; }
        public int Sequence { get; internal set; }
        public bool FeedbackRequired { get; internal set; }
    }
    public sealed class RouteScenarioResult
    {
        public string ScenarioId { get; internal set; }
        public bool Completed { get; internal set; }
        public string ChosenStairId { get; internal set; }
        public bool DetourPerformed { get; internal set; }
        internal readonly List<RouteDecision> History = new List<RouteDecision>();
        public IReadOnlyList<RouteDecision> Decisions => History.AsReadOnly();
    }
}
