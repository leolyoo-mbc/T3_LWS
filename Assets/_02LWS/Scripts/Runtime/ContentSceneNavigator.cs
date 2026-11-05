using System.Collections;
using UnityEngine;
using Unity.XR.CoreUtils;

namespace LWS.FireNotification.Runtime
{
    public sealed class ContentSceneNavigator : MonoBehaviour
    {
        public XROrigin origin;
        public GameObject menuRoot;
        public GameObject contentRoot;
        public Transform menuAnchor;
        public Transform contentAnchor;
        public FireNotificationController controller;
        public bool Loading { get; private set; }
        public bool InContent { get; private set; }
        private void OnEnable(){if(controller!=null)controller.MenuRequested+=LoadMenu;}
        private void OnDisable(){if(controller!=null)controller.MenuRequested-=LoadMenu;}
        private void Start()
        {
            contentRoot.SetActive(false);
            menuRoot.SetActive(true);
            MoveTo(menuAnchor);
        }
        public void StartContent()
        {
            if (!Loading && !InContent) StartCoroutine(Navigate(true));
        }
        private void LoadMenu()
        {
            if (!Loading && InContent) StartCoroutine(Navigate(false));
        }
        private IEnumerator Navigate(bool enterContent)
        {
            Loading = true;
            // Finish the selecting UI event before disabling its hierarchy.
            yield return null;
            menuRoot.SetActive(false);
            contentRoot.SetActive(false);
            MoveTo(enterContent ? contentAnchor : menuAnchor);
            InContent = enterContent;
            if (enterContent)
            {
                contentRoot.SetActive(true);
                controller.BeginExperience();
            }
            else menuRoot.SetActive(true);
            Loading = false;
        }
        private void MoveTo(Transform anchor)
        {
            // Preserve head height and compensate for room-scale head offsets.
            origin.MatchOriginUpCameraForward(Vector3.up, anchor.forward);
            origin.MoveCameraToWorldLocation(anchor.position + Vector3.up * origin.CameraInOriginSpaceHeight);
        }
    }
}
