using System;

namespace LWS.FireNotification.Domain
{
    // No engine objects, persistent progress or callbacks: the controller owns presentation.
    public sealed class ExperienceSession
    {
        public Guid SessionId { get; } = Guid.NewGuid();
        public ExperienceStage Stage { get; private set; } = ExperienceStage.SituationGuidance;
        public long ExecutionId { get; private set; } = 1;
        public bool Suspended { get; private set; }
        public bool TransitionInFlight { get; private set; }
        public bool FeedbackActive { get; private set; }
        public bool Closed { get; private set; }

        public bool IsCurrent(long execution) => !Closed && !Suspended && ExecutionId == execution;

        public bool TryCommand(ExperienceCommand command, long execution, string optionId = null)
        {
            if (!IsCurrent(execution) || TransitionInFlight) return false;
            ExperienceStage next;
            switch (command)
            {
                case ExperienceCommand.ChooseAction when Stage == ExperienceStage.AwaitAction && !FeedbackActive:
                    if (optionId != "shout" && optionId != "wait") return false;
                    FeedbackActive = optionId == "wait";
                    next = FeedbackActive ? Stage : ExperienceStage.ShoutResult;
                    break;
                case ExperienceCommand.Continue when Stage == ExperienceStage.AwaitNext:
                    next = ExperienceStage.RestoreSituation;
                    break;
                case ExperienceCommand.PressBell when Stage == ExperienceStage.AwaitBellPress:
                    next = ExperienceStage.AlarmResult;
                    break;
                case ExperienceCommand.Complete:
                    switch (Stage)
                    {
                        case ExperienceStage.SituationGuidance: next = ExperienceStage.AwaitAction; break;
                        case ExperienceStage.AwaitAction when FeedbackActive:
                            FeedbackActive = false; next = Stage; break;
                        case ExperienceStage.ShoutResult: next = ExperienceStage.AwaitNext; break;
                        case ExperienceStage.RestoreSituation: next = ExperienceStage.BellGuidance; break;
                        case ExperienceStage.BellGuidance: next = ExperienceStage.AwaitBellPress; break;
                        case ExperienceStage.AlarmResult: next = ExperienceStage.CompletionGuidance; break;
                        default: return false; // Waits and completion narration never auto-advance.
                    }
                    break;
                default: return false;
            }
            TransitionInFlight = true;
            Stage = next;
            ExecutionId++;
            return true;
        }

        public void Entered() => TransitionInFlight = false;
        public bool Suspend()
        {
            if (Closed || Suspended) return false;
            Suspended = true;
            TransitionInFlight = false;
            return true;
        }
        public bool Resume()
        {
            if (Closed || !Suspended) return false;
            Suspended = false;
            FeedbackActive = false;
            ExecutionId++;
            TransitionInFlight = true;
            return true;
        }
        public void Close() { Closed = true; FeedbackActive = false; ExecutionId++; }
#if UNITY_EDITOR
        public void DebugEnter(ExperienceStage stage)
        {
            if (Closed || Suspended) return;
            Stage = stage; FeedbackActive = false; ExecutionId++; TransitionInFlight = true;
        }
#endif
    }
}
