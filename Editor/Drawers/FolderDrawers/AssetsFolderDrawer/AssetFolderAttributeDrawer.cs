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
            string value = property.stringValue;

            string error = "";
            if (!string.IsNullOrEmpty(value) && !Directory.Exists(value))
            {
                error = $"Folder \"{value}\" does not exists";
            }

            return new AutoRunnerFixerResult
            {
                Error = error,
                ExecError = "",
                CanFix = false,
                Callback = null,
            };
        }
    }
}
