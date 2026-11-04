using UnityEngine;
using LWS.FireNotification.Domain;
using LWS.FireNotification.Presentation;

namespace LWS.FireNotification.Runtime
{
    public abstract class ExperienceStageHandler : MonoBehaviour
    {
        public abstract bool Handles(ExperienceStage stage);
        public abstract void Enter(FireNotificationController controller, PresentationRun run);
        public virtual void Cancel() { StopAllCoroutines(); }
    }
}
