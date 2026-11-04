using System;
using UnityEngine;
using TMPro;

namespace LWS.FireNotification.Presentation
{
    [CreateAssetMenu(menuName = "LWS/Guidance Catalog")]
    public sealed class GuidanceCatalog : ScriptableObject
    {
        public GuidanceCue[] cues;
        public TMP_FontAsset font;
        public GuidanceCue Get(string id)
        {
            var cue = Array.Find(cues, c => c.id == id);
            if (cue == null) throw new InvalidOperationException("Unknown cue: " + id);
            return cue;
        }
        public void Validate()
        {
            if (font == null) throw new InvalidOperationException("Missing local Korean font");
            var ids = new System.Collections.Generic.HashSet<string>();
            foreach (var cue in cues)
            {
                cue.Validate();
                if (!ids.Add(cue.id)) throw new InvalidOperationException("Duplicate cue: " + cue.id);
                foreach (var character in cue.text)
                    if (!char.IsWhiteSpace(character) && !font.HasCharacter(character))
                        throw new InvalidOperationException("Missing font glyph: " + character);
            }
            foreach (var id in new[] { "situation", "next", "bell", "completion", "feedback", "shout" }) Get(id);
        }
    }
}
