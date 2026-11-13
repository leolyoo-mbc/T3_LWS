using System;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Locomotion.Teleportation;

namespace LWS.FireNotification.Interaction
{
    public sealed class RouteWaypoint : MonoBehaviour
    {
        public string id, scenarioId, targetId;
        public string[] adjacentIds = Array.Empty<string>();
        public TeleportationAnchor anchor;
        public GameObject marker;
        public void SetReachable(bool value)
        {
            if(anchor!=null)anchor.enabled=value;
            if(marker!=null)marker.SetActive(value);
        }
    }
}
