using System;
using UnityEngine;
using TMPro;
namespace LWS.FireNotification.Presentation
{
    [CreateAssetMenu(menuName="LWS/Smoke Guidance")]
    public sealed class SmokeGuidanceCatalog : ScriptableObject
    {
        public GuidanceCue[] cues;
        public TMP_FontAsset font;
        public GuidanceCue Get(string id) => Array.Find(cues,c=>c.id==id) ?? throw new InvalidOperationException("Missing smoke cue: "+id);
        public void Validate()
        {
            if(font==null || cues==null)throw new InvalidOperationException("Missing Korean guidance assets");
            var keys=new System.Collections.Generic.HashSet<string>();
            foreach(var cue in cues)
            {
                cue.Validate();
                if(!keys.Add(cue.id))throw new InvalidOperationException("Duplicate smoke cue");
                foreach(var character in cue.text)if(!char.IsWhiteSpace(character)&&!font.HasCharacter(character,true,true))throw new InvalidOperationException("Missing Korean glyph: "+character);
            }
            foreach(var id in new[]{"ready","principle","practice","recognized","stand","completion"})Get(id);
        }
    }
}
