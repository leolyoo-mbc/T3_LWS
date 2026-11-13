using UnityEngine;
using LWS.FireNotification.Domain.EvacuationRoute;

namespace LWS.FireNotification.Presentation
{
    [CreateAssetMenu(menuName="LWS/Route Scenario")]
    public sealed class RouteScenarioAsset : ScriptableObject
    {
        public string id, startWaypointId;
        public bool isGuided;
        public RouteTarget[] targets;
        public LWS.FireNotification.Domain.EvacuationRoute.RouteWaypoint[] waypoints;
        public RouteScenario ToScenario() => new RouteScenario { Id=id,IsGuided=isGuided,StartWaypointId=startWaypointId,Targets=targets,Waypoints=waypoints };
    }
}
