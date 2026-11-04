using UnityEngine;
using LWS.FireNotification.Domain;
using LWS.FireNotification.Runtime;

namespace LWS.FireNotification.Interaction
{
    public sealed class NextInput : MonoBehaviour
    {
        public FireNotificationController controller;
        public GameObject panel;
        private void OnEnable(){controller.StateChanged+=Refresh;Refresh();}
        private void OnDisable(){if(controller!=null)controller.StateChanged-=Refresh;if(panel!=null)panel.SetActive(false);}
        private void Refresh(){var s=controller.Session;panel.SetActive(s!=null&&!s.Suspended&&!s.Closed&&s.Stage==ExperienceStage.AwaitNext);}
        public void Continue()=>controller.Send(ExperienceCommand.Continue);
    }
}
