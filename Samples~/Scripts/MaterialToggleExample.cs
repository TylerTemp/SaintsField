using UnityEngine;

namespace SaintsField.Samples.Scripts
{
    public class MaterialToggleExample: MonoBehaviour
    {
        public Renderer targetRenderer;
        [MaterialToggle(nameof(targetRenderer))] public Material _mat1;
        [MaterialToggle(nameof(targetRenderer))] public Material _mat2;
        [ReadOnly]
        [MaterialToggle(nameof(targetRenderer))] public Material _matDisabled;
    }
}
