using UnityEngine;
using UnityEngine.Rendering;

namespace SaintsField.Samples.Scripts
{
    public class PrefabChangeTest : MonoBehaviour
    {
        [ResizableTextArea] public string text;
        [PropRange(0, 100)] public int intValue;
        [Layer] public string layerString;
        [Layer] public int layerInt;

        [Scene] public int sceneInt;
        [Scene] public string sceneString;
        [Scene(fullPath: true)] public string sceneFullPathString;

        [SortingLayer] public string sortingLayerString;
        [SortingLayer] public int sortingLayerInt;

        [Tag] public string tagString;

        [InputAxis] public string inputAxisString;

        [ShaderParam] public string shaderParamString;
        [ShaderParam(ShaderPropertyType.Color)] public int shaderParamInt;

        [ShaderKeyword] public string shaderKeyword;

        [Rate(0, 5)] public int rate05;
        [Rate(1, 5)] public int rate15;
        [Rate(3, 5)] public int rate35;
    }
}
