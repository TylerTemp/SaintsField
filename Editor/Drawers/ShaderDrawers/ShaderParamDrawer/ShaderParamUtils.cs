#if UNITY_2021_2_OR_NEWER
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace SaintsField.Editor.Drawers.ShaderDrawers.ShaderParamDrawer
{
    public static class ShaderParamUtils
    {
        public readonly struct ShaderCustomInfo: IEquatable<ShaderCustomInfo>
        {
            public readonly string PropertyName;
            public readonly string PropertyDescription;
            public readonly ShaderPropertyType PropertyType;
            public readonly int PropertyID;

            public ShaderCustomInfo(string propertyName, string propertyDescription, ShaderPropertyType propertyType, int propertyID)
            {
                PropertyName = propertyName;
                PropertyDescription = propertyDescription;
                PropertyType = propertyType;
                PropertyID = propertyID;
            }

            public override string ToString() => GetString(true);

            public string GetString(bool imGui)
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
                    properyName = ObjectNames.NicifyVariableName(PropertyDescription) + (
                            imGui? $": {PropertyName}": $" <color=#808080>{PropertyName}</color>"
                        );
                }
                return properyName + (imGui? $"[{PropertyType}]": $" <color=#808080>{PropertyType}</color>");
            }

            public bool Equals(ShaderCustomInfo other)
            {
                return PropertyID == other.PropertyID;
            }

            public override bool Equals(object obj)
            {
                return obj is ShaderCustomInfo other && Equals(other);
            }

            public override int GetHashCode()
            {
                return PropertyID;
            }
        }

        public static IEnumerable<ShaderCustomInfo> GetShaderInfo(Shader shader, ShaderPropertyType? filterPropertyType)
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
                yield return new ShaderCustomInfo(propertyName, propertyDescription, propertyType,
                    Shader.PropertyToID(propertyName));
            }
        }
    }
}
#endif
