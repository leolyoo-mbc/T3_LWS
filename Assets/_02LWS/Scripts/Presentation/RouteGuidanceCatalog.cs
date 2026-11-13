using System;
using System.Collections.Generic;
using UnityEngine;

namespace LWS.FireNotification.Presentation
{
    [CreateAssetMenu(menuName="LWS/Route Guidance")]
    public sealed class RouteGuidanceCatalog : ScriptableObject
    {
        public GuidanceCue[] cues = Array.Empty<GuidanceCue>();
        public GuidanceCue Get(string id) => Array.Find(cues,c=>c!=null&&c.id==id) ?? throw new InvalidOperationException("Missing route cue: "+id);
        public void Validate()
        {
            var ids=new HashSet<string>(StringComparer.Ordinal);
            foreach(var cue in cues) { if(cue==null)throw new InvalidOperationException("Null route cue");cue.Validate();if(!ids.Add(cue.id))throw new InvalidOperationException("Duplicate route cue"); }
            foreach(var id in new[]{"ready","intro-a","intro-b","elevator","smoke","complete-a","completion"})Get(id);
        }
    }
}
