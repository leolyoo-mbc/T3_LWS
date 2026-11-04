using LWS.FireNotification.Domain;
using LWS.FireNotification.Presentation;

namespace LWS.FireNotification.Runtime
{
    public sealed class CompletionExperience : ExperienceStageHandler
    {
        public override bool Handles(ExperienceStage stage)=>stage==ExperienceStage.CompletionGuidance;
        public override void Enter(FireNotificationController controller,PresentationRun run)
            =>controller.guidance.Play(controller.catalog.Get("completion"),run,null);
    }
}
