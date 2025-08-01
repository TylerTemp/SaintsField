#if UNITY_2021_2_OR_NEWER
using SaintsField.Editor.UIToolkitElements;
using UnityEngine;

namespace SaintsField.Editor.Drawers.ShaderDrawers.ShaderKeywordDrawer
{
    public class ShaderKeywordElement: StringDropdownElement
    {
        private Shader _shader;

        public void BindShader(Shader shader)
        {
            _shader = shader;
            RefreshDisplay();
        }

        public override void SetValueWithoutNotify(string newValue)
        {
            CachedValue = newValue;
            RefreshDisplay();
        }

        private void RefreshDisplay()
        {
            if(_shader != null)
            {
                foreach (string shaderKeyword in ShaderKeywordUtils.GetShaderKeywords(_shader))
                {
                    // ReSharper disable once InvertIf
                    if(shaderKeyword == CachedValue)
                    {
                        Label.text = shaderKeyword;
                        return;
                    }
                }
            }

            Label.text = $"<color=red>?</color> {(string.IsNullOrEmpty(CachedValue)? "": $"({CachedValue})")}";
        }
    }
}
#endif
