using UnityEngine;
using LWS.FireNotification.Domain.SmokePosture;
namespace LWS.FireNotification.Presentation
{
    [CreateAssetMenu(menuName="LWS/Smoke Posture Tuning")]
    public sealed class SmokePostureTuningAsset : ScriptableObject
    {
        [Tooltip("App recognition proposal, not a fire safety standard")]
        public PostureTuning tuning=new PostureTuning();
    }
}
