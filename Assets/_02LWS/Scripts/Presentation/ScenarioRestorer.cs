using System.Collections;
using LWS.FireNotification.Domain;
using LWS.FireNotification.Runtime;
using UnityEngine;

namespace LWS.FireNotification.Presentation
{
    public sealed class ScenarioRestorer : MonoBehaviour
    {
        public void Restore(FireNotificationController controller,PresentationRun run)
        {StopAllCoroutines();StartCoroutine(Execute(controller,run));}
        private IEnumerator Execute(FireNotificationController controller,PresentationRun run)
        {
            controller.baseline.Restore(ExperienceStage.RestoreSituation);
            yield return null;
            if(!run.Canceled)controller.Complete(run);
        }
        public void Cancel()=>StopAllCoroutines();
    }
}
