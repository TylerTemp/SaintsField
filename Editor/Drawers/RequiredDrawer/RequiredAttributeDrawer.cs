using System;
using System.Reflection;
using SaintsField.Editor.AutoRunner;
using SaintsField.Editor.Core;
using SaintsField.Editor.Utils;
using UnityEditor;

namespace SaintsField.Editor.Drawers.RequiredDrawer
{
    [CustomPropertyDrawer(typeof(RequiredAttribute))]
    public partial class RequiredAttributeDrawer: SaintsPropertyDrawer, IAutoRunnerDrawer
    {
        private static (string error, bool result) Truly(SerializedProperty property, MemberInfo field, object target)
        {
            (string curError, int _, object curValue) = Util.GetValue(property, field, target);
            return curError != ""
                ? (curError, false)
                : ("", ReflectUtils.Truly(curValue));
        }

        public AutoRunnerFixerResult AutoRun(SerializedProperty property, MemberInfo memberInfo, object parent)
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

            if (ReflectUtils.Truly(curValue))
            {
                return null;
            }

            return new AutoRunnerFixerResult
            {
                CanFix = false,
                Error = $"{property.displayName} is required ({property.propertyPath})",
                ExecError = "",
            };
        }

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
