using System;
using UnityEngine;

namespace LWS.FireNotification.Presentation
{
    [Serializable]
    public sealed class SubtitleSegment
    {
        public float start;
        public float end;
        [TextArea(2, 8)] public string text;
    }
    [Serializable]
    public sealed class GuidanceCue
    {
        public string id;
        public AudioClip audio;
        [TextArea(3, 10)] public string text;
        public SubtitleSegment[] segments;

        public void Validate()
        {
            if (string.IsNullOrWhiteSpace(id) || audio == null || string.IsNullOrWhiteSpace(text))
                throw new InvalidOperationException("Missing local voice/text: " + id);
            if (segments == null || segments.Length == 0) throw new InvalidOperationException("Missing subtitles: " + id);
            float previousEnd = 0;
            foreach (var segment in segments)
            {
                if (segment.start < previousEnd || segment.end <= segment.start || segment.end > audio.length + 0.03f || string.IsNullOrWhiteSpace(segment.text))
                    throw new InvalidOperationException("Invalid subtitle timing: " + id);
                previousEnd = segment.end;
            }
            if (string.Join(" ", Array.ConvertAll(segments, s => s.text)) != text)
                throw new InvalidOperationException("Subtitle text differs from approved text: " + id);
        }
    }
}
