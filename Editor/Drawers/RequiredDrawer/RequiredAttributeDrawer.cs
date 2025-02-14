using System;
using System.Collections.Generic;
using System.Reflection;
using SaintsField.Editor.AutoRunner;
using SaintsField.Editor.Core;
using SaintsField.Editor.Utils;
using UnityEditor;
using UnityEngine;
#if SAINTSFIELD_ADDRESSABLE && !SAINTSFIELD_ADDRESSABLE_DISABLE
using UnityEngine.AddressableAssets;
#endif

namespace SaintsField.Editor.Drawers.RequiredDrawer
{
#if ODIN_INSPECTOR
    [Sirenix.OdinInspector.Editor.DrawerPriority(Sirenix.OdinInspector.Editor.DrawerPriorityLevel.SuperPriority)]
#endif
    [CustomPropertyDrawer(typeof(RequiredAttribute), true)]
    public partial class RequiredAttributeDrawer: SaintsPropertyDrawer, IAutoRunnerFixDrawer
    {
        private static (string error, bool result) Truly(SerializedProperty property, MemberInfo field, object target)
        {
            (string curError, int _, object curValue) = Util.GetValue(property, field, target);
            return curError != ""
                ? (curError, false)
                : ("", string.IsNullOrEmpty(ValidateValue(curValue)));
        }

        public AutoRunnerFixerResult AutoRunFix(PropertyAttribute propertyAttribute,
            IReadOnlyList<PropertyAttribute> allAttributes,
            SerializedProperty property, MemberInfo memberInfo, object parent)
        {
            (string curError, int _, object curValue) = Util.GetValue(property, memberInfo, parent);
            if (curError != "")
            {
                return new AutoRunnerFixerResult
                {
                    CanFix = false,
                    Error = "",
                    ExecError = curError,
                };
            }

            string validateInfo = ValidateValue(curValue);
            if (string.IsNullOrEmpty(validateInfo))
            {
                return null;
            }

            return new AutoRunnerFixerResult
            {
                CanFix = false,
                Error = $"{property.displayName}({property.propertyPath}): {validateInfo}",
                ExecError = "",
            };
        }

        private static string ValidateValue(object curValue)
        {
            if (ReflectUtils.Truly(curValue))
            {
#if SAINTSFIELD_ADDRESSABLE && !SAINTSFIELD_ADDRESSABLE_DISABLE
                if (curValue is AssetReference ar)
                {
                    return ValidateAddresable(ar);
                }
#endif

                return "";
            }



            return $"Target `{curValue}` is not a truly value";
        }

#if SAINTSFIELD_ADDRESSABLE && !SAINTSFIELD_ADDRESSABLE_DISABLE
        private static string ValidateAddresable(AssetReference ar)
        {
            if (ar.editorAsset == null)
            {
                return $"AssetReference is null";
            }

            return "";
        }
#endif

        private struct MetaInfo
        {
            public bool TypeError;
            // public bool IsTruly;
        }


        private static string ValidateType(SerializedProperty property, Type fieldType)
        {
            // if (property.propertyType == SerializedPropertyType.Integer)
            // {
            //     return $"`{property.displayName}` can not be a valued type: int";
            // }
            // if (property.propertyType == SerializedPropertyType.Float)
            // {
            //     return $"`{property.displayName}` can not be a valued type: float";
            // }

            // ReSharper disable once ConvertIfStatementToReturnStatement
            // Type curType = SerializedUtils.GetType(property);
            // an array, list, struct or class && not struct
            if (property.propertyType == SerializedPropertyType.Generic && fieldType.IsValueType)
            {
                return $"`{property.displayName}` can not be a valued type: {fieldType}";
            }

            return "";
        }
    }
}
