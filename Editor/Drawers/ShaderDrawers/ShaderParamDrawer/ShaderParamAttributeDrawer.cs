#if UNITY_2021_2_OR_NEWER
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using SaintsField.Editor.AutoRunner;
using SaintsField.Editor.Core;
using SaintsField.Editor.Drawers.AdvancedDropdownDrawer;
using SaintsField.Editor.Utils;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using Object = UnityEngine.Object;

namespace SaintsField.Editor.Drawers.ShaderDrawers.ShaderParamDrawer
{
#if ODIN_INSPECTOR
    [Sirenix.OdinInspector.Editor.DrawerPriority(Sirenix.OdinInspector.Editor.DrawerPriorityLevel.SuperPriority)]
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



        private struct ShaderInfo
        {
            public string PropertyName;
            public string PropertyDescription;
            public ShaderPropertyType PropertyType;
            public int PropertyID;

            public override string ToString()
            {
                // Debug.Log(ObjectNames.NicifyVariableName("_dstA"));
                // Debug.Log($"{PropertyName.Replace("_", "")} -> {PropertyDescription?.Replace("_", "").Replace(" ", "")}");
                string properyName;
                if (string.Equals(PropertyName.Replace("_", ""), PropertyDescription?.Replace("_", "").Replace(" ", ""),
                        StringComparison.CurrentCultureIgnoreCase))
                {
                    properyName = ObjectNames.NicifyVariableName(PropertyDescription);
                }
                else if (string.IsNullOrEmpty(PropertyDescription))
                {
                    properyName = PropertyName;
                }
                else
                {
                    properyName = $"{ObjectNames.NicifyVariableName(PropertyDescription)}: {PropertyName}";
                }
                return $"{properyName} [{PropertyType}]";
            }
        }

        private static IEnumerable<ShaderInfo> GetShaderInfo(Shader shader, ShaderPropertyType? filterPropertyType)
        {
            for (int index = 0; index < shader.GetPropertyCount(); index++)
            {
                string propertyName = shader.GetPropertyName(index);
                string propertyDescription = shader.GetPropertyDescription(index);
                ShaderPropertyType propertyType = shader.GetPropertyType(index);

                if(filterPropertyType != null && propertyType != (ShaderPropertyType)filterPropertyType)
                {
                    continue;
                }

#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_SHADER_PARAM
                Debug.Log($"#ShaderParam# Property Name: {propertyName}, Property Type: {propertyType}");
#endif
                yield return new ShaderInfo
                {
                    PropertyName = propertyName,
                    PropertyDescription = propertyDescription,
                    PropertyType = propertyType,
                    PropertyID = Shader.PropertyToID(propertyName),
                };
            }
        }

        private static (bool foundShaderInfo, ShaderInfo selectedShaderInfo) GetSelectedShaderInfo(SerializedProperty property, IEnumerable<ShaderInfo> shaderInfos)
        {
            foreach (ShaderInfo shaderInfo in shaderInfos)
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

        private static AdvancedDropdownMetaInfo GetMetaInfo(bool foundShaderInfo, ShaderInfo selectedShaderInfo, IEnumerable<ShaderInfo> shaderInfos, bool isImGui)
        {
            AdvancedDropdownList<ShaderInfo> dropdownListValue =
                new AdvancedDropdownList<ShaderInfo>(isImGui? "Shader Parameters": "");

            IReadOnlyList<object> curValues = foundShaderInfo
                ? new[] { (object)selectedShaderInfo }
                : Array.Empty<object>();

            foreach (ShaderInfo shaderInfo in shaderInfos)
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

            ShaderInfo[] shaderInfos = GetShaderInfo(shader, shaderParamAttribute.PropertyType).ToArray();
            (bool foundShaderInfo, ShaderInfo _) = GetSelectedShaderInfo(property, shaderInfos);
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
