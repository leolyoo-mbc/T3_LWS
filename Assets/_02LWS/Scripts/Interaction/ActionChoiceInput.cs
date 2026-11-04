using UnityEngine;
using LWS.FireNotification.Domain;
using LWS.FireNotification.Runtime;

namespace LWS.FireNotification.Interaction
{
    public sealed class ActionChoiceInput : MonoBehaviour
    {
        public FireNotificationController controller;
        public GameObject panel;
        private void OnEnable() { controller.StateChanged+=Refresh; Refresh(); }
        private void OnDisable() { if(controller!=null) controller.StateChanged-=Refresh; if(panel!=null) panel.SetActive(false); }
        private void Refresh()
        {
            var s=controller.Session;
            panel.SetActive(s!=null && !s.Suspended && !s.Closed && s.Stage==ExperienceStage.AwaitAction && !s.FeedbackActive);
        }
        public void ChooseShout() => controller.Send(ExperienceCommand.ChooseAction,"shout");
        public void ChooseWait() => controller.Send(ExperienceCommand.ChooseAction,"wait");
    }
}
