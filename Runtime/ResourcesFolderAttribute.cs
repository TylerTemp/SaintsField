using System.Diagnostics;

namespace SaintsField
{
    [Conditional("UNITY_EDITOR")]
    [System.AttributeUsage(System.AttributeTargets.Field | System.AttributeTargets.Property, Inherited = true, AllowMultiple = false)]
    public class ResourcesFolderAttribute: FolderAttribute
    {
        public ResourcesFolderAttribute(string folder="", string title="Choose a folder inside resources", string groupBy = "") : base(folder, title, groupBy)
        {
        }
    }

    [System.Obsolete("Use ResourcesFolder instead.")]
    [Conditional("UNITY_EDITOR")]
    [System.AttributeUsage(System.AttributeTargets.Field | System.AttributeTargets.Property, Inherited = true, AllowMultiple = false)]
    public class ResourceFolderAttribute: ResourcesFolderAttribute
    {
        public ResourceFolderAttribute(string folder="", string title="Choose a folder inside resources", string groupBy = "") : base(folder, title, groupBy)
        {
        }
    }
}
