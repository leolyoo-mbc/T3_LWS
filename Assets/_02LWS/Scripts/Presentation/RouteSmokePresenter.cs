using UnityEngine;
namespace LWS.FireNotification.Presentation
{
    public sealed class RouteSmokePresenter : MonoBehaviour
    {
        public ParticleSystem[] systems;
        public bool ConfigurationValid => systems!=null&&systems.Length>0&&System.Array.TrueForAll(systems,s=>s!=null);
        public void Play() {foreach(var s in systems)if(s!=null) {s.useAutoRandomSeed=false;s.randomSeed=931;s.Play(false);}}
        public void SetPaused(bool value) {foreach(var s in systems)if(s!=null) {if(value)s.Pause(false);else if(s.isPaused)s.Play(false);}}
        public void Stop() {if(systems!=null)foreach(var s in systems)if(s!=null)s.Stop(false,ParticleSystemStopBehavior.StopEmittingAndClear);}
        private void OnDisable()=>Stop();
    }
}
