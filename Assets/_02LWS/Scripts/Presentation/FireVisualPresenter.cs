using UnityEngine;

namespace LWS.FireNotification.Presentation
{
    public sealed class FireVisualPresenter : MonoBehaviour
    {
        public ParticleSystem[] systems;
        public Light fireLight;
        private bool frozen;
        private float elapsed;
        public bool Frozen => frozen;
        public void Restore()
        {
            frozen = false; elapsed = 0;
            if (!Application.isPlaying) return;
            for (int i = 0; i < systems.Length; i++)
            {
                var system = systems[i];
                system.Stop(false, ParticleSystemStopBehavior.StopEmittingAndClear);
                system.useAutoRandomSeed = false;
                system.randomSeed = (uint)(7100 + i);
                system.Simulate(0.7f, false, true, true);
                system.Play(false);
            }
            if (fireLight != null) fireLight.intensity = 1.15f;
        }
        public void Freeze()
        {
            frozen = true;
            if (!Application.isPlaying) return;
            foreach (var system in systems) system.Pause(false);
        }
        private void Update()
        {
            if (frozen) return;
            elapsed += Time.unscaledDeltaTime;
            if (fireLight != null) fireLight.intensity = 1.15f + Mathf.Sin(elapsed * 12) * 0.13f + Mathf.Sin(elapsed * 19) * 0.07f;
        }
    }
}
