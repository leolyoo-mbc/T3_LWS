using LWS.FireNotification.Domain;
using LWS.FireNotification.Presentation;

namespace LWS.FireNotification.Runtime
{
    public sealed class MethodSequence : ExperienceStageHandler
    {
        public ScenarioRestorer restorer;
        public override bool Handles(ExperienceStage stage)=>stage==ExperienceStage.AwaitNext||stage==ExperienceStage.RestoreSituation;
        public override void Enter(FireNotificationController controller,PresentationRun run)
        {
            if(run.Stage==ExperienceStage.AwaitNext)controller.guidance.Play(controller.catalog.Get("next"),run,null);
            else restorer.Restore(controller,run);
        }
        public override void Cancel(){base.Cancel();if(restorer!=null)restorer.Cancel();}
    }
}
