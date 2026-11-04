using System;
using System.Collections.Generic;
using UnityEngine;
using LWS.FireNotification.Domain;
using LWS.FireNotification.Presentation;

namespace LWS.FireNotification.Runtime
{
    public sealed class FireNotificationController : MonoBehaviour
    {
        public GuidanceCatalog catalog;
        public GuidancePresenter guidance;
        public ScenarioBaseline baseline;
        public ExperienceLifecycle lifecycle;
        public ExperienceStageHandler[] handlers = Array.Empty<ExperienceStageHandler>();
        public ExperienceSession Session { get; private set; }
        public SuspensionSnapshot Snapshot { get; private set; }
        public PresentationRun Run { get; private set; }
        public event Action StateChanged;
        public event Action MenuRequested;
        private struct Pending { public ExperienceCommand command; public long execution; public string option; }
        private readonly Queue<Pending> commands = new Queue<Pending>();
        private bool leaving;
        private bool started;

        private void Start()
        {
            catalog.Validate(); baseline.Capture();
            Session = new ExperienceSession(); started = true;
            lifecycle.Changed += OnSuspension;
            if (lifecycle.Blocked) OnSuspension(true); else Enter();
        }
        public void Send(ExperienceCommand command, string option = null)
        {
            if (!started || leaving || Session.Suspended) return;
            commands.Enqueue(new Pending { command = command, option = option, execution = Session.ExecutionId });
        }
        public void Complete(PresentationRun run)
        {
            if (run == Run && Session.IsCurrent(run.ExecutionId) && run.TryComplete())
                commands.Enqueue(new Pending { command = ExperienceCommand.Complete, execution = run.ExecutionId });
        }
        // Completion callbacks are queued; focus/pause and menu requests win before LateUpdate commits any transition.
        private void LateUpdate()
        {
            if (!started || leaving || Session.Suspended || lifecycle.Blocked) { commands.Clear(); return; }
            while (commands.Count > 0)
            {
                var command = commands.Dequeue();
                if (Session.TryCommand(command.command, command.execution, command.option)) { Enter(); break; }
            }
        }
        private void Enter()
        {
            Run?.Dispose();
            CancelPresentation();
            baseline.Restore(Session.Stage);
            Run = new PresentationRun(Session.ExecutionId, Session.Stage, CancelPresentation);
            Session.Entered();
            StateChanged?.Invoke();
            foreach (var handler in handlers)
                if (handler != null && handler.Handles(Session.Stage)) { handler.Enter(this, Run); return; }
            if (Session.Stage == ExperienceStage.SituationGuidance)
            {
                var run = Run;
                guidance.Play(catalog.Get("situation"), run, () => Complete(run));
            }
        }
        private void CancelPresentation()
        {
            if (guidance != null) guidance.Cancel();
            foreach (var handler in handlers) if (handler != null) handler.Cancel();
            if (baseline != null) baseline.Freeze();
        }
        private void OnSuspension(bool blocked)
        {
            if (!started || leaving || Session == null || Session.Closed) return;
            commands.Clear();
            if (blocked && Session.Suspend())
            {
                Snapshot = new SuspensionSnapshot { stage = Session.Stage, stageExecution = Session.ExecutionId };
                Run?.Dispose(); CancelPresentation(); StateChanged?.Invoke();
            }
            else if (!blocked && Session.Resume()) { Snapshot = null; Enter(); }
        }
        public void ReturnToMenu()
        {
            if (!started || leaving || Session.Suspended) return;
            leaving = true; Session.Close(); commands.Clear(); Snapshot = null;
            Run?.Dispose(); CancelPresentation(); StateChanged?.Invoke(); MenuRequested?.Invoke();
        }
        private void OnDisable()
        {
            if (lifecycle != null) lifecycle.Changed -= OnSuspension;
            Run?.Dispose(); CancelPresentation(); Session?.Close(); Snapshot = null; commands.Clear();
        }
#if UNITY_EDITOR
        public void DebugEnter(ExperienceStage stage)
        {
            if (!started || leaving || Session.Suspended) return;
            commands.Clear(); Session.DebugEnter(stage); Enter();
        }
#endif
    }
}
