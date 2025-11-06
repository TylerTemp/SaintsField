#if UNITY_2021_2_OR_NEWER
using System;
using System.Collections.Generic;
using SaintsField.Editor.Drawers.AdvancedDropdownDrawer;
using SaintsField.Editor.Drawers.TreeDropdownDrawer;
using SaintsField.Editor.Utils;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UIElements;

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

        public static void MakeDropdown<T>(T curValue, Shader shader, ShaderPropertyType? shaderPropertyType, VisualElement root, Action<T> onValueChangedCallback)
        {
            bool isString = typeof(T) == typeof(string);

            AdvancedDropdownList<ShaderCustomInfo> dropdown = new AdvancedDropdownList<ShaderCustomInfo>();
            if (isString)
            {
                dropdown.Add("[Empty String]", new ShaderCustomInfo("", "", default, -1));
                dropdown.AddSeparator();
            }

            bool selected = false;
            ShaderCustomInfo selectedInfo = default;
            foreach (ShaderCustomInfo shaderCustomInfo in GetShaderInfo(shader, shaderPropertyType))
            {
                // dropdown.Add(path, (path, index));
                dropdown.Add(shaderCustomInfo.GetString(false), shaderCustomInfo);
                // ReSharper disable once InvertIf
                if (isString && shaderCustomInfo.PropertyName == (string)(object)curValue
                    || !isString && shaderCustomInfo.PropertyID == (int)(object)curValue)
                {
                    selected = true;
                    selectedInfo = shaderCustomInfo;
                }
            }

            AdvancedDropdownMetaInfo metaInfo = new AdvancedDropdownMetaInfo
            {
                CurValues = selected ? new object[] { selectedInfo } : Array.Empty<object>(),
                DropdownListValue = dropdown,
                SelectStacks = Array.Empty<AdvancedDropdownAttributeDrawer.SelectStack>(),
            };

            (Rect worldBound, float maxHeight) = SaintsAdvancedDropdownUIToolkit.GetProperPos(root.worldBound);

            SaintsTreeDropdownUIToolkit sa = new SaintsTreeDropdownUIToolkit(
                metaInfo,
                root.worldBound.width,
                maxHeight,
                false,
                (curItem, _) =>
                {
                    ShaderCustomInfo shaderCustomInfo = (ShaderCustomInfo)curItem;
                    if (isString)
                    {
                        onValueChangedCallback.Invoke((T)(object)shaderCustomInfo.PropertyName);
                    }
                    else
                    {
                        onValueChangedCallback.Invoke((T)(object)shaderCustomInfo.PropertyID);
                    }

                    return new[] { curItem };
                }
            );

            UnityEditor.PopupWindow.Show(worldBound, sa);
        }
    }
}
#endif
