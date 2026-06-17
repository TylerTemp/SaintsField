#if UNITY_2021_3_OR_NEWER
using SaintsField.Editor.Core;
using SaintsField.Editor.UIToolkitElements;
using SaintsField.Editor.Utils;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UIElements;

namespace SaintsField.Editor.Drawers.ShaderDrawers.ShaderParamDrawer
{
    public class ShaderParamIntElement: IntDropdownElement, IBindShader
    {
        private Shader _shader;
        private readonly ShaderPropertyType? _filterPropertyType;

        private VisualElement _boundTarget;
        private HelpBox _helpBox;

        public ShaderParamIntElement(ShaderPropertyType? filterPropertyType)
        {
            _filterPropertyType = filterPropertyType;
            Button.clicked += () => ShaderParamUtils.MakeDropdown(value, _shader, filterPropertyType, _boundTarget ?? this, v => value = v);
        }

        public void BindBound(VisualElement target) => _boundTarget = target;
        public void BindHelpBox(HelpBox helpBox)
        {
            _helpBox = helpBox;
            RefreshDisplay();
        }

        public void BindShader(Shader shader)
        {
            // ReSharper disable once InvertIf
            if (_shader != shader)
            {
                _shader = shader;
                RefreshDisplay();
            }
        }

        public override void SetValueWithoutNotify(int newValue)
        {
            CachedValue = newValue;
            RefreshDisplay();
        }

        private readonly RichTextDrawer _richTextDrawer = new RichTextDrawer();

        private void RefreshDisplay()
        {
            if (_shader == null)
            {
                if(_helpBox != null)
                {
                    ShaderUtils.UpdateHelpBox(_helpBox, "Shader not found");
                    if(_helpBox != null)
                    {
                        ShaderUtils.UpdateHelpBox(_helpBox, "");
                    }
                }
            }
            else
            {
                foreach (ShaderParamUtils.ShaderCustomInfo r in ShaderParamUtils.GetShaderInfo(_shader, _filterPropertyType))
                {
                    // ReSharper disable once InvertIf
                    if (r.PropertyID == CachedValue)
                    {
                        UIToolkitUtils.SetLabel(Label, r.GetDisplayChunks(false), _richTextDrawer);
                        return;
                    }
                }
            }

            string wrongLabel = $"<color=red>?</color> {(CachedValue == null? "": $"({CachedValue})")}";
            UIToolkitUtils.SetLabel(Label, new[]{new RichTextDrawer.RichTextChunk(wrongLabel, false, wrongLabel)}, _richTextDrawer);

            if(_helpBox != null)
            {
                ShaderUtils.UpdateHelpBox(_helpBox, CachedValue == null? "": $"Shader Param {CachedValue} not found in {_shader}");
            }
        }
    }

    public class ShaderParamIntField: BaseField<int>
    {
        public readonly ShaderParamIntElement ShaderParamIntElement;
        public ShaderParamIntField(string label, ShaderParamIntElement visualInput) : base(label, visualInput)
        {
            style.flexShrink = 1;
            ShaderParamIntElement = visualInput;
            visualInput.BindBound(this);
        }
    }
}
#endif
