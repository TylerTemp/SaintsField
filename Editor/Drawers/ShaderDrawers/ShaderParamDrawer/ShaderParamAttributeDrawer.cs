using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using SaintsField.Editor.Core;
using SaintsField.Editor.Drawers.AdvancedDropdownDrawer;
using SaintsField.Editor.Utils;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using Object = UnityEngine.Object;

namespace SaintsField.Editor.Drawers.ShaderDrawers.ShaderParamDrawer
{
    [CustomPropertyDrawer(typeof(ShaderParamAttribute))]
    public partial class ShaderParamAttributeDrawer: SaintsPropertyDrawer
    {
        private static (string error, Material material) GetMaterial(string callback, int index, SerializedProperty property, MemberInfo info, object parent)
        {
            if (string.IsNullOrEmpty(callback))  // find on target
            {
                Renderer directRenderer;
                Object obj = property.serializedObject.targetObject;
                switch (obj)
                {
                    case Component comp:
                        directRenderer = comp.GetComponent<Renderer>();
                        break;
                    case GameObject go:
                        directRenderer = go.GetComponent<Renderer>();
                        break;
                    default:
                        return ($"{obj} is not a valid target", null);
                }
                return GetMaterialFromRenderer(directRenderer, index);
            }

            (string error, Object uObj) = Util.GetOf<Object>(callback, null, property, info, parent);
            if (error != "")
            {
#if SAINTSFIELD_DEBUG
                Debug.LogError(error);
#endif
                return (error, null);
            }

            if (Util.IsNull(uObj))
            {
                return ($"Target `{callback}` is null", null);
            }

            // ReSharper disable once ConvertSwitchStatementToSwitchExpression
            switch (uObj)
            {
                case Material material:
                    return ("", material);
                case Renderer renderer:
                    return GetMaterialFromRenderer(renderer, index);
                case Component comp:
                    return GetMaterialFromRenderer(comp.GetComponent<Renderer>(), index);
                case GameObject go:
                    return GetMaterialFromRenderer(go.GetComponent<Renderer>(), index);
                default:
                    return ($"Target `{callback}` is not a valid target: {uObj} ({uObj.GetType()})", null);
            }
        }

        private static (string error, Material material) GetMaterialFromRenderer(Renderer renderer, int index)
        {
            if (renderer == null)
            {
                return ($"No renderer found on target", null);
            }

            Material[] targetMaterials = EditorApplication.isPlaying ? renderer.materials : renderer.sharedMaterials;
            // ReSharper disable once ConvertIfStatementToReturnStatement
            if (index >= targetMaterials.Length)
            {
                return ($"Index {index} out of range ({targetMaterials.Length})", null);
            }
            return ("", targetMaterials[index]);
        }

        private struct ShaderInfo
        {
            public string PropertyName;
            public string PropertyDescription;
            public ShaderPropertyType PropertyType;
            public int PropertyID;

            public override string ToString()
            {
                // Debug.Log($"{PropertyName.Replace("_", "")} -> {PropertyDescription?.Replace("_", "").Replace(" ", "")}");
                string properyName;
                if (string.Equals(PropertyName.Replace("_", ""), PropertyDescription?.Replace("_", "").Replace(" ", ""),
                        StringComparison.CurrentCultureIgnoreCase))
                {
                    properyName = PropertyDescription;
                }
                else if (string.IsNullOrEmpty(PropertyDescription))
                {
                    properyName = PropertyName;
                }
                else
                {
                    properyName = $"{PropertyDescription}: {PropertyName}";
                }
                return $"{properyName} [{PropertyType}]";
            }
        }

        private static IEnumerable<ShaderInfo> GetShaderInfo(Material material, ShaderPropertyType? filterPropertyType)
        {
            Shader shader = material.shader;

            foreach (int index in Enumerable.Range(0, shader.GetPropertyCount()))
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
    }
}
