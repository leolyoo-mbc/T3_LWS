using System;
using UnityEngine;
using LWS.FireNotification.Domain;

namespace LWS.FireNotification.Presentation
{
    [CreateAssetMenu(menuName="LWS/Action Options")]
    public sealed class ActionOptions : ScriptableObject
    {
        public ActionOption[] options;
        public ActionOption Get(string id) => Array.Find(options, x=>x.id==id) ?? throw new InvalidOperationException("Unknown option: "+id);
        public void Validate(GuidanceCatalog catalog)
        {
            if(options==null || options.Length!=2 || Get("shout").label!="큰소리로 알리기" || Get("wait").label!="다른 사람이 알릴 때까지 기다리기"
                || !Get("shout").isShout || Get("wait").isShout || Get("wait").feedbackCueId!="feedback")
                throw new InvalidOperationException("Action options differ from approved two choices");
            catalog.Get(Get("wait").feedbackCueId).Validate();
        }
    }
}
