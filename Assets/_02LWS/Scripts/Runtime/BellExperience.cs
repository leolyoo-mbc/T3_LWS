using LWS.FireNotification.Domain;
using LWS.FireNotification.Presentation;

namespace LWS.FireNotification.Runtime
{
    public sealed class BellExperience : ExperienceStageHandler
    {
        public AlarmResultPresenter result;
        public override bool Handles(ExperienceStage stage)=>stage==ExperienceStage.BellGuidance||stage==ExperienceStage.AwaitBellPress||stage==ExperienceStage.AlarmResult;
        public override void Enter(FireNotificationController controller,PresentationRun run)
        {
            if(run.Stage==ExperienceStage.BellGuidance)
                controller.guidance.Play(controller.catalog.Get("bell"),run,()=>controller.Complete(run));
            else if(run.Stage==ExperienceStage.AlarmResult)result.Play(controller,run);
        }
        public override void Cancel(){base.Cancel();if(result!=null)result.Cancel();}
    }
}
