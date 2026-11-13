using System;
using UnityEngine;
using LWS.FireNotification.Domain;
using LWS.FireNotification.Domain.SmokePosture;
using LWS.FireNotification.Interaction;
using LWS.FireNotification.Presentation;

namespace LWS.FireNotification.Runtime
{
    [DefaultExecutionOrder(100)]
    public sealed class SmokePostureController : ContentExperienceAdapter
    {
        public SmokeGuidanceCatalog catalog;
        public SmokePostureTuningAsset tuning;
        public HeadPoseSampleProvider poses;
        public ExperienceLifecycle lifecycle;
        public GuidancePresenter guidance => view != null && view.Owner == this ? view.guidance : null;
        public SmokeLayerPresenter smoke;
        public SmokePostureValidationDisplay validation;
        public SmokePostureSession Session { get; private set; }
        private static long nextRun;
        private bool userPaused, configurationError, practiceArmed, wasBlocked;
        private bool systemRecoveryPending;
        private PresentationRun playback;
        private SmokeStage shownStage=SmokeStage.Closed;
        private long shownExecution;
        private Action pendingGuidance;
        public override bool Blocked => Session==null || Session.Blocked;
        public override bool ResultVisible => Session!=null && Session.Stage==SmokeStage.Result;
        public override string ResultText => "연기 속에서 자세 낮추기\n자세 연습 완료\n연기 관찰 · 몸 낮추기 연습\n실제 대피 완료는 아닙니다.";
        private PostureTuning Values => tuning.tuning;
#if UNITY_EDITOR
        private bool debugInput;
        private HeadPoseSample debugSample;
#endif
        private void OnEnable() { if(lifecycle!=null)lifecycle.Changed+=LifecycleChanged; }
        private void OnDisable() { if(lifecycle!=null)lifecycle.Changed-=LifecycleChanged; Stop(); }
        private void LifecycleChanged(bool blocked)
        {
            if(Session==null || Session.Closed)return;
            if(blocked)
            {
                systemRecoveryPending=true;
                practiceArmed=false;
                // Invalidate queued narration callbacks before the session can resume.
                playback?.Dispose();playback=null;pendingGuidance=null;guidance?.Cancel();
            }
            UpdateBlockers(Read(0));
        }
        private HeadPoseSample Read(float delta)
        {
#if UNITY_EDITOR
            if(debugInput) { var value=debugSample;value.deltaSeconds=0;value.lifecycleAllowed=lifecycle!=null&&!lifecycle.Blocked;return value; }
#endif
            return poses!=null ? poses.Read(lifecycle!=null&&!lifecycle.Blocked,delta) : default;
        }
        private void UpdateBlockers(HeadPoseSample sample)
        {
            if(Session==null || Session.Closed)return;
            var blockers=SmokeBlocker.None;
            if(userPaused)blockers|=SmokeBlocker.UserPause;
            if(lifecycle==null || lifecycle.Blocked)blockers|=SmokeBlocker.Lifecycle;
            if(!sample.positionTracked)blockers|=SmokeBlocker.Tracking;
            if(configurationError || poses==null || !poses.ConfigurationValid || tuning==null)blockers|=SmokeBlocker.Configuration;
            else if(tuning.tuning==null || (Session.BaselineHeight.HasValue && !tuning.tuning.Valid(Session.BaselineHeight.Value)) ||
                (!Session.BaselineHeight.HasValue && sample.Valid && !tuning.tuning.Valid(sample.heightInOrigin)))blockers|=SmokeBlocker.Configuration;
            Session.SetBlockers(blockers);
            bool blocked=Session.Blocked;
            if(blocked!=wasBlocked)
            {
                // System recovery restarts guidance only after every blocker is cleared.
                if(blocked || !systemRecoveryPending)guidance?.SetPaused(blocked);
                smoke?.SetPaused(blocked);wasBlocked=blocked;Publish();
            }
        }
        public override void Begin()
        {
            if(Session!=null&&!Session.Closed)return;
            Session=new SmokePostureSession(++nextRun);userPaused=false;configurationError=false;wasBlocked=false;systemRecoveryPending=false;shownStage=SmokeStage.Closed;shownExecution=0;
            view?.Bind(this, "연기 속에서 자세 낮추기");
            try
            {
                if(catalog==null || tuning==null || guidance==null || smoke==null || poses==null || lifecycle==null || view==null)throw new InvalidOperationException("Missing smoke references");
                if(guidance.voice==null || guidance.subtitle==null || !smoke.ConfigurationValid || tuning.tuning==null)
                    throw new InvalidOperationException("Missing smoke presentation configuration");
                catalog.Validate();
            }
            catch(Exception error) { configurationError=true;Debug.LogWarning("Smoke configuration: "+error.Message); }
            Present(); UpdateBlockers(Read(0));RefreshStatus();Publish();
        }
        public void Ready()
        {
            if(Session==null)return;
            var sample=Read(0);UpdateBlockers(sample);
            if(Session.Blocked || tuning==null) { RefreshStatus();return; }
            if(Session.Ready(sample,Values)) { Present();Publish(); }
            RefreshStatus();
        }
        private void LateUpdate()
        {
            if(Session==null||Session.Closed)return;
            var sample=Read(Time.unscaledDeltaTime);
            UpdateBlockers(sample);
            if(!Session.Blocked)
            {
                if(Session.BaselineHeight.HasValue && sample.referenceRevision!=Session.ReferenceRevision && !Session.PostureCompleted)Session.ReferenceChanged();
                if(systemRecoveryPending)
                {
                    systemRecoveryPending=false;
                    Present();
                }
                var pending=pendingGuidance;pendingGuidance=null;pending?.Invoke();
                bool low=Session.LowRecognized;
                if(practiceArmed)Session.Step(sample,Values);
                if(!low&&Session.LowRecognized)
                {
                    validation?.MarkBoundary(Session.RunId);
                    view.SetStatus(this,"낮은 자세가 인식됐어요. 잠시 유지해 주세요.");
                    validation?.MarkFeedback(Session.RunId);
                    Play("recognized",null);
                }
            }
            if(Session.Execution!=shownExecution || Session.Stage!=shownStage)Present();
            RefreshStatus();
        }
        private void Play(string key,Action done)
        {
            playback?.Dispose();guidance.Cancel();
            var current=Session;long execution=current.Execution;
            var run=new PresentationRun(execution,ExperienceStage.SituationGuidance,null);playback=run;
            guidance.Play(catalog.Get(key),run,()=>
            {
                if(playback==run && Session==current && current.Execution==execution && !current.Closed)
                    pendingGuidance=()=> { if(playback==run && Session==current && current.Execution==execution && !current.Blocked)done?.Invoke(); };
            });
        }
        private void Present()
        {
            playback?.Dispose();guidance?.Cancel();pendingGuidance=null;
            shownStage=Session.Stage;shownExecution=Session.Execution;practiceArmed=false;
            if(view!=null) { if(Session.Stage==SmokeStage.AwaitingReady && !configurationError)view.SetActions(this,new ContentUIAction("ready","준비 완료",Ready));else view.SetActions(this); }
            if(configurationError)return;
            var run=Session.RunId;var execution=Session.Execution;
            switch(Session.Stage)
            {
                case SmokeStage.AwaitingReady: smoke.Stop();Play("ready",null);break;
                case SmokeStage.PrincipleGuidance: smoke.Collect();Play("principle",()=>Session.GuidanceDone(run,execution));break;
                case SmokeStage.Practice: smoke.Steady();Play("practice",()=>practiceArmed=true);break;
                case SmokeStage.CompletionGuidance:
                    smoke.Steady();Play("stand",()=>Play("completion",()=>Session.GuidanceDone(run,execution)));break;
                case SmokeStage.Result: smoke.Steady();break;
            }
            if(Session.Blocked) { guidance.SetPaused(true);smoke.SetPaused(true); }
            Publish();
        }
        private void RefreshStatus()
        {
            if(view==null || Session==null || Session.Closed)return;
            string message;
            if((Session.Blockers&SmokeBlocker.Configuration)!=0)message="체험을 시작할 수 없습니다. 콘텐츠 선택으로 돌아가 주세요.";
            else if((Session.Blockers&SmokeBlocker.Tracking)!=0)message="머리 위치를 확인할 수 없어 잠시 멈췄습니다. 추적 복구를 기다리거나 메뉴로 나갈 수 있습니다.";
            else if(Session.Stage==SmokeStage.AwaitingReady)message="편하게 서서 준비 완료를 선택하세요.\n불편하면 언제든 메뉴에서 중단할 수 있습니다.";
            else if(Session.Stage==SmokeStage.Practice)message=!practiceArmed?"안내를 듣고 편한 범위에서 연습하세요.":Session.LowRecognized?"낮은 자세가 인식됐어요. 잠시 유지해 주세요.":"무리하지 않는 범위에서 머리와 몸을 낮춰 보세요.\n이동할 필요는 없습니다. 불편하면 중단하세요.";
            else if(Session.Stage==SmokeStage.CompletionGuidance)message="자세 연습을 마쳤어요. 편하게 일어서세요.\n낮은 자세만으로 안전해지는 것은 아닙니다.";
            else if(Session.Stage==SmokeStage.PrincipleGuidance)message="연기가 위로 올라가 모이는 모습을 관찰하세요.";
            else message="";
            view.SetStatus(this,message);
        }
        public override void SetUserPaused(bool value) { userPaused=value;if(Session!=null)UpdateBlockers(Read(0)); }
        public override void RequestReturn() { if(Session==null||Session.Closed)return;if(ResultVisible)ReturnToMenu();else RequestExit(); }
        public override void ReturnToMenu() { Stop();RequestMenu(); }
        public override void Stop()
        {
            playback?.Dispose();playback=null;pendingGuidance=null;guidance?.Cancel();smoke?.Stop();validation?.Clear();
            Session?.Close();userPaused=false;practiceArmed=false;systemRecoveryPending=false;
            view?.Clear(this);
#if UNITY_EDITOR
            debugInput=false;debugSample=default;
#endif
            Publish();
        }
        public override void Restart() { Stop();Session=null;Begin(); }
        public override void ApplySettings(float voice,float effects,bool subtitles) { /* Common view owns narration settings. */ }
#if UNITY_EDITOR
        public void DebugPose(double height,bool tracked=true,long revision=0)
        { debugInput=true;debugSample=new HeadPoseSample{heightInOrigin=height,positionTracked=tracked,referenceRevision=revision,lifecycleAllowed=true}; }
        public void DebugAdvance(double seconds)
        {
            if(Session==null)return;
            var sample=Read(0);sample.deltaSeconds=seconds;UpdateBlockers(sample);
            if(practiceArmed)Session.Step(sample,Values);
            if(Session.Stage!=shownStage || Session.Execution!=shownExecution)Present();RefreshStatus();
        }
#endif
    }
}
