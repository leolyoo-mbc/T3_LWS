using UnityEngine;
using LWS.FireNotification.Runtime;

namespace LWS.FireNotification.Interaction
{
    public sealed class ContentMenuInput : MonoBehaviour
    {
        public ContentSceneNavigator navigator;
        public string contentId = "fire-notification";
        public void StartContent()=>navigator.StartContent(contentId);
    }
}
