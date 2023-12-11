using UnityEngine;

namespace SaintsField.Samples.Scripts
{
    public class GetComponentInChildren: MonoBehaviour
    {
        [GetComponentInChildren] public BoxCollider boxColliderInChildren;
        [GetComponentInChildren(compType: typeof(BoxCollider))] public GameObject goWithBoxColliderInChildren;
    }
}
