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
        public bool manageSubtitleBackground = true;
        private Coroutine playback;
        private bool paused;
        private float elapsed;
        public float Elapsed => playback != null && voice != null && voice.clip != null
            ? (float)voice.timeSamples / voice.clip.frequency : elapsed;
        public bool IsPresenting => playback != null;
        public void SetPaused(bool value)
        {
            paused = value;
            if (voice == null || playback == null) return;
            if (value) voice.Pause(); else voice.UnPause();
        }
        public void Play(GuidanceCue cue, PresentationRun run, Action completed)
        {
            Cancel(); cue.Validate();
            if (voice == null || subtitle == null) throw new InvalidOperationException("Missing voice/subtitle presenter references");
            playback = StartCoroutine(Playback(cue, run, completed));
        }
        private IEnumerator Playback(GuidanceCue cue, PresentationRun run, Action completed)
        {
            voice.clip = cue.audio; voice.loop = false; voice.Play();
            elapsed = 0; paused = false;
            int startFrame = Time.frameCount;
            while (!run.Canceled)
            {
                if (paused) { yield return null; continue; }
                // Natural end resets timeSamples to zero; detect it before sampling the audio clock.
                if (!voice.isPlaying && Time.frameCount > startFrame)
                { elapsed = cue.audio.length; break; }
                elapsed = (float)voice.timeSamples / cue.audio.frequency;
                string currentText = "";
                foreach (var segment in cue.segments)
                    if (elapsed >= segment.start && elapsed < segment.end) { currentText = segment.text; break; }
                subtitle.text = currentText;
                yield return null;
            }
            subtitle.text = ""; playback = null;
            if (run.Canceled) voice.Stop();
            if (!run.Canceled) completed?.Invoke();
        }
        public void Cancel()
        {
            if (playback != null) StopCoroutine(playback);
            playback = null;
            if (voice != null) { voice.Stop(); voice.clip = null; }
            if (subtitle != null) subtitle.text = "";
            elapsed = 0; paused = false;
        }
        private void LateUpdate()
        {
            if (!manageSubtitleBackground) return;
            if(subtitle == null || subtitle.canvas == null) return;
            var background = subtitle.canvas.GetComponent<UnityEngine.UI.Image>();
            if(background != null) background.enabled = subtitle.enabled && !string.IsNullOrWhiteSpace(subtitle.text);
        }
        private void OnDisable() => Cancel();
    }
}
