using System;
using System.Collections.Generic;
using System.Reflection;
using SaintsField.Editor.AutoRunner;
using SaintsField.Editor.Core;
using SaintsField.Editor.Utils;
using UnityEditor;
using UnityEngine;

namespace SaintsField.Editor.Drawers.ValidateInputDrawer
{
#if ODIN_INSPECTOR
    [Sirenix.OdinInspector.Editor.DrawerPriority(Sirenix.OdinInspector.Editor.DrawerPriorityLevel.SuperPriority)]
#endif
    [CustomPropertyDrawer(typeof(ValidateInputAttribute), true)]
    public partial class ValidateInputAttributeDrawer : SaintsPropertyDrawer, IAutoRunnerFixDrawer
    {
        private static string CallValidateMethod(string callback, string label, SerializedProperty property, MemberInfo fieldInfo, object parent)
        {
            (string error, object validateResult) = Util.GetMethodOf<object>(callback, null, property, fieldInfo, parent);
            // Debug.Log($"parent {parent}, call {callback} get {validateResult}, error={error}");
            if(error != "")
            {
                return error;
            }

            // ReSharper disable once ConvertSwitchStatementToSwitchExpression
            switch (validateResult)
            {
                case bool boolValue:
                    return boolValue? "" : $"`{label}` is invalid";
                case string stringContent:
                    return stringContent;
                case null:
                    return "";
                default:
                    throw new ArgumentOutOfRangeException(nameof(validateResult), validateResult, null);
            }
        }

        public AutoRunnerFixerResult AutoRunFix(PropertyAttribute propertyAttribute, IReadOnlyList<PropertyAttribute> allAttributes,
            SerializedProperty property, MemberInfo memberInfo, object parent)
        {
            if (property.isArray)
            {
                return null;
            }

            string callback = ((ValidateInputAttribute) propertyAttribute).Callback;
            string validateResult = CallValidateMethod(callback, property.displayName, property, memberInfo, parent);
            return string.IsNullOrEmpty(validateResult) ? null : new AutoRunnerFixerResult
            {
                ExecError = "",
                Error = validateResult,
            };
        }
    }
}
