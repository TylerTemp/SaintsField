using SaintsField.Playa;
using UnityEngine;
using UnityEngine.Rendering;

namespace SaintsField.Samples.Scripts
{
    public class ShaderParamExample : SaintsMonoBehaviour
    {
#if UNITY_2021_2_OR_NEWER
        public ShaderParam shaderParamType;
        // attribute is also supported
        [ShaderParam(ShaderPropertyType.Float)] public ShaderParam shaderParamTypeWithFilter;

        [ShaderParam, OnValueChanged(":Debug.Log")] public string shaderParamString;
        [ShaderParam(0)] public int shaderParamInt;
        [ShaderParam(ShaderPropertyType.Texture)] public int shaderParamFilter;

        [FieldSeparator("By Target")]
        [GetComponent] public Renderer targetRenderer;

        [ShaderParam(nameof(targetRenderer))] public int shaderParamRenderer;

        private Material GetMat() => targetRenderer.sharedMaterial;
        [ShaderParam(nameof(GetMat))] public int shaderParamMat;

        private Shader GetShader() => targetRenderer.sharedMaterial.shader;
        [ShaderParam(nameof(GetShader))] public int shaderParamShader;

        [FieldReadOnly, FieldLabelText("<icon=star.png/><label/>"), ShaderParam]
        public string readOnlyField;

        [Separator]
        [ShowInInspector, ShaderParam]
        public string ShowShaderParamString
        {
            get => shaderParamString;
            set => shaderParamString = value;
        }

        [ShowInInspector, ShaderParam]
        public int ShowShaderParamInt
        {
            get => shaderParamInt;
            set => shaderParamInt = value;
        }

        // private void OnValueChanged(string s) => Debug.Log(s);

        private void Awake()
        {
            // directly put into `Set*` in material, it'll convert & cache the hash id
            targetRenderer.material.SetFloat(shaderParamTypeWithFilter, 2);
        }
#endif
    }
}
