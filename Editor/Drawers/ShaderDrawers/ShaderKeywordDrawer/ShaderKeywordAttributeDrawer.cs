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
using UnityEngine.Rendering;

namespace SaintsField.Editor.Drawers.ShaderDrawers.ShaderKeywordDrawer
{
#if ODIN_INSPECTOR
    [Sirenix.OdinInspector.Editor.DrawerPriority(Sirenix.OdinInspector.Editor.DrawerPriorityLevel.SuperPriority)]
#endif
    [CustomPropertyDrawer(typeof(ShaderKeywordAttribute), true)]
    public partial class ShaderKeywordAttributeDrawer: SaintsPropertyDrawer, IAutoRunnerFixDrawer
    {
        private static string GetTypeMismatchError(SerializedProperty property)
        {
            return property.propertyType != SerializedPropertyType.String
                ? $"{property.propertyType} is not supported"
                : "";
        }

        private static AdvancedDropdownMetaInfo GetMetaInfo(int selectedIndex, IReadOnlyList<string> shaderKeywords, bool isImGui)
        {
            AdvancedDropdownList<string> dropdownListValue =
                new AdvancedDropdownList<string>(isImGui? "Shader Keywords": "");

            IReadOnlyList<object> curValues = selectedIndex >= 0
                ? new[] { (object)shaderKeywords[selectedIndex] }
                : Array.Empty<object>();

            foreach (string shaderKeyword in shaderKeywords)
            {
                dropdownListValue.Add(shaderKeyword, shaderKeyword);
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

        private static IEnumerable<string> GetShaderKeywords(Shader shader)
        {
            LocalKeywordSpace keywordSpace = shader.keywordSpace;

            foreach (LocalKeyword localKeyword in keywordSpace.keywords)
            {
                yield return localKeyword.name;
            }
        }

        public AutoRunnerFixerResult AutoRunFix(PropertyAttribute propertyAttribute, IReadOnlyList<PropertyAttribute> allAttributes,
            SerializedProperty property, MemberInfo memberInfo, object parent)
        {
            string mismatchError = GetTypeMismatchError(property);
            if (mismatchError != "")
            {
                return new AutoRunnerFixerResult
                {
                    ExecError = "",
                    Error = mismatchError,
                };
            }

            ShaderKeywordAttribute shaderKeywordAttribute = (ShaderKeywordAttribute) propertyAttribute;
            (string error, Shader shader) = ShaderUtils.GetShader(shaderKeywordAttribute.TargetName, shaderKeywordAttribute.Index, property, memberInfo, parent);
            if(error != "")
            {
                return new AutoRunnerFixerResult
                {
                    ExecError = error,
                    Error = "",
                };
            }

            string selectedShaderKeyword = property.stringValue;

            if (GetShaderKeywords(shader).Any(shaderKeyword => selectedShaderKeyword == shaderKeyword))
            {
                return null;
            }

            return new AutoRunnerFixerResult
            {
                ExecError = "",
                Error = $"Shader keyword `{selectedShaderKeyword}` not found in shader {shader.name}",
            };

        }
    }
}
#endif
