namespace LWS.FireNotification.Domain
{
    public enum ExperienceStage
    {
        SituationGuidance, AwaitAction, ShoutResult, AwaitNext, RestoreSituation,
        BellGuidance, AwaitBellPress, AlarmResult, CompletionGuidance
    }
    public enum ExperienceCommand { Complete, ChooseAction, Continue, PressBell }
}
