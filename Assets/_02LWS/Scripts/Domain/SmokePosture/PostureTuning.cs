using System;
namespace LWS.FireNotification.Domain.SmokePosture
{
    [Serializable]
    public sealed class PostureTuning
    {
        public double dropRatio = .20;
        public double releaseMarginMeters = .03;
        public const double RequiredHoldSeconds = 2;
        public static bool Finite(double value) => !double.IsNaN(value) && !double.IsInfinity(value);
        public bool Valid(double baseline) => Finite(baseline) && baseline > 0 && Finite(dropRatio) && dropRatio > 0 && dropRatio < 1 && Finite(releaseMarginMeters) && releaseMarginMeters >= 0 && releaseMarginMeters < baseline * dropRatio;
    }
}
