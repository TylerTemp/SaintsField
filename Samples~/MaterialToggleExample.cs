using UnityEngine;

namespace SaintsField.Samples
{
    public class MaterialToggleExample: MonoBehaviour
    {
        [SerializeField, MaterialToggle] private Material _mat1;
        [SerializeField, MaterialToggle] private Material _mat2;
    }
}
