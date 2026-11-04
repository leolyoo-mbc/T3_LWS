using System;
using System.Collections;
using UnityEngine;
using TMPro;

namespace LWS.FireNotification.Presentation
{
    public sealed class GuidancePresenter : MonoBehaviour
    {
        public AudioSource voice;
        public TMP_Text subtitle;
        private Coroutine playback;
        public void Play(GuidanceCue cue, PresentationRun run, Action completed)
        {
            Cancel(); cue.Validate();
            if (voice == null || subtitle == null) throw new InvalidOperationException("Missing voice/subtitle presenter references");
            playback = StartCoroutine(Playback(cue, run, completed));
        }
        private IEnumerator Playback(GuidanceCue cue, PresentationRun run, Action completed)
        {
            voice.clip = cue.audio; voice.loop = false; voice.Play();
            float elapsed = 0;
            while (elapsed < cue.audio.length && !run.Canceled)
            {
                subtitle.text = "";
                foreach (var segment in cue.segments)
                    if (elapsed >= segment.start && elapsed < segment.end) { subtitle.text = segment.text; break; }
                yield return null;
                elapsed += Time.unscaledDeltaTime;
            }
            subtitle.text = ""; playback = null;
            if (!run.Canceled) completed?.Invoke();
        }
        public void Cancel()
        {
            if (playback != null) StopCoroutine(playback);
            playback = null;
            if (voice != null) { voice.Stop(); voice.clip = null; }
            if (subtitle != null) subtitle.text = "";
        }
        private void OnDisable() => Cancel();
    }
}
