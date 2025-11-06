#if UNITY_2021_3_OR_NEWER
using SaintsField.Editor.UIToolkitElements;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UIElements;

namespace SaintsField.Editor.Drawers.ShaderDrawers.ShaderParamDrawer
{
    public class ShaderParamStringElement: StringDropdownElement, IBindShader
    {
        private Shader _shader;
        private readonly ShaderPropertyType? _filterPropertyType;

        private VisualElement _boundTarget;
        private HelpBox _helpBox;

        public ShaderParamStringElement(ShaderPropertyType? filterPropertyType)
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
                foreach (ShaderParamUtils.ShaderCustomInfo r in ShaderParamUtils.GetShaderInfo(_shader,
                             _filterPropertyType))
                {
                    // ReSharper disable once InvertIf
                    if (r.PropertyName == CachedValue)
                    {
                        SetLabelString(r.GetString(false));
                        if(_helpBox != null)
                        {
                            ShaderUtils.UpdateHelpBox(_helpBox, "");
                        }
                        return;
                    }
                }
            }

            string toError = "";
            if (string.IsNullOrEmpty(CachedValue))
            {
                SetLabelString("");
            }
            else
            {
                SetLabelString($"<color=red>?</color> {CachedValue}");
                toError = $"Shader Param {CachedValue} not found in {_shader}";
            }
            if(_helpBox != null)
            {
                ShaderUtils.UpdateHelpBox(_helpBox, toError);
            }
        }
    }

    public class ShaderParamStringField : BaseField<string>
    {
        public readonly ShaderParamStringElement ShaderParamStringElement;

        public ShaderParamStringField(string label, ShaderParamStringElement visualInput) : base(label, visualInput)
        {
            ShaderParamStringElement = visualInput;
            visualInput.BindBound(this);
        }
    }
}
#endif
