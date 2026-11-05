using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit.UI;

namespace LWS.FireNotification.Interaction
{
    public sealed class FreshPressButton : MonoBehaviour, IPointerDownHandler, IPointerClickHandler, IPointerUpHandler
    {
        public InputActionAsset inputs;
        public UnityEvent selected = new UnityEvent();
        public bool allowWhilePaused;
        private bool Blocked => !allowWhilePaused && Runtime.SharedUIController.Active != null && Runtime.SharedUIController.Active.IsOpen;
        private bool armed;
        private int acceptedPointer = int.MinValue;
        private InputAction left;
        private InputAction right;
        private bool released;
        private void OnEnable()
        {
            armed = false; released = false; acceptedPointer = int.MinValue;
            left = inputs != null ? inputs.FindAction("Left/Press") : null;
            right = inputs != null ? inputs.FindAction("Right/Press") : null;
        }
        private void Update()
        {
            if (acceptedPointer == int.MinValue && !(left?.IsPressed() ?? false) && !(right?.IsPressed() ?? false)) armed = true;
        }
        public void OnPointerDown(PointerEventData data)
        {
if (UnityEngine.XR.XRSettings.isDeviceActive && !(data is TrackedDeviceEventData)) return;
            if (!armed || !isActiveAndEnabled || Blocked || (GetComponent<Presentation.ThemedButton>() is Presentation.ThemedButton style && (!style.Available || style.Busy))) return;
            acceptedPointer = data.pointerId; armed = false;
        }
        public void OnPointerClick(PointerEventData data)
        {
            if (acceptedPointer != data.pointerId || !isActiveAndEnabled || Blocked || (GetComponent<Presentation.ThemedButton>() is Presentation.ThemedButton style && (!style.Available || style.Busy))) return;
            acceptedPointer = int.MinValue; selected.Invoke();
        }
        public void OnPointerUp(PointerEventData data) { if (acceptedPointer == data.pointerId) released = true; }
        private void LateUpdate() { if (released) { acceptedPointer = int.MinValue; released = false; } }
        private void OnDisable() { armed = false; acceptedPointer = int.MinValue; }
    }
}
