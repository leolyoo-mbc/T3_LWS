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
            controller.StateChanged += RefreshView;
            controller.MenuRequested += RequestMenu;
            controller.ExitConfirmationRequested += RequestExit;
            bound = true;
        }
        private void OnEnable() => Bind();
        private void OnDisable()
        {
            view?.Clear(this);
            if (!bound || controller == null) return;
            controller.StateChanged -= RefreshView;
            controller.MenuRequested -= RequestMenu;
            controller.ExitConfirmationRequested -= RequestExit;
            bound = false;
        }
        public override void Begin()
        {
            Bind();
            view.Bind(this, "화재 사실 알리기");
            controller.view = view; controller.presentationOwner = this;
            controller.BeginExperience(); RefreshView();
        }
        private void RefreshView()
        {
            var session = controller.Session;
            if (session == null || session.Closed) { view?.Clear(this); Publish(); return; }
            if (view != null && view.Owner == this)
            {
                view.SetStatus(this, session.Stage == ExperienceStage.AwaitAction ? "주변에 화재 사실을 알릴 행동을 선택하세요." :
                    session.Stage == ExperienceStage.AwaitNext ? "안내를 확인했다면 다음을 선택하세요." : "");
                if (!session.Suspended && session.Stage == ExperienceStage.AwaitAction && !session.FeedbackActive)
                    view.SetActions(this, new ContentUIAction("shout", "큰소리로 알리기", ChooseShout), new ContentUIAction("wait", "다른 사람이 알릴 때까지 기다리기", ChooseWait));
                else if (!session.Suspended && session.Stage == ExperienceStage.AwaitNext)
                    view.SetActions(this, new ContentUIAction("next", "다음", Continue));
                else view.SetActions(this);
            }
            Publish();
        }
        private void ChooseShout() => controller.Send(ExperienceCommand.ChooseAction, "shout");
        private void ChooseWait() => controller.Send(ExperienceCommand.ChooseAction, "wait");
        private void Continue() => controller.Send(ExperienceCommand.Continue);
        public override void Stop()
        {
            if (controller != null && controller.Session != null && !controller.Session.Closed) controller.ReturnToMenu();
            view?.Clear(this);
        }
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
                source.volume = volumes[source] * effects;
            }
        }
    }
}
