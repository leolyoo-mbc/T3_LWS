#if UNITY_EDITOR
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace LWS.FireNotification.Validation
{
    public static class ValidationEvidenceChecks
    {
        public static void Run(string parent)
        {
            var type=typeof(FloorButtonChecks).Assembly.GetType("LWS.FireNotification.Validation.ValidationEvidence");
            var allocate=type?.GetMethod("CreateRunDirectory",BindingFlags.Public|BindingFlags.Static);
            if(allocate==null)throw new InvalidOperationException("Missing collision-safe validation evidence allocator");
            parent=(string)allocate.Invoke(null,new object[]{parent,"allocator-checks"});
            string requested=Path.Combine(parent,"repeated-request");
            string first=(string)allocate.Invoke(null,new object[]{requested,"checks"});
            string firstFile=Path.Combine(first,"floor-checks.txt");
            byte[] original={17,0,255,62,8};File.WriteAllBytes(firstFile,original);
            string second=(string)allocate.Invoke(null,new object[]{requested,"checks"});
            if(first==second||!original.SequenceEqual(File.ReadAllBytes(firstFile)))throw new InvalidOperationException("Repeated requested path overwrites earlier evidence");
            File.WriteAllText(Path.Combine(second,"floor-checks.txt"),"second run");
            if(!original.SequenceEqual(File.ReadAllBytes(firstFile)))throw new InvalidOperationException("Second log replaced first run");
            string collision=Path.Combine(parent,"file-collision");File.WriteAllText(collision,"keep file");
            string available=(string)allocate.Invoke(null,new object[]{collision,"checks"});
            if(available==collision||File.ReadAllText(collision)!="keep file")throw new InvalidOperationException("Existing file collision not preserved");
            string defaultFirst=(string)allocate.Invoke(null,new object[]{null,"allocator-default-check"});
            string defaultSecond=(string)allocate.Invoke(null,new object[]{null,"allocator-default-check"});
            if(defaultFirst==defaultSecond)throw new InvalidOperationException("Default evidence path reused");
            bool rejected=false;
            try {allocate.Invoke(null,new object[]{Path.GetFullPath("Assets/ValidationEvidenceMustNotBeCreated"),"checks"});}
            catch(TargetInvocationException e)when(e.InnerException is ArgumentException){rejected=true;}
            if(!rejected)throw new InvalidOperationException("Evidence outside Artifacts must be rejected");
            Debug.Log("Evidence allocator PASS: repeated path, bytes, existing file, scoped root");
        }
    }
}
#endif
