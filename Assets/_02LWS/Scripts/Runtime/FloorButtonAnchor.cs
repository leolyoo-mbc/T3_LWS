using System;
using System.Collections.Generic;
using UnityEngine;

namespace LWS.FireNotification.Runtime
{
    [DisallowMultipleComponent]
    public sealed class FloorButtonAnchor : MonoBehaviour
    {
        public string id;
        [Min(0)] public float raisedBottomHeight=.60f, floorClearance=.02f;
        [Min(.01f)] public float singleWidth=.66f, twoTotalWidth=1.35f, gap=.02f, minimumHeight=.24f;
        [Min(.01f)] public float riseSeconds=.35f, lowerSeconds=.25f;
        public string[] ConfigurationIssues()
        {
            var errors=new List<string>();
            if(string.IsNullOrWhiteSpace(id))errors.Add("Empty floor anchor id");
            float[] values={raisedBottomHeight,floorClearance,singleWidth,twoTotalWidth,gap,minimumHeight,riseSeconds,lowerSeconds};
            foreach(float v in values)if(float.IsNaN(v)||float.IsInfinity(v)){errors.Add("Non-finite floor anchor value");break;}
            if(raisedBottomHeight<0||floorClearance<0||singleWidth<=0||twoTotalWidth<=0||minimumHeight<=0||gap<0||gap>=twoTotalWidth||riseSeconds<=0||lowerSeconds<=0)errors.Add("Invalid floor anchor dimensions or duration");
            if((transform.lossyScale-Vector3.one).sqrMagnitude>.000001f)errors.Add("Floor anchor scale must be one");
            var nav=FindFirstObjectByType<ContentSceneNavigator>(FindObjectsInactive.Include);
            if(nav!=null&&nav.origin!=null&&transform.IsChildOf(nav.origin.transform))errors.Add("Floor anchor cannot follow XR origin/head/controllers");
            if(nav!=null)foreach(var root in gameObject.scene.GetRootGameObjects())foreach(var a in root.GetComponentsInChildren<FloorButtonAnchor>(true))
                if(a!=this&&a.id==id){errors.Add("Duplicate floor anchor id: "+id);return errors.ToArray();}
            return errors.ToArray();
        }
        private void OnDrawGizmos()
        {
            Gizmos.color=Color.cyan;
            Gizmos.DrawLine(transform.position,transform.position+Vector3.up*raisedBottomHeight);
            Gizmos.matrix=Matrix4x4.TRS(transform.position+Vector3.up*raisedBottomHeight,transform.rotation,Vector3.one);
            Gizmos.DrawWireCube(new Vector3(0,minimumHeight*.5f,0),new Vector3(twoTotalWidth,minimumHeight,.01f));
            Gizmos.DrawRay(Vector3.zero,Vector3.back*.4f);
        }
    }
}
