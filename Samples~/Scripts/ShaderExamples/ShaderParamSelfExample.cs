using UnityEngine;

namespace SaintsField.Samples.Scripts.ShaderExamples
{
    public class ShaderParamSelfExample : MonoBehaviour
    {
        [ShaderParam] public string shaderParamString;
        [ShaderParam(0)] public int shaderParamInt;
    }
}
