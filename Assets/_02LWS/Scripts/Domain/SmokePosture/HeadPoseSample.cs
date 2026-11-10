namespace LWS.FireNotification.Domain.SmokePosture
{
    public struct HeadPoseSample
    {
        public double heightInOrigin;
        public bool positionTracked;
        public long referenceRevision;
        public double deltaSeconds;
        public bool lifecycleAllowed;
        public bool Valid => positionTracked && lifecycleAllowed && PostureTuning.Finite(heightInOrigin) && PostureTuning.Finite(deltaSeconds) && deltaSeconds >= 0;
    }
}
