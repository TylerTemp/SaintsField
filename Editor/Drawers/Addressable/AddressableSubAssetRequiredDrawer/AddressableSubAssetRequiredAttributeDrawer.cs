using System.Collections.Generic;
using System.Reflection;
using SaintsField.Addressable;
using SaintsField.Editor.AutoRunner;
using SaintsField.Editor.Core;
using UnityEditor;
using UnityEngine;

namespace SaintsField.Editor.Drawers.Addressable.AddressableSubAssetRequiredDrawer
{
#if ODIN_INSPECTOR
    [Sirenix.OdinInspector.Editor.DrawerPriority(Sirenix.OdinInspector.Editor.DrawerPriorityLevel.WrapperPriority)]
#endif
    [CustomPropertyDrawer(typeof(AddressableSubAssetRequiredAttribute))]
    public partial class AddressableSubAssetRequiredAttributeDrawer: SaintsPropertyDrawer, IAutoRunnerFixDrawer
    {
        private const string MSubObjectName = "m_SubObjectName";

        private static (string error, string result) ValidateProperty(SerializedProperty property)
        {
            SerializedProperty sp = property.FindPropertyRelative(MSubObjectName);
            if (sp == null)
            {
                return ("No sub object field found", "");
            }

            string subOjectName = sp.stringValue;
            // Debug.Log(subOjectName);
            if (string.IsNullOrEmpty(subOjectName))
            {
                return ("", "subAsset name is empty");
            }

            // string path = AssetDatabase.GUIDToAssetPath(subOjectName);
            // if (string.IsNullOrEmpty(path))
            // {
            //     return ("", $"subAsset {subOjectName} path not found");
            // }

            // Object subObj = AssetDatabase.LoadAssetAtPath<Object>(path);
            // return ("", ReflectUtils.Truly(subObj)? "": $"subAsset {subObj} is null");
            return ("", "");
        }

        public AutoRunnerFixerResult AutoRunFix(PropertyAttribute propertyAttribute,
            IReadOnlyList<PropertyAttribute> allAttributes,
            SerializedProperty property, MemberInfo memberInfo, object parent)
        {

            (string error, string result) = ValidateProperty(property);
            if (error != "")
            {
                return new AutoRunnerFixerResult
                {
                    CanFix = false,
                    Error = "",
                    ExecError = error,
                };
            }

            if (string.IsNullOrEmpty(result))
            {
                return null;
            }

            return new AutoRunnerFixerResult
            {
                CanFix = false,
                Error = $"{property.displayName}({property.propertyPath}): {result}",
                ExecError = "",
            };
        }
    }
}
