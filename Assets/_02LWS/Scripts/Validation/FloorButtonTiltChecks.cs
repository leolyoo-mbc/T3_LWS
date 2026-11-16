#if UNITY_EDITOR
using System;
using System.IO;
using System.Linq;
using System.Collections;
using UnityEditor;
using UnityEngine;
using LWS.FireNotification.Runtime;

namespace LWS.FireNotification.Validation
{
    public static partial class FloorButtonChecks
    {
        [Serializable] private sealed class TiltLayout
        {
            public string name;
            public Vector3 position;
            public Quaternion rotation;
            public int count;
            public float scale;
            public Vector3[] corners;
        }
        private static Vector3[] ButtonCorners(FloorButtonPresenter panel)
        {
            var result=new System.Collections.Generic.List<Vector3>();
            foreach(var b in panel.buttons.Where(b=>b.gameObject.activeSelf))
            {var corners=new Vector3[4];((RectTransform)b.transform).GetWorldCorners(corners);result.AddRange(corners);}
            return result.ToArray();
        }
        private static void CheckTiltBoundary(FloorButtonPresenter panel,FloorButtonAnchor anchor,string context)
        {
            var corners=new Vector3[4];panel.viewport.GetWorldCorners(corners);
            float floor=anchor.transform.position.y+anchor.floorClearance;
            Check(Mathf.Abs(corners[0].y-floor)<.001f&&Mathf.Abs(corners[3].y-floor)<.001f,"horizontal floor mask boundary "+context);
        }
        private static void CaptureTilt(Camera camera,FloorButtonAnchor anchor,string name)
        {
            var target=new RenderTexture(1920,1080,24);var image=new Texture2D(1920,1080,TextureFormat.RGB24,false);
            var oldTarget=camera.targetTexture;var oldEye=camera.stereoTargetEye;var active=RenderTexture.active;
            var oldPosition=camera.transform.position;var oldRotation=camera.transform.rotation;
            try
            {
                float yaw=anchor.transform.eulerAngles.y;
                camera.transform.SetPositionAndRotation(anchor.transform.position-Quaternion.Euler(0,yaw,0)*Vector3.forward*1.4f+Vector3.up*1.7f,Quaternion.Euler(12,yaw,0));
                camera.targetTexture=target;camera.stereoTargetEye=StereoTargetEyeMask.None;camera.Render();RenderTexture.active=target;
                image.ReadPixels(new Rect(0,0,1920,1080),0,0);image.Apply();File.WriteAllBytes(Path.Combine(EvidenceDirectory,name+".png"),image.EncodeToPNG());
                CaptureRequested?.Invoke(name);
            }
            finally
            {
                camera.targetTexture=oldTarget;camera.stereoTargetEye=oldEye;camera.transform.SetPositionAndRotation(oldPosition,oldRotation);RenderTexture.active=active;
                target.Release();UnityEngine.Object.Destroy(target);UnityEngine.Object.Destroy(image);
            }
        }
        private static IEnumerator RunTiltCoverage(ContentSceneNavigator nav)
        {
            var route=(EvacuationRouteController)nav.contents.Single(e=>e.id=="safe-evacuation-route").adapter;
            var panel=nav.contentUI.floorButtons;var view=nav.contentUI;GameObject temporary=null;int caseNumber=0;
            try
            {
                route.DebugTracking(true);nav.StartContent("safe-evacuation-route");yield return Wait(()=>nav.ActiveAdapter==route&&!nav.Loading,"tilt route entry");
                temporary=new GameObject("Temporary floor tilt validation anchor");temporary.hideFlags=HideFlags.DontSave;
                var anchor=temporary.AddComponent<FloorButtonAnchor>();anchor.id="coverage-tilt";anchor.transform.position=route.readyAnchor.transform.position;
                anchor.riseSeconds=anchor.lowerSeconds=.8f;
                foreach(var angle in new[]{new Vector3(15,0,0),new Vector3(-15,0,0),new Vector3(0,0,15),new Vector3(0,0,-15),new Vector3(15,0,15)})
                foreach(int count in new[]{1,2})foreach(float scale in new[]{1f,1.5f})
                {
                    string name="tilt-"+(++caseNumber).ToString("00")+"-"+count+"-"+(scale>1?"150":"100");
                    anchor.transform.rotation=route.readyAnchor.transform.rotation*Quaternion.Euler(angle);
                    view.ResetFloorDisplay();panel.ApplySettings(scale);
                    Check(panel.Phase==FloorButtonPhase.Hidden&&panel.group.alpha==0,"tilted hidden "+name);
                    CaptureTilt(nav.origin.Camera,anchor,name+"-hidden");
                    Action clicked=()=>view.SetActions(route);
                    var actions=count==1?new[]{new ContentUIAction("ready","준비 완료",clicked)}:
                        new[]{new ContentUIAction("left","이 계단으로 대피한다",clicked),new ContentUIAction("right","다른 경로를 찾는다",clicked)};
                    view.SetActions(route,anchor,actions);
                    Vector3 start=panel.movingPanel.position;
                    yield return Wait(()=>panel.Progress>=.35f&&panel.Phase==FloorButtonPhase.Rising,"tilted Rising "+name);
                    Vector3 moved=panel.movingPanel.position-start;
                    Check(new Vector2(moved.x,moved.z).magnitude<.001f&&moved.y>0,"world vertical movement "+name);
                    CheckTiltBoundary(panel,anchor,name+" Rising");CaptureTilt(nav.origin.Camera,anchor,name+"-rising");
                    yield return Wait(()=>panel.Phase==FloorButtonPhase.Ready,"tilted Ready "+name);yield return null;Canvas.ForceUpdateCanvases();
                    CheckTiltBoundary(panel,anchor,name+" Ready");
                    var vertices=ButtonCorners(panel);
                    Check(vertices.All(v=>v.y>=anchor.transform.position.y-.001f),"tilted buttons above floor "+name);
                    foreach(var b in panel.buttons.Where(b=>b.gameObject.activeSelf))
                    {
                        var text=b.GetComponentInChildren<TMPro.TMP_Text>();var corners=new Vector3[4];text.rectTransform.GetWorldCorners(corners);
                        Check(text.GetPreferredValues(text.text,text.rectTransform.rect.width,Mathf.Infinity).y<=text.rectTransform.rect.height+1,"tilted full text fits "+name);
                        Check(corners.All(v=>panel.viewport.rect.Contains((Vector2)panel.viewport.InverseTransformPoint(v))),"tilted text inside mask "+name);
                    }
                    var layout=new TiltLayout{name=name,position=anchor.transform.position,rotation=anchor.transform.rotation,count=count,scale=scale,corners=vertices};
                    File.WriteAllText(Path.Combine(EvidenceDirectory,name+".json"),JsonUtility.ToJson(layout,true));
                    CaptureTilt(nav.origin.Camera,anchor,name+"-ready");
                    ClickFloor(panel,0);Check(panel.Phase==FloorButtonPhase.Lowering,"tilted selection lowers "+name);
                    yield return Wait(()=>panel.Progress>=.35f&&panel.Phase==FloorButtonPhase.Lowering,"tilted Lowering "+name);
                    CheckTiltBoundary(panel,anchor,name+" Lowering");CaptureTilt(nav.origin.Camera,anchor,name+"-lowering");
                    yield return Wait(()=>panel.Phase==FloorButtonPhase.Hidden,"tilted lower complete "+name);
                    Check(panel.group.alpha==0,"tilted complete hidden "+name);CaptureTilt(nav.origin.Camera,anchor,name+"-after");
                }
                route.ReturnToMenu();yield return Wait(()=>!nav.InContent&&!nav.Loading,"tilt returns to menu");
            }
            finally
            {
                view.ResetFloorDisplay();panel.ApplySettings(SharedUIController.Active.TextScale);
                if(temporary!=null)UnityEngine.Object.Destroy(temporary);
            }
        }
        public static void ValidateTiltPreviews(string directory)
        {
            if(Application.isPlaying)throw new InvalidOperationException("Tilt preview comparison requires Edit Mode");
            var files=Directory.GetFiles(directory,"tilt-*.json");
            if(files.Length!=20)throw new InvalidOperationException("Expected all 20 runtime tilt records, found "+files.Length);
            foreach(string file in files)
            {
                var layout=JsonUtility.FromJson<TiltLayout>(File.ReadAllText(file));var go=new GameObject("Temporary tilt preview comparison");go.hideFlags=HideFlags.DontSave;
                UnityEditor.Editor inspector=null;
                try
                {
                    var anchor=go.AddComponent<FloorButtonAnchor>();anchor.id="preview-tilt-comparison";go.transform.SetPositionAndRotation(layout.position,layout.rotation);
                    Selection.activeGameObject=go;inspector=UnityEditor.Editor.CreateEditor(anchor);var type=inspector.GetType();
                    var flags=System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.NonPublic;
                    type.GetField("two",flags).SetValue(inspector,layout.count==2);type.GetField("large",flags).SetValue(inspector,layout.scale>1);
                    type.GetMethod("Show",flags).Invoke(inspector,new object[]{anchor,true});
                    var preview=Resources.FindObjectsOfTypeAll<FloorButtonPresenter>().Single(p=>(p.hideFlags&HideFlags.DontSave)!=0);
                    var actual=ButtonCorners(preview);
                    if(actual.Length!=layout.corners.Length||actual.Where((v,i)=>Vector3.Distance(v,layout.corners[i])>.001f).Any())throw new InvalidOperationException("Preview/runtime geometry mismatch: "+layout.name);
                    if(preview.group.blocksRaycasts||preview.buttons.Any(b=>b.enabled))throw new InvalidOperationException("Preview input enabled");
                    File.AppendAllText(Path.Combine(directory,"tilt-preview.txt"),"PASS "+layout.name+" geometry within 1mm, input disabled\n");
                }
                finally {Selection.activeGameObject=null;if(inspector!=null)UnityEngine.Object.DestroyImmediate(inspector);UnityEngine.Object.DestroyImmediate(go);}
            }
        }
    }
}
#endif
