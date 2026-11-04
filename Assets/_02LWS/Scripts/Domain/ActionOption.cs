using System;

namespace LWS.FireNotification.Domain
{
    [Serializable]
    public sealed class ActionOption
    {
        public string id;
        public string label;
        public bool isShout;
        public string feedbackCueId;
    }
}
