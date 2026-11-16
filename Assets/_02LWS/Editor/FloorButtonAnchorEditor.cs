#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using LWS.FireNotification.Runtime;

namespace LWS.FireNotification.Editor
{
    [CustomEditor(typeof(FloorButtonAnchor))]
    public sealed class FloorButtonAnchorEditor : UnityEditor.Editor
    {
        private static GameObject preview;
        private bool two=true, large;
        [InitializeOnLoadMethod]
        private static void RegisterCleanup()
        {
            Selection.selectionChanged+=Clear;
            AssemblyReloadEvents.beforeAssemblyReload+=Clear;
            EditorApplication.playModeStateChanged+=_=>Clear();
        }
        private static void Clear(){if(preview!=null)Object.DestroyImmediate(preview);preview=null;}
        private void OnDisable()=>Clear();
        public override void OnInspectorGUI()
        {
            EditorGUI.BeginChangeCheck();DrawDefaultInspector();
            if(EditorGUI.EndChangeCheck())Clear();
            var anchor=(FloorButtonAnchor)target;
            foreach(var issue in anchor.ConfigurationIssues())EditorGUILayout.HelpBox(issue,MessageType.Error);
            EditorGUILayout.Space();EditorGUILayout.LabelField("바닥판 미리보기",EditorStyles.boldLabel);
            two=EditorGUILayout.Toggle("선택지 2개",two);large=EditorGUILayout.Toggle("글자 150%",large);
            using(new EditorGUI.DisabledScope(Application.isPlaying))
            {
                if(GUILayout.Button("숨김 위치 미리보기"))Show(anchor,false);
                if(GUILayout.Button("완전 상승 미리보기"))Show(anchor,true);
                if(GUILayout.Button("미리보기 해제"))Clear();
            }
        }
        private void Show(FloorButtonAnchor anchor,bool raised)
        {
            Clear();
            var prefab=AssetDatabase.LoadAssetAtPath<GameObject>("Assets/_02LWS/Prefabs/UI/FloorButtonPanel.prefab");
            if(prefab==null)return;
            preview=Object.Instantiate(prefab);preview.name="FloorButtonPreview (not saved)";
            foreach(var t in preview.GetComponentsInChildren<Transform>(true))
            {
                t.gameObject.hideFlags=HideFlags.HideAndDontSave;
                foreach(var c in t.GetComponents<Component>())c.hideFlags=HideFlags.HideAndDontSave;
            }
            preview.GetComponent<FloorButtonPresenter>().Preview(anchor,raised,two,large?1.5f:1);
            SceneView.RepaintAll();
        }
    }
}
#endif
