#if UNITY_EDITOR
using System;
using System.IO;

namespace LWS.FireNotification.Validation
{
    public static class ValidationEvidence
    {
        public static string CreateRunDirectory(string requested,string label)
        {
            string root=Path.GetFullPath("Artifacts/Validation/VisualEvidence");
            string path=Path.GetFullPath(string.IsNullOrWhiteSpace(requested)?
                Path.Combine(root,DateTime.Now.ToString("yyyyMMdd-HHmmss")+"-"+label):requested);
            if(!path.StartsWith(root+Path.DirectorySeparatorChar,StringComparison.OrdinalIgnoreCase))
                throw new ArgumentException("Validation evidence must stay under Artifacts/Validation/VisualEvidence");
            string available=path;int suffix=0;
            while(Directory.Exists(available)||File.Exists(available))available=path+"-"+(++suffix).ToString("000");
            Directory.CreateDirectory(available);
            return available;
        }
    }
}
#endif
