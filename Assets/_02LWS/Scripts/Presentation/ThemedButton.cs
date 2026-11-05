using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

namespace LWS.FireNotification.Presentation
{
    [RequireComponent(typeof(Image))]
    public sealed class ThemedButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler, ISelectHandler, IDeselectHandler
    {
        public UITheme theme;
        public bool primary;
        public bool Available { get; private set; } = true;
        public bool Busy { get; private set; }
        private bool hover, pressed;
        private Image image;
        private Outline outline;
        private void Awake() { image = GetComponent<Image>(); outline = GetComponent<Outline>(); }
        public void SetAvailable(bool available, bool busy = false) { Available = available; Busy = busy; Paint(); }
        public void OnPointerEnter(PointerEventData e) { hover = true; Paint(); }
        public void OnPointerExit(PointerEventData e) { hover = pressed = false; Paint(); }
        public void OnPointerDown(PointerEventData e) { pressed = true; Paint(); }
        public void OnPointerUp(PointerEventData e) { pressed = false; Paint(); }
        public void OnSelect(BaseEventData e) { hover = true; Paint(); }
        public void OnDeselect(BaseEventData e) { hover = pressed = false; Paint(); }
        private void OnEnable() { hover = pressed = false; Paint(); }
        private void Paint()
        {
            if (theme == null) return;
            if (image == null) image = GetComponent<Image>();
            if (outline == null) outline = GetComponent<Outline>();
            image.color = !Available ? theme.disabledFill : Busy ? theme.raised : primary
                ? (pressed ? theme.primaryPressed : hover ? theme.primaryHover : theme.primary)
                : (pressed ? theme.secondaryPressed : hover ? theme.secondaryHover : theme.secondary);
            if (outline != null) { outline.effectColor = Busy ? theme.warning : theme.accent; outline.enabled = Available && (hover || Busy); }
            foreach (var text in GetComponentsInChildren<TMP_Text>()) text.color = !Available ? theme.disabledText : Busy ? theme.warning : theme.text;
        }
    }
}
