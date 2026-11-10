using UnityEngine.XR.Interaction.Toolkit.Locomotion.Teleportation;
using LWS.FireNotification.Runtime;

namespace LWS.FireNotification.Interaction
{
    public sealed class MenuTeleportationProvider : TeleportationProvider
    {
        public SharedUIController ui;
        public override bool QueueTeleportRequest(TeleportRequest request)
        {
            if (ui != null && (ui.IsOpen || !ui.navigator.MovementAllowed)) return false;
            return base.QueueTeleportRequest(request);
        }
        public void CancelPending()
        {
            validRequest = false;
            TryEndLocomotion();
        }
    }
}
