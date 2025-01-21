using UnityEngine;

namespace SaintsField.Samples.Scripts
{
    public class ShaderKeywordExample : MonoBehaviour
    {
#if UNITY_2021_2_OR_NEWER
        [ShaderKeyword] public string shaderKeywordString;
        [ShaderKeyword(0)] public string shaderKeywordIndex;

        [Separator("By Target")]
        [GetComponent] public Renderer targetRenderer;

        [ShaderKeyword(nameof(targetRenderer))] public string shaderKeywordRenderer;

        private Material GetMat() => targetRenderer.sharedMaterial;
        [ShaderKeyword(nameof(GetMat))] public string shaderKeywordMat;

        private Shader GetShader() => targetRenderer.sharedMaterial.shader;
        [ShaderKeyword(nameof(GetShader))] public string shaderKeywordShader;

        [ReadOnly, RichLabel("<icon=star.png/><label/>"), ShaderKeyword]
        public string readOnlyField;
#endif
    }
}
