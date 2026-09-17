using System.Diagnostics;

namespace SaintsField
{
    [Conditional("UNITY_EDITOR")]
    [System.AttributeUsage(System.AttributeTargets.Field | System.AttributeTargets.Property)]
    public class AssetsFolderAttribute: FolderAttribute
    {
        public AssetsFolderAttribute(string folder="Assets", string title="Choose a folder inside assets", string groupBy = "") : base(folder, title, groupBy)
        {
        }
    }

    [System.Obsolete("Use AssetsFolder instead.")]
    [Conditional("UNITY_EDITOR")]
    [System.AttributeUsage(System.AttributeTargets.Field | System.AttributeTargets.Property)]
    public class AssetFolderAttribute: AssetsFolderAttribute
    {
        public AssetFolderAttribute(string folder="Assets", string title="Choose a folder inside assets", string groupBy = "") : base(folder, title, groupBy)
        {
        }
    }
}
