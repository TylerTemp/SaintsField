using UnityEngine;

namespace SaintsField.Samples.Scripts
{
    public class GetParentByPathExample : MonoBehaviour
    {
        [GetComponentByPath("..")] public GameObject parent;
        [GetComponentByPath("../..")] public GetComponentByPathExample grandParent;
    }
}
