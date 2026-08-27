using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using SaintsField.Editor.AutoRunner;
using SaintsField.Editor.Core;
using SaintsField.Editor.Drawers.AdvancedDropdownDrawer;
using UnityEditor;
using UnityEngine;

namespace SaintsField.Editor.Drawers.ShaderDrawers.ShaderParamDrawer
{
#if ODIN_INSPECTOR
    [Sirenix.OdinInspector.Editor.DrawerPriority(Sirenix.OdinInspector.Editor.DrawerPriorityLevel.AttributePriority)]
#endif
    [CustomPropertyDrawer(typeof(ShaderParamAttribute), true)]
    [CustomPropertyDrawer(typeof(ShaderParam), true)]
    public partial class ShaderParamAttributeDrawer: SaintsPropertyDrawer, IAutoRunnerFixDrawer
    {
        private static string GetTypeMismatchError(SerializedProperty property)
        {
            // Shader.PropertyToID values are not stable between runs and must not be serialized.
            if (GetShaderParamNameProperty(property) == null)
            {
                return $"{property.propertyType} is not supported";
            }
            return "";
        }

        private static SerializedProperty GetShaderParamNameProperty(SerializedProperty property)
        {
            if (property.propertyType == SerializedPropertyType.String)
            {
                return property;
            }

            SerializedProperty nameProperty = property.propertyType == SerializedPropertyType.Generic
                ? property.FindPropertyRelative(nameof(ShaderParam.name))
                : null;
            return nameProperty?.propertyType == SerializedPropertyType.String ? nameProperty : null;
        }

        private static (bool foundShaderInfo, ShaderParamUtils.ShaderCustomInfo selectedShaderInfo) GetSelectedShaderInfo(SerializedProperty property, IEnumerable<ShaderParamUtils.ShaderCustomInfo> shaderInfos)
        {
            SerializedProperty nameProperty = GetShaderParamNameProperty(property);
            return GetSelectedShaderInfo(nameProperty?.stringValue, shaderInfos);
        }

        private static (bool foundShaderInfo, ShaderParamUtils.ShaderCustomInfo selectedShaderInfo) GetSelectedShaderInfo(string value, IEnumerable<ShaderParamUtils.ShaderCustomInfo> shaderInfos)
        {
            foreach (ShaderParamUtils.ShaderCustomInfo shaderInfo in shaderInfos)
            {
                if (shaderInfo.PropertyName == value)
                {
                    return (true, shaderInfo);
                }
            }

            return (false, default);
        }

        private static AdvancedDropdownMetaInfo GetMetaInfo(bool foundShaderInfo, ShaderParamUtils.ShaderCustomInfo selectedShaderInfo, IEnumerable<ShaderParamUtils.ShaderCustomInfo> shaderInfos, bool isImGui)
        {
            Dropdown<ShaderParamUtils.ShaderCustomInfo> dropdownListValue =
                new Dropdown<ShaderParamUtils.ShaderCustomInfo>(isImGui? "Shader Parameters": "");

            IReadOnlyList<object> curValues = foundShaderInfo
                ? new[] { (object)selectedShaderInfo }
                : Array.Empty<object>();

            foreach (ShaderParamUtils.ShaderCustomInfo shaderInfo in shaderInfos)
            {
                dropdownListValue.Add(shaderInfo.GetString(true), shaderInfo, false, shaderInfo.GetIcon());
            }

            IReadOnlyList<AdvancedDropdownAttributeDrawer.SelectStack> curSelected;
            if (curValues.Count == 0)
            {
                curSelected = Array.Empty<AdvancedDropdownAttributeDrawer.SelectStack>();
            }
            else
            {
                (IReadOnlyList<AdvancedDropdownAttributeDrawer.SelectStack> stacks, string _) =
                    AdvancedDropdownUtil.GetSelected(curValues[0],
                        Array.Empty<AdvancedDropdownAttributeDrawer.SelectStack>(), dropdownListValue);
                curSelected = stacks;
            }

            return new AdvancedDropdownMetaInfo
            {
                Error = "",
                CurValues = curValues,
                DropdownListValue = dropdownListValue,
                SelectStacks = curSelected,
            };
        }

        public AutoRunnerFixerResult AutoRunFix(PropertyAttribute propertyAttribute, IReadOnlyList<PropertyAttribute> allAttributes,
            SerializedProperty property, MemberInfo memberInfo, object parent)
        {
            string mismatchError = GetTypeMismatchError(property);
            if (mismatchError != "")
            {
                return new AutoRunnerFixerResult
                {
                    Error = mismatchError,
                    ExecError = "",
                };
            }

            ShaderParamAttribute shaderParamAttribute = propertyAttribute as ShaderParamAttribute ?? new ShaderParamAttribute();
            (string error, Shader shader) = ShaderUtils.GetShader(shaderParamAttribute.TargetName, shaderParamAttribute.Index, property, memberInfo, parent);
            if(error != "")
            {
                return new AutoRunnerFixerResult
                {
                    Error = "",
                    ExecError = error,
                };
            }

            ShaderParamUtils.ShaderCustomInfo[] shaderInfos = ShaderParamUtils.GetShaderInfo(shader, shaderParamAttribute.PropertyType).ToArray();
            (bool foundShaderInfo, ShaderParamUtils.ShaderCustomInfo _) = GetSelectedShaderInfo(property, shaderInfos);
            if (!foundShaderInfo)
            {
                SerializedProperty nameProperty = GetShaderParamNameProperty(property);
                return new AutoRunnerFixerResult
                {
                    Error = $"No shader params found for {nameProperty?.stringValue} in {shader.name}",
                    ExecError = "",
                };
            }

            return null;
        }
    }
}
