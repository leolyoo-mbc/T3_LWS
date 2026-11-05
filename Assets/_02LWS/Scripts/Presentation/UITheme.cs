using UnityEngine;
using TMPro;

namespace LWS.FireNotification.Presentation
{
    [CreateAssetMenu(menuName = "LWS/UI Theme")]
    public sealed class UITheme : ScriptableObject
    {
        public TMP_FontAsset font;
        public Sprite panelSprite;
        public Sprite buttonSprite;
        public Sprite iconSprite;
        public Color surface = Hex("11161C"), raised = Hex("1B2630");
        public Color text = Hex("F5F7FA"), secondaryText = Hex("BEC9D2");
        public Color primary = Hex("075E60"), primaryHover = Hex("087174"), primaryPressed = Hex("06494B");
        public Color secondary = Hex("202C36"), secondaryHover = Hex("293C47"), secondaryPressed = Hex("15242D");
        public Color accent = Hex("68D5CA"), warning = Hex("FFD166"), danger = Hex("FF8C8C"), success = Hex("82D6AE");
        public Color disabledText = Hex("9EABB6"), disabledFill = Hex("222B33");
        public float titleSize = 48, panelTitleSize = 40, bodySize = 32, detailSize = 28;
        public float padding = 32, spacing = 16, buttonHeight = 80, buttonWidth = 240;
        public float transitionSeconds = .15f;
        public static Color Hex(string value) { ColorUtility.TryParseHtmlString("#" + value, out var color); return color; }
    }
}
