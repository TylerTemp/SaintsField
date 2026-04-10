using System.Collections.Generic;
using System.IO;
using System.Reflection;
using SaintsField.Editor.AutoRunner;
using UnityEditor;
using UnityEngine;

namespace SaintsField.Editor.Drawers.FolderDrawers.AssetsFolderDrawer
{
#if ODIN_INSPECTOR
    [Sirenix.OdinInspector.Editor.DrawerPriority(Sirenix.OdinInspector.Editor.DrawerPriorityLevel.WrapperPriority)]
#endif
    [CustomPropertyDrawer(typeof(AssetFolderAttribute), true)]
    public partial class AssetFolderAttributeDrawer: FolderDrawerBase, IAutoRunnerFixDrawer
    {
        protected override (string error, string actualFolder) ValidateFullFolder(string folderValue)
        {
            return GetAssetsPath(folderValue);
        }

        protected override string WrapFolderToOpen(string folder)
        {
            return folder;
        }

        public AutoRunnerFixerResult AutoRunFix(PropertyAttribute propertyAttribute, IReadOnlyList<PropertyAttribute> allAttributes,
            SerializedProperty property, MemberInfo memberInfo, object parent)
        {
            if (property.propertyType != SerializedPropertyType.String)
            {
                return new AutoRunnerFixerResult
                {
                    Error = $"{property.propertyPath}({property.propertyType}) is not string type",
                    ExecError = "",
                    CanFix = false,
                    Callback = null,
                };
            }
            string err = Directory.Exists(property.stringValue)? "": $"Folder \"{property.stringValue}\" does not exists";
            return new AutoRunnerFixerResult
            {
                Error = err,
                ExecError = "",
                CanFix = false,
                Callback = null,
            };
        }
    }
}
