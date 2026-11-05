using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR;

namespace LWS.FireNotification.Presentation
{
    // Project tuning values, not Meta-mandated thresholds.
    public sealed class ComfortFollowUI : MonoBehaviour
    {
        public Transform head, origin;
        public Vector3 offset;
        public float angleThreshold = 30f, positionThreshold = .25f, followSpeed = 6f;
        private Vector3 anchor, previousOrigin;
        private Quaternion heading;
        private bool initialized, settling;

        private void OnEnable() { initialized = false; }
        public void Recenter()
        {
            if (head == null || origin == null) return;
            var forward = Vector3.ProjectOnPlane(head.forward, Vector3.up);
            if (forward.sqrMagnitude < .001f) forward = Vector3.forward;
            heading = Quaternion.LookRotation(forward.normalized, Vector3.up);
            anchor = head.position;
            previousOrigin = origin.position;
            initialized = true; settling = false;
            transform.SetPositionAndRotation(anchor + heading * offset, heading);
        }
        private static bool Pressed(XRNode node)
        {
            return InputDevices.GetDeviceAtXRNode(node).TryGetFeatureValue(UnityEngine.XR.CommonUsages.triggerButton, out var pressed) && pressed;
        }
        private void LateUpdate()
        {
            if (head == null || origin == null) return;
            // XR Origin discontinuities include teleport and space navigation. Never fly across the scene.
            if (!initialized || Vector3.Distance(previousOrigin, origin.position) > .5f) { Recenter(); return; }
            previousOrigin = origin.position;
            if ((Mouse.current != null && Mouse.current.leftButton.isPressed) || Pressed(XRNode.LeftHand) || Pressed(XRNode.RightHand)) return;
            var forward = Vector3.ProjectOnPlane(head.forward, Vector3.up);
            if (forward.sqrMagnitude < .001f) return;
            if (Vector3.Distance(anchor, head.position) > positionThreshold || Vector3.Angle(heading * Vector3.forward, forward) > angleThreshold)
                settling = true;
            if (!settling) return;
            float blend = 1f - Mathf.Exp(-followSpeed * Time.unscaledDeltaTime);
            anchor = Vector3.Lerp(anchor, head.position, blend);
            heading = Quaternion.Slerp(heading, Quaternion.LookRotation(forward.normalized, Vector3.up), blend);
            transform.SetPositionAndRotation(anchor + heading * offset, heading);
            if (Vector3.Distance(anchor, head.position) < .005f && Vector3.Angle(heading * Vector3.forward, forward) < .5f) settling = false;
        }
    }
}
