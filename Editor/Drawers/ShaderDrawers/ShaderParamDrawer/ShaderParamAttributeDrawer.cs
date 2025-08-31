#if UNITY_2021_2_OR_NEWER
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
    public partial class ShaderParamAttributeDrawer: SaintsPropertyDrawer, IAutoRunnerFixDrawer
    {
        private static string GetTypeMismatchError(SerializedProperty property)
        {
            if(property.propertyType != SerializedPropertyType.String &&
               property.propertyType != SerializedPropertyType.Integer)
            {
                return $"{property.propertyType} is not supported";
            }
            return "";
        }

        private static (bool foundShaderInfo, ShaderParamUtils.ShaderCustomInfo selectedShaderInfo) GetSelectedShaderInfo(SerializedProperty property, IEnumerable<ShaderParamUtils.ShaderCustomInfo> shaderInfos)
        {
            foreach (ShaderParamUtils.ShaderCustomInfo shaderInfo in shaderInfos)
            {
                // ReSharper disable once ConvertIfStatementToSwitchStatement
                if (property.propertyType == SerializedPropertyType.String &&
                    shaderInfo.PropertyName == property.stringValue)
                {
                    return (true, shaderInfo);

                }
                if(property.propertyType == SerializedPropertyType.Integer &&
                        shaderInfo.PropertyID == property.intValue)
                {
                    return (true, shaderInfo);
                }
            }

            return (false, default);
        }

        private static AdvancedDropdownMetaInfo GetMetaInfo(bool foundShaderInfo, ShaderParamUtils.ShaderCustomInfo selectedShaderInfo, IEnumerable<ShaderParamUtils.ShaderCustomInfo> shaderInfos, bool isImGui)
        {
            AdvancedDropdownList<ShaderParamUtils.ShaderCustomInfo> dropdownListValue =
                new AdvancedDropdownList<ShaderParamUtils.ShaderCustomInfo>(isImGui? "Shader Parameters": "");

            IReadOnlyList<object> curValues = foundShaderInfo
                ? new[] { (object)selectedShaderInfo }
                : Array.Empty<object>();

            foreach (ShaderParamUtils.ShaderCustomInfo shaderInfo in shaderInfos)
            {
                dropdownListValue.Add(shaderInfo.ToString(), shaderInfo);
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

            ShaderParamAttribute shaderParamAttribute = (ShaderParamAttribute)propertyAttribute;
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
                return new AutoRunnerFixerResult
                {
                    Error =
                        $"No shader params found for {(property.propertyType == SerializedPropertyType.String ? property.stringValue : property.intValue.ToString())} in {shader.name}",
                    ExecError = "",
                };
            }

            return null;
        }
    }
}
#endif
