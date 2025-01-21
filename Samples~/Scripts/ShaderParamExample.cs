using UnityEngine;
using UnityEngine.Rendering;

namespace SaintsField.Samples.Scripts
{
    public class ShaderParamExample : MonoBehaviour
    {
        [ShaderParam] public string shaderParamString;
        [ShaderParam(0)] public int shaderParamInt;
        [ShaderParam(ShaderPropertyType.Texture)] public int shaderParamFilter;

        [Separator("By Target")]
        [GetComponent] public Renderer targetRenderer;

        [ShaderParam("$" + nameof(targetRenderer))] public int shaderParamRenderer;

        private Material GetMat() => targetRenderer.sharedMaterial;
        [ShaderParam("$" + nameof(GetMat))] public int shaderParamMat;

        [ReadOnly, RichLabel("<icon=star.png/><label/>"), ShaderParam]
        public string readOnlyField;
    }
}
