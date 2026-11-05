using UnityEngine;

namespace LWS.FireNotification.Presentation
{
    [RequireComponent(typeof(Animator))]
    public sealed class ConversationBystander : MonoBehaviour
    {
        public Animator animator;
        [Range(0, 1)] public float conversationPhase;
        [Min(0.1f)] public float blendSeconds = 0.3f;
        [Min(1)] public float turnDegreesPerSecond = 180;
        private static readonly int Conversation = Animator.StringToHash("Conversation");
        private static readonly int WatchingFire = Animator.StringToHash("WatchingFire");
        private Quaternion conversationRotation;
        private Quaternion fireRotation;
        private bool captured;
        private bool turning;
        private bool frozen;
        private bool userPaused;
        public void SetUserPaused(bool value) { userPaused = value; if(animator != null) animator.speed = value || frozen ? 0 : 1; }

        public void Capture()
        {
            if (captured) return;
            if (animator == null) animator = GetComponent<Animator>();
            conversationRotation = transform.rotation;
            captured = true;
        }
        public void Restore(bool watchingFire, Transform target)
        {
            Capture();
            frozen = false; userPaused = false; turning = false;
            transform.rotation = conversationRotation;
            if (watchingFire) transform.rotation = Facing(target);
            if (!Application.isPlaying) return;
            animator.applyRootMotion = false;
            animator.updateMode = AnimatorUpdateMode.UnscaledTime;
            animator.speed = 1;
            animator.Play(watchingFire ? WatchingFire : Conversation, 0, watchingFire ? 0 : conversationPhase);
            animator.Update(0);
        }
        public void NoticeFire(Transform target)
        {
            Capture();
            if (target == null) return;
            fireRotation = Facing(target);
            turning = true; frozen = false;
            animator.CrossFadeInFixedTime(WatchingFire, blendSeconds, 0, 0);
        }
        private Quaternion Facing(Transform target)
        {
            if (target == null) return conversationRotation;
            var direction = target.position - transform.position;
            direction.y = 0;
            return direction.sqrMagnitude > 0.0001f ? Quaternion.LookRotation(direction) : conversationRotation;
        }
        public void Freeze()
        {
            frozen = true;
            if (animator != null) animator.speed = 0;
        }
        private void Update()
        {
            if (!turning || frozen || userPaused) return;
            transform.rotation = Quaternion.RotateTowards(transform.rotation, fireRotation, turnDegreesPerSecond * Time.unscaledDeltaTime);
            if (Quaternion.Angle(transform.rotation, fireRotation) < 0.1f) turning = false;
        }
    }
}
