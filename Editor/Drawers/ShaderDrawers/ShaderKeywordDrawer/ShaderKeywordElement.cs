#if UNITY_2021_2_OR_NEWER
using System;
using SaintsField.Editor.Drawers.AdvancedDropdownDrawer;
using SaintsField.Editor.Drawers.TreeDropdownDrawer;
using SaintsField.Editor.UIToolkitElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace SaintsField.Editor.Drawers.ShaderDrawers.ShaderKeywordDrawer
{
    public class ShaderKeywordElement: StringDropdownElement
    {
        private Shader _shader;
        private VisualElement _boundTarget;
        private HelpBox _helpBox;

        public ShaderKeywordElement()
        {
            Button.clicked += MakeDropdown;
        }

        public void BindShader(Shader shader)
        {
            _shader = shader;
            RefreshDisplay();
        }

        public void BindBound(VisualElement target) => _boundTarget = target;
        public void BindHelpBox(HelpBox helpBox)
        {
            _helpBox = helpBox;
            RefreshDisplay();
        }

        public override void SetValueWithoutNotify(string newValue)
        {
            CachedValue = newValue;
            RefreshDisplay();
        }

        private void RefreshDisplay()
        {
            if (_shader == null)
            {
                if(_helpBox != null)
                {
                    ShaderUtils.UpdateHelpBox(_helpBox, "Shader not found");
                }
            }
            else
            {
                foreach (string shaderKeyword in ShaderKeywordUtils.GetShaderKeywords(_shader))
                {
                    // ReSharper disable once InvertIf
                    if (shaderKeyword == CachedValue)
                    {
                        SetLabelString(shaderKeyword);

                        if(_helpBox != null)
                        {
                            ShaderUtils.UpdateHelpBox(_helpBox, "");
                        }
                        return;
                    }
                }

                string toLabel = "";
                string toError = "";
                if (!string.IsNullOrEmpty(CachedValue))
                {
                    toLabel = $"<color=red>?</color> ({CachedValue})";
                    toError = $"Shader Keyword {CachedValue} not found in {_shader}";
                }

                SetLabelString(toLabel);
                if(_helpBox != null)
                {
                    ShaderUtils.UpdateHelpBox(_helpBox, toError);
                }
            }


        }

        private void MakeDropdown()
        {
            AdvancedDropdownList<string> dropdown = new AdvancedDropdownList<string>();
            dropdown.Add("[Empty String]", string.Empty);
            dropdown.AddSeparator();

            string selected = null;
            foreach (string shaderKeyword in ShaderKeywordUtils.GetShaderKeywords(_shader))
            {
                // dropdown.Add(path, (path, index));
                dropdown.Add(shaderKeyword, shaderKeyword);
                // ReSharper disable once InvertIf
                if (shaderKeyword == value)
                {
                    selected = shaderKeyword;
                }
            }

            AdvancedDropdownMetaInfo metaInfo = new AdvancedDropdownMetaInfo
            {
                CurValues = selected is null ? Array.Empty<object>(): new object[] { selected },
                DropdownListValue = dropdown,
                SelectStacks = Array.Empty<AdvancedDropdownAttributeDrawer.SelectStack>(),
            };

            VisualElement root = _boundTarget ?? this;
            (Rect dropdownWorldBound, float maxHeight) = SaintsAdvancedDropdownUIToolkit.GetProperPos(root.worldBound);

            SaintsTreeDropdownUIToolkit sa = new SaintsTreeDropdownUIToolkit(
                metaInfo,
                root.worldBound.width,
                maxHeight,
                false,
                (curItem, _) =>
                {
                    value = (string)curItem;
                    return new[] { value };
                }
            );

            UnityEditor.PopupWindow.Show(dropdownWorldBound, sa);
        }
    }

    public class ShaderKeywordField : BaseField<string>
    {
        public readonly ShaderKeywordElement ShaderKeywordElement;
        public ShaderKeywordField(string label, ShaderKeywordElement visualInput) : base(label, visualInput)
        {
            visualInput.BindBound(this);
            ShaderKeywordElement = visualInput;
        }
    }
}
#endif
