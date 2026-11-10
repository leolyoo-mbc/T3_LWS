using UnityEngine;
namespace LWS.FireNotification.Presentation
{
    public sealed class SmokePostureValidationDisplay : MonoBehaviour
    {
        public bool show;
        private string boundary, feedback;
        public void MarkBoundary(long run) { boundary=$"INPUT run={run} frame={Time.frameCount} t={Time.realtimeSinceStartupAsDouble:F6}";feedback=""; }
        public void MarkFeedback(long run) { feedback=$"FEEDBACK run={run} frame={Time.frameCount} t={Time.realtimeSinceStartupAsDouble:F6}"; }
        public void Clear() { boundary=feedback=null; }
        private void OnGUI()
        {
            if(!show || !Debug.isDebugBuild)return;
            GUI.Box(new Rect(12,12,600,72),"Validation only\n"+boundary+"\n"+feedback);
        }
        private void OnDisable()=>Clear();
    }
}
