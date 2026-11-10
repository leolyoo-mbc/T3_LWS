using System;
namespace LWS.FireNotification.Domain.SmokePosture
{
    public enum SmokeStage { AwaitingReady, PrincipleGuidance, Practice, CompletionGuidance, Result, Closed }
    [Flags] public enum SmokeBlocker { None=0, UserPause=1, Lifecycle=2, Tracking=4, Configuration=8 }
    public sealed class SmokePostureSession
    {
        public long RunId { get; }
        public long Execution { get; private set; } = 1;
        public SmokeStage Stage { get; private set; } = SmokeStage.AwaitingReady;
        public double? BaselineHeight { get; private set; }
        public long ReferenceRevision { get; private set; }
        public double HoldSeconds { get; private set; }
        public bool LowRecognized { get; private set; }
        public bool PostureCompleted { get; private set; }
        public SmokeBlocker Blockers { get; private set; }
        public bool Closed => Stage == SmokeStage.Closed;
        public bool Blocked => Blockers != SmokeBlocker.None || Closed;
        public SmokePostureSession(long id) { RunId=id; }
        private void Enter(SmokeStage stage) { Stage=stage; Execution++; }
        private void ResetHold() { HoldSeconds=0; LowRecognized=false; }
        public void SetBlockers(SmokeBlocker value)
        {
            Blockers=value;
            if (Blocked && !PostureCompleted) ResetHold();
        }
        public bool Ready(HeadPoseSample sample, PostureTuning tuning)
        {
            if (Stage!=SmokeStage.AwaitingReady || Blocked || !sample.Valid || !tuning.Valid(sample.heightInOrigin)) return false;
            BaselineHeight=sample.heightInOrigin; ReferenceRevision=sample.referenceRevision;
            ResetHold(); Enter(SmokeStage.PrincipleGuidance); return true;
        }
        public bool GuidanceDone(long run, long execution)
        {
            if (Blocked || RunId!=run || Execution!=execution) return false;
            if (Stage==SmokeStage.PrincipleGuidance) { ResetHold(); Enter(SmokeStage.Practice); return true; }
            if (Stage==SmokeStage.CompletionGuidance) { Enter(SmokeStage.Result); return true; }
            return false;
        }
        public void ReferenceChanged()
        {
            if (Closed || PostureCompleted) return;
            BaselineHeight=null; ResetHold(); Enter(SmokeStage.AwaitingReady);
        }
        public void Step(HeadPoseSample sample, PostureTuning tuning)
        {
            if (Closed || PostureCompleted) return;
            if (BaselineHeight.HasValue && sample.referenceRevision!=ReferenceRevision) { ReferenceChanged(); return; }
            if (Blocked || !sample.Valid) { ResetHold(); return; }
            if (Stage!=SmokeStage.Practice || !BaselineHeight.HasValue) return;
            if (tuning==null || !tuning.Valid(BaselineHeight.Value)) { ResetHold(); return; }
            var boundary=BaselineHeight.Value*(1-tuning.dropRatio);
            if (sample.heightInOrigin > boundary+(LowRecognized?tuning.releaseMarginMeters:0)) { ResetHold(); return; }
            // The first low sample establishes the interval; it cannot count pre-entry time.
            if (!LowRecognized) { LowRecognized=true; HoldSeconds=0; return; }
            HoldSeconds=Math.Min(PostureTuning.RequiredHoldSeconds,HoldSeconds+sample.deltaSeconds);
            if (HoldSeconds>=PostureTuning.RequiredHoldSeconds)
            { PostureCompleted=true; Enter(SmokeStage.CompletionGuidance); }
        }
        public void Close() { if (Closed) return; BaselineHeight=null; ResetHold(); Enter(SmokeStage.Closed); }
    }
}
