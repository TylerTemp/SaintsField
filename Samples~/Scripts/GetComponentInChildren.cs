using UnityEngine;

namespace SaintsField.Samples.Scripts
{
    public class GetComponentInChildren: MonoBehaviour
    {
        [GetComponentInChildren] public BoxCollider colliderInChildren;
        [GetComponentInChildren(compType: typeof(BoxCollider))] public GameObject goWithColliderInChildren;
    }
}
