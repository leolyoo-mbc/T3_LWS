using LWS.FireNotification.Domain;
using LWS.FireNotification.Presentation;

namespace LWS.FireNotification.Runtime
{
    public sealed class ShoutExperience : ExperienceStageHandler
    {
        public ActionOptions options;
        public ShoutResultPresenter result;
        public override bool Handles(ExperienceStage stage) => stage==ExperienceStage.SituationGuidance || stage==ExperienceStage.AwaitAction || stage==ExperienceStage.ShoutResult;
        public override void Enter(FireNotificationController controller,PresentationRun run)
        {
            options.Validate(controller.catalog);
            if(run.Stage==ExperienceStage.SituationGuidance)
                controller.guidance.Play(controller.catalog.Get("situation"),run,()=>controller.Complete(run));
            else if(run.Stage==ExperienceStage.AwaitAction && controller.Session.FeedbackActive)
                controller.guidance.Play(controller.catalog.Get(options.Get("wait").feedbackCueId),run,()=>controller.Complete(run));
            else if(run.Stage==ExperienceStage.ShoutResult) result.Play(controller,run);
        }
        public override void Cancel() { base.Cancel(); if(result!=null) result.Cancel(); }
    }
}
