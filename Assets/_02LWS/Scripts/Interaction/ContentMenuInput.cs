using UnityEngine;
using LWS.FireNotification.Runtime;

namespace LWS.FireNotification.Interaction
{
    public sealed class ContentMenuInput : MonoBehaviour
    {
        public ContentSceneNavigator navigator;
        public void StartContent()=>navigator.StartContent();
    }
}
