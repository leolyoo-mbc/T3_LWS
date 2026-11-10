using UnityEngine;
namespace LWS.FireNotification.Presentation
{
    public sealed class SmokeLayerPresenter : MonoBehaviour
    {
        public ParticleSystem rising, layer;
        public enum Phase { InitialCollection, SteadyPractice, Frozen, Stopped }
        public Phase Current { get; private set; } = Phase.Stopped;
        private Phase beforePause;
        public void Collect()
        {
            Stop();Current=Phase.InitialCollection;
            rising.Play(true);layer.Play(true);
        }
        public void Steady()
        {
            Current=Phase.SteadyPractice;
            rising.Stop(true,ParticleSystemStopBehavior.StopEmittingAndClear);
            if(!layer.isPlaying)layer.Play(true);
        }
        public void SetPaused(bool value)
        {
            if(value && Current!=Phase.Frozen && Current!=Phase.Stopped)
            { beforePause=Current;Current=Phase.Frozen;rising.Pause(true);layer.Pause(true); }
            else if(!value && Current==Phase.Frozen)
            { Current=beforePause; if(Current==Phase.InitialCollection)rising.Play(true);layer.Play(true); }
        }
        public void Stop()
        {
            if(rising!=null)rising.Stop(true,ParticleSystemStopBehavior.StopEmittingAndClear);
            if(layer!=null)layer.Stop(true,ParticleSystemStopBehavior.StopEmittingAndClear);
            Current=Phase.Stopped;
        }
        private void OnDisable()=>Stop();
    }
}
