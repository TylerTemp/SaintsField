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
#if SAINTSFIELD_I2_LOC
using I2.Loc;
#endif

namespace SaintsField.Editor.Drawers.RequiredDrawer
{
#if ODIN_INSPECTOR
    [Sirenix.OdinInspector.Editor.DrawerPriority(Sirenix.OdinInspector.Editor.DrawerPriorityLevel.WrapperPriority)]
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
            if (!ReflectUtils.Truly(curValue))
            {
                return $"Target `{curValue}` is not a truly value";
            }

#if SAINTSFIELD_ADDRESSABLE && !SAINTSFIELD_ADDRESSABLE_DISABLE
            // ReSharper disable once ConvertIfStatementToSwitchStatement
            if (curValue is AssetReference ar)
            {
                return ValidateAddresable(ar);
            }
#endif
#if SAINTSFIELD_I2_LOC
            if(curValue is LocalizedString localString)
            {
                return ValidateLocalizedString(localString);
            }
#endif

            return "";

        }

#if SAINTSFIELD_ADDRESSABLE && !SAINTSFIELD_ADDRESSABLE_DISABLE
        private static string ValidateAddresable(AssetReference ar)
        {
            // ReSharper disable once ConvertIfStatementToReturnStatement
            if (ar.editorAsset)
            {
                return "";

            }

            return "AssetReference is null";
        }
#endif
#if SAINTSFIELD_I2_LOC
        private static string ValidateLocalizedString(LocalizedString localizedString) =>
            string.IsNullOrEmpty(localizedString.mTerm)
                ? "LocalizedString is null"
                : "";
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
#if SAINTSFIELD_I2_LOC
                if(fieldType == typeof(LocalizedString))
                {
                    return "";
                }
#endif
                return $"`{property.displayName}` can not be a valued type: {fieldType}";
            }

            return "";
        }
    }
}
