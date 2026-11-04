using System.Collections;
using UnityEngine;
using LWS.FireNotification.Runtime;

namespace LWS.FireNotification.Presentation
{
    public sealed class ShoutResultPresenter : MonoBehaviour
    {
        [Min(1)] public float reactionDuration=2.2f;
        public void Play(FireNotificationController controller, PresentationRun run)
        {
            StopAllCoroutines();
            var cue=controller.catalog.Get("shout");
            controller.guidance.Play(cue,run,null);
            StartCoroutine(Finish(controller,run,cue.audio.length));
        }
        private IEnumerator Finish(FireNotificationController controller, PresentationRun run,float shoutDuration)
        {
            float elapsed=0;
            while(elapsed<shoutDuration && !run.Canceled) { yield return null; elapsed+=Time.unscaledDeltaTime; }
            if(run.Canceled) yield break;
            controller.baseline.NoticeFire();
            float duration=Mathf.Max(reactionDuration,shoutDuration+1.1f);
            while(elapsed<duration && !run.Canceled) { yield return null; elapsed+=Time.unscaledDeltaTime; }
            if(!run.Canceled) controller.Complete(run);
        }
        public void Cancel() => StopAllCoroutines();
    }
}
