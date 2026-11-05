using System.Collections;
using UnityEngine;
using LWS.FireNotification.Runtime;

namespace LWS.FireNotification.Presentation
{
    public sealed class AlarmResultPresenter : MonoBehaviour
    {
        public AudioSource alarm;
        public AudioClip clip;
        [Min(1)] public float resultDuration=3.5f;
        public void Play(FireNotificationController controller,PresentationRun run)
        {
            Cancel();
            if(clip==null)throw new System.InvalidOperationException("Missing local alarm audio");
            alarm.clip=clip;alarm.loop=true;alarm.Play();
            controller.baseline.NoticeFire();
            StartCoroutine(Finish(controller,run));
        }
        private IEnumerator Finish(FireNotificationController controller,PresentationRun run)
        {
            float time=0;
            while(time<resultDuration&&!run.Canceled){yield return null;time+=controller.PresentationDelta;}
            if(!run.Canceled)controller.Complete(run);
        }
        public void SetPaused(bool value) { if(alarm == null || alarm.clip == null) return; if(value) alarm.Pause(); else alarm.UnPause(); }
        public void Cancel(){StopAllCoroutines();if(alarm!=null){alarm.Stop();alarm.clip=null;}}
        private void OnDisable()=>Cancel();
    }
}
