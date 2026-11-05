using UnityEngine;
using UnityEngine.UI;

namespace LWS.FireNotification.Presentation
{
    [RequireComponent(typeof(Image))]
    public sealed class ThemedPanel : MonoBehaviour
    {
        public UITheme theme;
        private void OnEnable()
        {
            if(theme == null) return;
            var image=GetComponent<Image>();
            image.sprite=theme.panelSprite;
            image.type=Image.Type.Sliced;
            image.color=theme.surface;
        }
    }
}
