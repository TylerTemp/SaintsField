#if UNITY_2021_3_OR_NEWER
using SaintsField.Editor.UIToolkitElements;
using UnityEngine;
using UnityEngine.Rendering;

namespace SaintsField.Editor.Drawers.ShaderDrawers.ShaderParamDrawer
{
    public class ShaderParamIntElement: IntDropdownElement
    {
        private Shader _shader;
        private readonly ShaderPropertyType? _filterPropertyType;

        public ShaderParamIntElement(ShaderPropertyType? filterPropertyType)
        {
            _filterPropertyType = filterPropertyType;
        }

        public void BindShader(Shader shader)
        {
            _shader = shader;
            RefreshDisplay();
        }

        public override void SetValueWithoutNotify(int newValue)
        {
            CachedValue = newValue;
            RefreshDisplay();
        }

        private void RefreshDisplay()
        {
            if(_shader != null)
            {
                foreach (ShaderParamUtils.ShaderCustomInfo r in ShaderParamUtils.GetShaderInfo(_shader, _filterPropertyType))
                {
                    // ReSharper disable once InvertIf
                    if(r.PropertyID == CachedValue)
                    {
                        Label.text = r.GetString(false);
                        return;
                    }
                }
            }

            Label.text = $"<color=red>?</color> {(CachedValue == -1? "": $"({CachedValue})")}";
        }
    }
}
#endif
