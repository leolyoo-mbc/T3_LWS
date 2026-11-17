#if UNITY_EDITOR
using System;
using System.IO;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using TMPro;
using LWS.FireNotification.Runtime;
using LWS.FireNotification.Presentation;

namespace LWS.FireNotification.Validation
{
    public static class SubtitlePresentationChecks
    {
        public static string EvidenceDirectory;
        public static bool Running { get; private set; }
        public static int Passed { get; private set; }
        public static int Failed { get; private set; }
        public static readonly string[] CatalogPaths = {
            "Assets/_02LWS/Content/GuidanceCatalog.asset",
            "Assets/_02LWS/Data/SmokePosture/GuidanceCatalog.asset",
            "Assets/_02LWS/Data/EvacuationRoute/RouteGuidance.asset"
        };
        public static GuidanceCue[] Cues(string path)
        {
            var asset=AssetDatabase.LoadMainAssetAtPath(path);
            return (GuidanceCue[])asset.GetType().GetField("cues").GetValue(asset);
        }
        [Serializable] private sealed class Baseline { public BaselineCue[] cues; public StatusSample[] statusSamples; }
        [Serializable] private sealed class BaselineCue { public string catalog,id,text,audio,hash; }
        [Serializable] private sealed class StatusSample { public string kind,text; public float scale,forward,downwardAngle,angularHeight,angularWidth; }
        private static void Check(bool ok,string message)
        {
            if(ok)Passed++;else Failed++;
            if(!string.IsNullOrEmpty(EvidenceDirectory))
            {
                Directory.CreateDirectory(EvidenceDirectory);
                File.AppendAllText(EvidenceDirectory+"/subtitle-checks.txt",(ok?"PASS ":"FAIL ")+message+"\n");
            }
            if(!ok)Debug.LogWarning("Subtitle validation: "+message);
        }
        [MenuItem("Tools/LWS/Validate Subtitle Configuration")]
        public static void ValidateConfiguration()
        {
            if(Running)throw new InvalidOperationException("Subtitle checks already running");
            Passed=Failed=0;
            var nav=UnityEngine.Object.FindFirstObjectByType<ContentSceneNavigator>(FindObjectsInactive.Include);
            foreach(var error in SharedUIContractAudit.Validate(nav))Check(false,error);
            var view=nav.contentUI;var text=view.guidance.subtitle;
            var originalPanel=view.panel;
            try
            {
                view.panel=null;
                try {Check(SharedUIContractAudit.Validate(nav).Count>0,"missing panel returns configuration errors without throwing");}
                catch(Exception error){Check(false,"missing panel audit threw "+error.GetType().Name);}
            }
            finally {view.panel=originalPanel;}
            Baseline baseline=null;
            if(!string.IsNullOrEmpty(EvidenceDirectory)&&File.Exists(EvidenceDirectory+"/baseline.json"))
                baseline=JsonUtility.FromJson<Baseline>(File.ReadAllText(EvidenceDirectory+"/baseline.json"));
            var root=text.transform.parent as RectTransform;
            Check(root!=null&&root.name=="SubtitleRoot"&&root.parent==view.panel.transform&&view.status.transform.parent==root,"shared guidance root under ContentPanel");
            Check(root!=null&&Vector3.Distance(root.localScale,new Vector3(5f/9,5f/9,5f/9))<.001f,"subtitle scale preserves angular size");
            Check(root!=null&&root.GetComponent<Canvas>()==null&&root.GetComponent<UnityEngine.UI.Image>()==null&&root.GetComponent<CanvasGroup>()==null,"no subtitle canvas or background");
            Check(!text.raycastTarget,"subtitle does not intercept controller input");
            Check(!view.status.raycastTarget&&view.status.fontSharedMaterial==text.fontSharedMaterial,"status uses the same noninteractive outline style");
            var panelBackground=view.panel.GetComponent<UnityEngine.UI.Image>();
            Check(panelBackground==null||!panelBackground.enabled,"guidance panel background disabled");
            var mat=text.fontSharedMaterial;
            Check(AssetDatabase.GetAssetPath(mat)=="Assets/_02LWS/Art/Materials/SubtitleOutline.mat","dedicated subtitle material");
            Check(mat.IsKeywordEnabled("OUTLINE_ON")&&Mathf.Approximately(mat.GetFloat("_OutlineWidth"),.45f)&&
                Mathf.Approximately(mat.GetFloat("_FaceDilate"),.30f)&&mat.GetColor("_OutlineColor")==Color.black&&mat.GetColor("_FaceColor")==Color.white,"white face and black outline preset");
            Check(!mat.IsKeywordEnabled("UNDERLAY_ON")&&!mat.IsKeywordEnabled("UNDERLAY_INNER"),"no additional shadow");
            foreach(var other in nav.gameObject.scene.GetRootGameObjects().SelectMany(x=>x.GetComponentsInChildren<TMP_Text>(true)))
                if(other!=text&&other!=view.status)Check(other.fontSharedMaterial!=mat||AssetDatabase.GetAssetPath(mat)!="Assets/_02LWS/Art/Materials/SubtitleOutline.mat","material isolation: "+other.name);
            var go=new GameObject("Subtitle measurement",typeof(RectTransform),typeof(Canvas));go.hideFlags=HideFlags.HideAndDontSave;
            try
            {
                var child=new GameObject("Measurement text",typeof(RectTransform));child.hideFlags=HideFlags.HideAndDontSave;child.transform.SetParent(go.transform,false);
                var measure=child.AddComponent<TextMeshProUGUI>();measure.font=text.font;measure.fontSharedMaterial=mat;
                measure.enableAutoSizing=false;measure.textWrappingMode=TextWrappingModes.Normal;
                measure.overflowMode=TextOverflowModes.Overflow;measure.rectTransform.sizeDelta=new Vector2(1352,1000);
                var themed=text.GetComponent<ThemedText>();
                foreach(var path in CatalogPaths)foreach(var cue in Cues(path))
                {
                    if(baseline!=null)
                    {
                        var original=baseline.cues.Single(x=>x.catalog==path&&x.id==cue.id);
                        using(var sha=System.Security.Cryptography.SHA256.Create())
                        {
                            var audioPath=AssetDatabase.GetAssetPath(cue.audio);
                            var hash=BitConverter.ToString(sha.ComputeHash(File.ReadAllBytes(audioPath))).Replace("-","");
                            Check(cue.text==original.text&&audioPath==original.audio&&hash==original.hash,"approved text/audio unchanged: "+path+"/"+cue.id);
                        }
                    }
                    try {cue.Validate();Check(true,path+"/"+cue.id+" timing/text");}
                    catch(Exception ex){Check(false,path+"/"+cue.id+": "+ex.Message);}
                    foreach(var segment in cue.segments)foreach(float scale in new[]{1f,1.5f})
                    {
                        measure.fontSize=themed.baseSize*scale;measure.text=segment.text;measure.ForceMeshUpdate(true,true);
                        Check(measure.textInfo.characterCount>0&&measure.textInfo.lineCount>=1&&measure.textInfo.lineCount<=2&&!measure.isTextOverflowing,path+"/"+cue.id+" "+segment.start+" @"+scale+" <=2 lines (actual "+measure.textInfo.lineCount+")");
                    }
                }
            }
            finally {UnityEngine.Object.DestroyImmediate(go);}
            Debug.Log("Subtitle configuration: "+Passed+" passed, "+Failed+" failed");
            if(Failed>0)throw new InvalidOperationException("Subtitle configuration failed: "+Failed);
        }
        [MenuItem("Tools/LWS/Validate Subtitle Presentation")]
        public static void RunPlayMode()
        {
            if(!Application.isPlaying||Running)throw new InvalidOperationException("Start Play Mode with no subtitle checks running");
            var nav=UnityEngine.Object.FindFirstObjectByType<ContentSceneNavigator>();
            if(nav.InContent)throw new InvalidOperationException("Return to content selection before running subtitle checks");
            Passed=Failed=0;Running=true;nav.StartCoroutine(Guard(Run(nav)));
        }
        private static IEnumerator Guard(IEnumerator run)
        {
            try
            {
                while(true)
                {
                    bool more;object next=null;
                    try {more=run.MoveNext();if(more)next=run.Current;}
                    catch(Exception error){Check(false,"runner exception: "+error);break;}
                    if(!more)break;yield return next;
                }
            }
            finally {(run as IDisposable)?.Dispose();Running=false;}
        }
        private static void Tick(SharedContentUI view) {view.SendMessage("LateUpdate");}
        private static void StatusGeometry(SharedContentUI view,string content)
        {
            var text=view.status;string previous=text.text;bool subtitles=view.SubtitlesEnabled;
            var theme=text.GetComponent<ThemedText>();float previousScale=text.fontSize/theme.baseSize;
            var follower=view.GetComponentInParent<ComfortFollowUI>();follower.Recenter();
            Baseline baseline=null;
            if(!string.IsNullOrEmpty(EvidenceDirectory)&&File.Exists(EvidenceDirectory+"/baseline.json"))
                baseline=JsonUtility.FromJson<Baseline>(File.ReadAllText(EvidenceDirectory+"/baseline.json"));
            try
            {
                foreach(var kind in new[]{ContentStatusKind.Instruction,ContentStatusKind.Warning})foreach(float scale in new[]{1f,1.5f})
                {
                    string message=kind==ContentStatusKind.Instruction?"주변에 화재 사실을 알려 주세요.":"머리 위치를 확인할 수 없어 잠시 멈췄습니다. 추적 복구를 기다리거나 메뉴로 나갈 수 있습니다.";
                    theme.Apply(scale);view.SetStatus(view.Owner,message,kind);
                    view.ApplySettings(SharedUIController.Active.VoiceVolume,kind==ContentStatusKind.Warning);Tick(view);text.ForceMeshUpdate(true,true);
                    var delta=text.transform.position-follower.head.position;
                    float forward=Vector3.Dot(delta,follower.transform.forward);
                    float angle=Mathf.Atan2(-Vector3.Dot(delta,follower.transform.up),forward)*Mathf.Rad2Deg;
                    float height=text.rectTransform.rect.height*text.transform.lossyScale.y/forward;
                    float width=text.rectTransform.rect.width*text.transform.lossyScale.x/forward;
                    var before=baseline?.statusSamples?.FirstOrDefault(s=>s.kind==kind.ToString()&&s.scale==scale&&s.text==message);
                    float referenceHeight=before!=null?before.angularHeight:text.rectTransform.rect.height*.001f/1.8f;
                    float referenceAngle=before!=null?before.downwardAngle:Mathf.Atan2(.53f+text.rectTransform.rect.height*.0005f,1.8f)*Mathf.Rad2Deg;
                    float referenceWidth=before!=null?before.angularWidth:1.352f/1.8f;
                    string context=content+" "+kind+" @"+scale;
                    Check(text.enabled&&text.gameObject.activeInHierarchy&&!view.guidance.subtitle.enabled,"only status visible: "+context);
                    Check(Mathf.Abs(forward-1)<.02f,"status forward distance 1m: "+context);
                    Check(Mathf.Abs(angle-referenceAngle)/referenceAngle<=.05f&&Mathf.Abs(height-referenceHeight)/referenceHeight<=.05f&&Mathf.Abs(width-referenceWidth)/referenceWidth<=.05f,"status apparent size and angle preserved: "+context);
                    Check(text.fontSharedMaterial==view.subtitleMaterial&&text.color==Color.white&&!text.raycastTarget,"shared status preset after settings: "+context);
                    Check(!view.panel.GetComponent<UnityEngine.UI.Image>().enabled,"no instruction or warning background: "+context);
                    Check(text.text==message&&text.textInfo.characterCount>0&&!text.isTextOverflowing&&!text.enableAutoSizing&&Mathf.Approximately(text.fontSize,theme.baseSize*scale),"full message without shrinking or clipping: "+context);
                    view.panel.SetActive(false);Check(!text.gameObject.activeInHierarchy,"status inherits panel hide: "+context);
                    view.panel.SetActive(true);Tick(view);Check(text.color==Color.white&&text.fontSharedMaterial==view.subtitleMaterial,"status preset after reactivation: "+context);
                    if(!string.IsNullOrEmpty(EvidenceDirectory))File.AppendAllText(EvidenceDirectory+"/status-geometry.txt",context+": forward="+forward+", angle="+angle+", beforeAngle="+referenceAngle+", angularHeight="+height+", beforeHeight="+referenceHeight+", lines="+text.textInfo.lineCount+"\n");
                }
            }
            finally {text.text=previous;theme.Apply(previousScale);view.SetStatus(view.Owner,previous);view.ApplySettings(SharedUIController.Active.VoiceVolume,subtitles);Tick(view);}
        }
        private static void Geometry(SharedContentUI view,string content)
        {
            var text=view.guidance.subtitle;string previous=text.text;
            var theme=text.GetComponent<ThemedText>();float previousScale=text.fontSize/theme.baseSize;
            var follower=view.GetComponentInParent<ComfortFollowUI>();follower.Recenter();
            foreach(float scale in new[]{1f,1.5f})
            {
                text.text="주변에 화재 사실을 알려 주세요.";theme.Apply(scale);
                view.ApplySettings(SharedUIController.Active.VoiceVolume,true);Tick(view);
                var delta=text.transform.position-follower.head.position;
                float forward=Vector3.Dot(delta,follower.transform.forward);
                float down=-Vector3.Dot(delta,follower.transform.up);
                float angle=Mathf.Atan2(down,forward)*Mathf.Rad2Deg;
                float baselineAngle=Mathf.Atan2(scale==1?.5775f:.589f,1.8f)*Mathf.Rad2Deg;
                float angularHeight=text.rectTransform.rect.height*text.transform.lossyScale.y/forward;
                float baselineHeight=(scale==1?47f:70f)*.001f/1.8f;
                Check(Mathf.Abs(forward-1)<.02f,"100/150 stable forward distance: "+content+" @"+scale);
                Check(Mathf.Abs(angle-baselineAngle)/baselineAngle<=.05f,"downward angle within baseline 5%: "+content+" @"+scale);
                Check(Mathf.Abs(angularHeight-baselineHeight)/baselineHeight<=.05f,"angular height within baseline 5%: "+content+" @"+scale);
                Check(text.fontSharedMaterial==view.subtitleMaterial&&text.color==Color.white,"preset survives size apply: "+content+" @"+scale);
                if(!string.IsNullOrEmpty(EvidenceDirectory))File.AppendAllText(EvidenceDirectory+"/geometry.txt",content+" @"+scale+": forward="+forward+", downwardAngle="+angle+", baselineAngle="+baselineAngle+", angularHeight="+angularHeight+", baselineHeight="+baselineHeight+"\n");
            }
            text.text=previous;theme.Apply(previousScale);view.ApplySettings(SharedUIController.Active.VoiceVolume,true);Tick(view);
        }
        private static IEnumerator Run(ContentSceneNavigator nav)
        {
            var view=nav.contentUI;bool subtitles=SharedUIController.Active.SubtitlesEnabled;
            float originalPitch=view.guidance.voice.pitch;
            var sample=AudioClip.Create("Subtitle timing fixture",44100,1,44100,false);
            var cue=new GuidanceCue{id="validation",audio=sample,text="첫 문장 다음 문장",segments=new[]{
                new SubtitleSegment{start=.05f,end=.3f,text="첫 문장"},new SubtitleSegment{start=.65f,end=.95f,text="다음 문장"}}};
            PresentationRun current=null;
            try
            {
                foreach(var entry in nav.contents)
                {
                    nav.StartContent(entry.id);
                    yield return new WaitForSecondsRealtime(.5f);
                    Check(nav.ActiveAdapter==entry.adapter&&view.Owner==entry.adapter,"owner entered: "+entry.id);
                    view.guidance.Cancel();view.ApplySettings(1,true);
                    StatusGeometry(view,entry.id);
                    current=new PresentationRun(99001,default,null);int completed=0;
                    view.SetStatus(entry.adapter,"첫 문장");view.guidance.Play(cue,current,()=>completed++);
                    yield return new WaitForSecondsRealtime(.12f);Tick(view);
                    Check(!view.status.enabled&&view.guidance.subtitle.enabled,"narration suppresses instruction: "+entry.id);
                    Check(view.guidance.subtitle.text=="첫 문장","current segment: "+entry.id);
                    Geometry(view,entry.id);
                    var follower=view.GetComponentInParent<ComfortFollowUI>();follower.Recenter();
                    var subtitle=view.guidance.subtitle;
                    Check(Mathf.Abs(Vector3.Dot(subtitle.transform.position-follower.head.position,follower.transform.forward)-1)<.02f,"stable subtitle forward distance 1m: "+entry.id);
                    Check(Mathf.Abs(subtitle.rectTransform.rect.width*subtitle.transform.lossyScale.x-.7511111f)<.002f,"angular width preserved: "+entry.id);
                    view.ApplySettings(1,false);Tick(view);
                    Check(view.status.enabled&&!view.guidance.subtitle.enabled,"subtitles off preserves instruction: "+entry.id);
                    view.ApplySettings(1,true);Tick(view);
                    Check(!view.status.enabled&&view.guidance.subtitle.enabled,"toggle on restores current narration: "+entry.id);
                    view.guidance.SetPaused(true);float before=view.guidance.Elapsed;
                    yield return new WaitForSecondsRealtime(.1f);Tick(view);
                    Check(view.guidance.IsPresenting&&Mathf.Abs(before-view.guidance.Elapsed)<.03f,"pause retains playback position: "+entry.id);
                    view.guidance.SetPaused(false);
                    yield return new WaitForSecondsRealtime(.3f);Tick(view);
                    Check(view.guidance.IsPresenting&&view.guidance.subtitle.text==""&&!view.status.enabled,"no instruction flash in subtitle gap: "+entry.id);
                    view.SetStatus(entry.adapter,"추적 복구를 기다리세요.",ContentStatusKind.Warning);Tick(view);
                    Check(view.status.enabled&&!view.guidance.subtitle.enabled,"warning only during narration: "+entry.id);
                    view.SetStatus(entry.adapter,"현재 행동 안내");Tick(view);
                    Check(!view.status.enabled&&view.guidance.subtitle.enabled,"warning clear uses current narration: "+entry.id);
                    view.ApplySettings(1,false);view.SetStatus(entry.adapter,"경고",ContentStatusKind.Warning);Tick(view);
                    view.SetStatus(entry.adapter,"현재 행동 안내");Tick(view);
                    Check(view.status.enabled&&!view.guidance.subtitle.enabled,"warning clear does not enable disabled subtitles: "+entry.id);
                    view.ApplySettings(1,true);view.SetStatus(entry.adapter,"",ContentStatusKind.Warning);Tick(view);
                    Check(!view.status.enabled&&view.guidance.subtitle.enabled,"empty warning does not suppress narration: "+entry.id);
                    view.guidance.voice.timeSamples=(int)(sample.frequency*.72f);yield return null;Tick(view);
                    Check(Mathf.Abs(view.guidance.Elapsed-(float)view.guidance.voice.timeSamples/sample.frequency)<.04f&&view.guidance.subtitle.text=="다음 문장","subtitle follows actual audio sample position: "+entry.id);
                    view.SetStatus(entry.adapter,"현재 행동 안내");
                    yield return new WaitForSecondsRealtime(.7f);Tick(view);
                    Check(!view.guidance.IsPresenting&&completed==1&&view.status.enabled,"natural end restores current instruction, completion once: "+entry.id);
                    view.guidance.Play(cue,current,()=>completed++);view.guidance.Cancel();Tick(view);
                    Check(!view.guidance.IsPresenting&&view.guidance.subtitle.text==""&&completed==1,"cancel clears subtitle without completion: "+entry.id);
                    current.Dispose();current=null;nav.LoadMenu();yield return new WaitForSecondsRealtime(.3f);
                    Check(view.Owner==null&&!view.panel.activeSelf,"owner cleared on menu: "+entry.id);
                }
                nav.StartContent(nav.contents[0].id);yield return new WaitForSecondsRealtime(.5f);
                view.guidance.Cancel();view.ApplySettings(1,true);
                // Hold the actual audio clock during synthetic boundary inspection. Realtime playback
                // and natural completion are tested above at the original pitch, separately.
                view.guidance.voice.pitch=0;
                foreach(var path in CatalogPaths)foreach(var authored in Cues(path))
                {
                    current=new PresentationRun(99002,default,null);int complete=0;
                    view.guidance.Play(authored,current,()=>complete++);
                    foreach(var segment in authored.segments)
                    {
                        view.guidance.voice.timeSamples=(int)(authored.audio.frequency*((segment.start+segment.end)/2));
                        yield return null;yield return null;Tick(view);
                        Check(view.guidance.subtitle.text==segment.text,"authored current segment: "+path+"/"+authored.id+"/"+segment.start);
                        Check(Mathf.Abs(view.guidance.Elapsed-(float)view.guidance.voice.timeSamples/authored.audio.frequency)<.04f,"authored audio clock: "+path+"/"+authored.id);
                    }
                    var boundaries=authored.segments.SelectMany(s=>new[]{s.start-.08f,s.start+.08f,s.end-.08f,s.end+.08f})
                        .Where(t=>t>=0&&t<authored.audio.length-.1f).Distinct().ToArray();
                    foreach(float point in boundaries)
                    {
                        string expected=authored.segments.FirstOrDefault(s=>point>=s.start&&point<s.end)?.text??"";
                        view.guidance.voice.timeSamples=(int)(authored.audio.frequency*point);
                        yield return null;yield return null;Tick(view);
                        Check(view.guidance.subtitle.text==expected,"authored boundary/gap "+point+": "+path+"/"+authored.id);
                    }
                    view.guidance.Cancel();current.Dispose();current=null;
                    Check(complete==0,"seek inspection does not invoke completion: "+authored.id);
                }
            }
            finally
            {
                current?.Dispose();view.guidance.Cancel();view.guidance.voice.pitch=originalPitch;view.ApplySettings(SharedUIController.Active.VoiceVolume,subtitles);
                if(nav.InContent)nav.LoadMenu();UnityEngine.Object.Destroy(sample);
            }
        }
    }
}
#endif
