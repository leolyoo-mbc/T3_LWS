using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
using Unity.XR.CoreUtils;
using LWS.FireNotification.Domain.SmokePosture;
namespace LWS.FireNotification.Interaction
{
    public sealed class HeadPoseSampleProvider : MonoBehaviour
    {
        public XROrigin origin;
        public long Revision { get; private set; }
        private readonly List<XRInputSubsystem> subsystems = new List<XRInputSubsystem>();
        private Vector3 position,scale;
        private Quaternion rotation;
        private bool validLast;
        private void OnEnable()
        {
            SubsystemManager.GetSubsystems(subsystems);
            foreach(var subsystem in subsystems) subsystem.trackingOriginUpdated+=OnOrigin;
            Cache(); validLast=false;
        }
        private void OnDisable() { foreach(var subsystem in subsystems) subsystem.trackingOriginUpdated-=OnOrigin; subsystems.Clear(); validLast=false; }
        private void OnOrigin(XRInputSubsystem _) { Revision++; validLast=false; }
        private void Cache() { if(origin==null)return; position=origin.transform.position;rotation=origin.transform.rotation;scale=origin.transform.lossyScale; }
        public HeadPoseSample Read(bool lifecycleAllowed, float delta)
        {
            if(origin==null || origin.Camera==null) return default;
            if(Vector3.Distance(position,origin.transform.position)>.001f || Quaternion.Angle(rotation,origin.transform.rotation)>.1f || Vector3.Distance(scale,origin.transform.lossyScale)>.001f)
            { Revision++;validLast=false;Cache(); }
            var device=InputDevices.GetDeviceAtXRNode(XRNode.Head);
            bool tracked=device.isValid && device.TryGetFeatureValue(CommonUsages.isTracked,out var isTracked) && isTracked
                && device.TryGetFeatureValue(CommonUsages.trackingState,out var state) && (state&InputTrackingState.Position)!=0;
            tracked &= Vector3.Distance(origin.transform.lossyScale,Vector3.one)<.001f;
            var sample=new HeadPoseSample {heightInOrigin=origin.CameraInOriginSpaceHeight,positionTracked=tracked,referenceRevision=Revision,lifecycleAllowed=lifecycleAllowed,deltaSeconds=validLast&&tracked&&lifecycleAllowed?delta:0};
            validLast=tracked&&lifecycleAllowed; return sample;
        }
        public bool ConfigurationValid => origin!=null && origin.Camera!=null && Vector3.Distance(origin.transform.lossyScale,Vector3.one)<.001f;
    }
}
