using System;
using LWS.FireNotification.Domain;

namespace LWS.FireNotification.Presentation
{
    public sealed class PresentationRun : IDisposable
    {
        public long ExecutionId { get; }
        public ExperienceStage Stage { get; }
        public bool Canceled { get; private set; }
        public bool Completed { get; private set; }
        private Action cleanup;
        public PresentationRun(long executionId, ExperienceStage stage, Action cleanup)
        { ExecutionId = executionId; Stage = stage; this.cleanup = cleanup; }
        public bool TryComplete()
        {
            if (Canceled || Completed) return false;
            Completed = true; return true;
        }
        public void Dispose()
        {
            if (Canceled) return;
            Canceled = true;
            var action = cleanup; cleanup = null; action?.Invoke();
        }
    }
}
