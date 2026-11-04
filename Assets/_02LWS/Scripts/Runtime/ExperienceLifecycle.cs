using System;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.OpenXR;
using LWS.FireNotification.Domain;

namespace LWS.FireNotification.Runtime
{
    [Serializable]
    public sealed class SuspensionSnapshot
    {
        public ExperienceStage stage;
        public long stageExecution;
    }
    [DefaultExecutionOrder(-200)]
    public sealed class ExperienceLifecycle : MonoBehaviour
    {
        private bool paused;
        private bool unfocused;
        private bool xrBlocked;
        private bool xrObserved;
        private bool debugBlocked;
        public bool Blocked => paused || unfocused || xrBlocked || debugBlocked;
        public event Action<bool> Changed;
        private bool last;
        private void OnApplicationPause(bool value)
        {
#if UNITY_EDITOR
            if (ignoreEditorCallbacks) return;
#endif
            paused = value; Publish();
        }
        private void OnApplicationFocus(bool value)
        {
#if UNITY_EDITOR
            if (ignoreEditorCallbacks) return;
#endif
            unfocused = !value; Publish();
        }
        private void Update()
        {
            // Editor focus differs from an installed headset session; device acceptance remains separate.
#if !UNITY_EDITOR
            xrObserved |= XRSettings.isDeviceActive;
            xrBlocked = xrObserved && (!XRSettings.isDeviceActive || !OpenXRUtility.IsSessionFocused || !OpenXRUtility.IsUserPresent);
#endif
            Publish();
        }
        private void Publish() { if (last == Blocked) return; last = Blocked; Changed?.Invoke(last); }
#if UNITY_EDITOR
        private bool ignoreEditorCallbacks;
        public void DebugIsolateLifecycle(bool value)
        {
            ignoreEditorCallbacks=value;
            if(value) { paused=false; unfocused=false; debugBlocked=false; Publish(); }
        }
        public void DebugSuspend(bool value) { debugBlocked = value; Publish(); }
#endif
    }
}
