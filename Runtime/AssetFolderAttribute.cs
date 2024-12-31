using System.Diagnostics;

namespace SaintsField
{
    [Conditional("UNITY_EDITOR")]
    [System.AttributeUsage(System.AttributeTargets.Field | System.AttributeTargets.Property, Inherited = true, AllowMultiple = false)]
    public class AssetFolderAttribute: FolderAttribute
    {
        public AssetFolderAttribute(string folder="Assets", string title="Choose a folder inside assets", string groupBy = "") : base(folder, title, groupBy)
        {
        }
    }
}
