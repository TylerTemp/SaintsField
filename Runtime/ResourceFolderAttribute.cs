using System.Diagnostics;

namespace SaintsField
{
    [Conditional("UNITY_EDITOR")]
    [System.AttributeUsage(System.AttributeTargets.Field | System.AttributeTargets.Property, Inherited = true, AllowMultiple = false)]
    public class ResourceFolderAttribute: FolderAttribute
    {
        public ResourceFolderAttribute(string folder="", string title="Choose a folder inside resources", string groupBy = "") : base(folder, title, groupBy)
        {
        }
    }
}
