using System.Collections.Generic;
using UnityEngine;
using LWS.FireNotification.Domain;

namespace LWS.FireNotification.Runtime
{
    public sealed class FireNotificationAdapter : ContentExperienceAdapter
    {
        public FireNotificationController controller;
        private bool bound;
        private readonly Dictionary<AudioSource, float> volumes = new Dictionary<AudioSource, float>();
        public override bool Blocked => controller == null || controller.InputBlocked;
        public override bool ResultVisible => controller != null && controller.Session != null && !controller.Session.Closed && !controller.Session.Suspended && controller.Session.Stage == ExperienceStage.CompletionGuidance;
        public override string ResultText => "화재 사실 알리기\n체험 완료\n큰소리로 화재 알리기 수행\n비상벨 누르기 수행";
        private void Bind()
        {
            if (bound || controller == null) return;
            controller.StateChanged += Publish;
            controller.MenuRequested += RequestMenu;
            controller.ExitConfirmationRequested += RequestExit;
            bound = true;
        }
        private void OnEnable() => Bind();
        private void OnDisable()
        {
            if (!bound || controller == null) return;
            controller.StateChanged -= Publish;
            controller.MenuRequested -= RequestMenu;
            controller.ExitConfirmationRequested -= RequestExit;
            bound = false;
        }
        public override void Begin() { Bind(); controller.BeginExperience(); }
        public override void Stop() { if (controller != null && controller.Session != null && !controller.Session.Closed) controller.ReturnToMenu(); }
        public override void Restart() => controller.RestartExperience();
        public override void SetUserPaused(bool value) { if (controller != null) controller.SetUserPaused(value); }
        public override void RequestReturn() => controller.RequestReturnToMenu();
        public override void ReturnToMenu() => controller.ReturnToMenu();
        public override void ApplySettings(float voice, float effects, bool subtitles)
        {
            if (controller == null) return;
            foreach (var source in GetComponentsInChildren<AudioSource>(true))
            {
                if (!volumes.ContainsKey(source)) volumes[source] = source.volume;
                source.volume = volumes[source] * (source == controller.guidance.voice ? voice : effects);
            }
            if (controller.guidance.subtitle != null) controller.guidance.subtitle.enabled = subtitles;
        }
    }
}
