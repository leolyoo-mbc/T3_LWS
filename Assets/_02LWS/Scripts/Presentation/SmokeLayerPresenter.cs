using System;
using System.Collections.Generic;
using UnityEngine;

namespace LWS.FireNotification.Presentation
{
    public sealed class SmokeLayerPresenter : MonoBehaviour
    {
        public ParticleSystem rising, layer;
        public ParticleSystem[] fireSystems = Array.Empty<ParticleSystem>();
        public Light fireLight;
        [Min(0)] public float fireLightIntensity = 1.15f;
        [Min(0)] public float fireLightVariation = .2f;
        [Min(0)] public float fireLightFrequency = 1.9f;
        public enum Phase { InitialCollection, SteadyPractice, Frozen, Stopped }
        public Phase Current { get; private set; } = Phase.Stopped;
        private Phase beforePause;
        private float elapsed;

        public bool ConfigurationValid => rising != null && layer != null && fireLight != null &&
            fireSystems != null && fireSystems.Length > 0 && Array.TrueForAll(fireSystems, system => system != null);

        private IEnumerable<ParticleSystem> Systems()
        {
            var seen = new HashSet<ParticleSystem>();
            if (rising != null && seen.Add(rising)) yield return rising;
            if (layer != null && seen.Add(layer)) yield return layer;
            if (fireSystems == null) yield break;
            foreach (var system in fireSystems)
                if (system != null && seen.Add(system)) yield return system;
        }

        public void Collect()
        {
            Stop();
            Current = Phase.InitialCollection;
            int index = 0;
            foreach (var system in Systems())
            {
                system.useAutoRandomSeed = false;
                system.randomSeed = (uint)(7100 + index++);
                system.Play(false);
            }
            ApplyLight();
        }

        public void Steady()
        {
            if (Current == Phase.Frozen) { beforePause = Phase.SteadyPractice; return; }
            if (Current == Phase.Stopped) Collect();
            Current = Phase.SteadyPractice;
            foreach (var system in Systems()) if (!system.isPlaying) system.Play(false);
            ApplyLight();
        }

        public void SetPaused(bool value)
        {
            if (value && Current != Phase.Frozen && Current != Phase.Stopped)
            {
                beforePause = Current;
                Current = Phase.Frozen;
                foreach (var system in Systems()) system.Pause(false);
            }
            else if (!value && Current == Phase.Frozen)
            {
                Current = beforePause;
                foreach (var system in Systems()) system.Play(false);
            }
        }

        public void Stop()
        {
            Current = Phase.Stopped;
            elapsed = 0;
            foreach (var system in Systems()) system.Stop(false, ParticleSystemStopBehavior.StopEmittingAndClear);
            if (fireLight != null) { fireLight.intensity = 0; fireLight.enabled = false; }
        }

        private void ApplyLight()
        {
            if (fireLight == null || Current == Phase.Stopped) return;
            fireLight.enabled = true;
            float phase = elapsed * fireLightFrequency * Mathf.PI * 2;
            fireLight.intensity = Mathf.Max(0, fireLightIntensity + fireLightVariation *
                (.65f * Mathf.Sin(phase) + .35f * Mathf.Sin(phase * 1.57f)));
        }

        private void Update()
        {
            if (Current == Phase.Stopped || Current == Phase.Frozen) return;
            elapsed += Time.unscaledDeltaTime;
            ApplyLight();
        }

        private void OnDisable() => Stop();
    }
}
