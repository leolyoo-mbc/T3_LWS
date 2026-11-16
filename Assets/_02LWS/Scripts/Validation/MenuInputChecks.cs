#if UNITY_EDITOR
using System;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.InputSystem.XR;
using LWS.FireNotification.Runtime;

namespace LWS.FireNotification.Validation
{
    public static class MenuInputChecks
    {
        // Run with a live XR controller. A mock layout cannot establish device compatibility.
        public static string ValidateLiveBinding()
        {
            var ui = SharedUIController.Active;
            if (!Application.isPlaying || ui == null)
                throw new InvalidOperationException("Start Play Mode with SharedUIController");
            var action = ui.menuInputs.FindAction("Left/Menu", true);
            var expected = InputSystem.devices.OfType<XRController>()
                .Where(device => device.added && device.enabled && device.usages.Any(usage => usage == "LeftHand"))
                .SelectMany(device => device.allControls.OfType<ButtonControl>())
                .Where(control => control.usages.Any(usage => usage == "MenuButton"))
                .ToArray();
            if (expected.Length == 0)
                throw new InvalidOperationException("No live left XR MenuButton control; device compatibility is unverified");
            var bound = action.controls.ToArray();
            var detail = "expected=" + string.Join(",", expected.Select(control => control.path)) +
                " bound=" + string.Join(",", bound.Select(control => control.path));
            if (!action.enabled || expected.Any(control => !bound.Contains(control)) ||
                bound.Any(control => !expected.Contains(control)))
                throw new InvalidOperationException("Left/Menu does not exclusively bind live left MenuButton controls: " + detail);
            return "PASS " + detail;
        }
    }
}
#endif
