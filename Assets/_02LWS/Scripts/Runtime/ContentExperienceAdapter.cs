using System;
using UnityEngine;

namespace LWS.FireNotification.Runtime
{
    public abstract class ContentExperienceAdapter : MonoBehaviour
    {
        public SharedContentUI view;
        public event Action StateChanged;
        public event Action MenuRequested;
        public event Action ExitConfirmationRequested;
        public abstract bool Blocked { get; }
        public virtual bool LocomotionAllowed => !Blocked;
        public abstract bool ResultVisible { get; }
        public abstract string ResultText { get; }
        public abstract void Begin();
        public abstract void Stop();
        public abstract void Restart();
        public abstract void SetUserPaused(bool value);
        public abstract void RequestReturn();
        public abstract void ReturnToMenu();
        public abstract void ApplySettings(float voice, float effects, bool subtitles);
        protected void Publish() => StateChanged?.Invoke();
        protected void RequestMenu() => MenuRequested?.Invoke();
        protected void RequestExit() => ExitConfirmationRequested?.Invoke();
    }
}
