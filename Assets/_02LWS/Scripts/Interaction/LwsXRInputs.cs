using UnityEngine;
using UnityEngine.InputSystem;

namespace LWS.FireNotification.Interaction
{
    public sealed class LwsXRInputs : MonoBehaviour
    {
        public InputActionAsset inputs;
        private void OnEnable() { if (inputs != null) inputs.Enable(); }
        private void OnDisable() { if (inputs != null) inputs.Disable(); }
    }
}
