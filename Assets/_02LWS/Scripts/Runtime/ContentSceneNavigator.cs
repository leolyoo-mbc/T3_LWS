using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace LWS.FireNotification.Runtime
{
    public sealed class ContentSceneNavigator : MonoBehaviour
    {
        public string menuScene="Assets/_02LWS/Scenes/ContentMenu.unity";
        public string contentScene="Assets/_02LWS/Scenes/FireNotification.unity";
        public FireNotificationController controller;
        public bool Loading { get; private set; }
        private void OnEnable(){if(controller!=null)controller.MenuRequested+=LoadMenu;}
        private void OnDisable(){if(controller!=null)controller.MenuRequested-=LoadMenu;}
        public void StartContent()=>Load(contentScene);
        private void LoadMenu()=>Load(menuScene);
        private void Load(string path)
        {
            if(Loading)return;
            if(!Application.CanStreamedLevelBeLoaded(path))throw new System.InvalidOperationException("Scene not registered: "+path);
            Loading=true;StartCoroutine(Navigate(path));
        }
        private IEnumerator Navigate(string path)
        {
            // Finish the input event before replacing its scene and event system.
            yield return null;
            yield return SceneManager.LoadSceneAsync(path,LoadSceneMode.Single);
        }
    }
}
