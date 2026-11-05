using UnityEngine;
using LWS.FireNotification.Domain;

namespace LWS.FireNotification.Presentation
{
    public sealed class ScenarioBaseline : MonoBehaviour
    {
        public ConversationBystander[] bystanders = System.Array.Empty<ConversationBystander>();
        public Transform fireAttentionTarget;
        public Transform alarmCap;
        public GameObject fireVisual;
        public FireVisualPresenter fireEffect;
        public Transform playerAnchor;
        private Vector3 capRest;
        private bool captured;
        public void Capture()
        {
            if (captured) return;
            foreach (var person in bystanders) person.Capture();
            if (alarmCap != null) capRest = alarmCap.localPosition;
            captured = true;
        }
        public void Restore(ExperienceStage stage)
        {
            Capture();
            bool watchingFire = stage == ExperienceStage.AwaitNext || stage == ExperienceStage.CompletionGuidance;
            foreach (var person in bystanders) person.Restore(watchingFire, fireAttentionTarget);
            if (alarmCap != null) alarmCap.localPosition = capRest;
            if (fireVisual != null) fireVisual.SetActive(true);
            if (fireEffect != null) fireEffect.Restore();
        }
        public void SetUserPaused(bool value)
        {
            foreach(var person in bystanders) person.SetUserPaused(value);
            if(fireEffect != null) fireEffect.SetUserPaused(value);
        }
        public void Freeze()
        {
            foreach (var person in bystanders) person.Freeze();
            if (fireEffect != null) fireEffect.Freeze();
        }
        public void NoticeFire()
        {
            foreach (var person in bystanders) person.NoticeFire(fireAttentionTarget);
        }
    }
}
