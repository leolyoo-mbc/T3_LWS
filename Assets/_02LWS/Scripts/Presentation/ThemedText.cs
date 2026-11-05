using UnityEngine;
using TMPro;

namespace LWS.FireNotification.Presentation
{
    [RequireComponent(typeof(TMP_Text))]
    public sealed class ThemedText : MonoBehaviour
    {
        public UITheme theme;
        public float baseSize = 32;
        public bool secondary;
        private float currentScale = 1;
        public void Apply(float scale)
        {
            currentScale = scale;
            var text = GetComponent<TMP_Text>();
            text.font = theme.font;
            text.fontSize = baseSize * Mathf.Clamp(scale, 1, 1.5f);
            text.enableAutoSizing = false;
            text.fontStyle = FontStyles.Normal;
            text.color = secondary ? theme.secondaryText : theme.text;
            text.overflowMode = TextOverflowModes.Overflow;
        }
        private void OnEnable() { if (theme != null) Apply(currentScale); }
    }
}
