using UnityEngine;

namespace SaintsField.Samples.Scripts
{
    public class GetComponentInChildrenExample: MonoBehaviour
    {
        [GetComponentInChildren] public BoxCollider childBoxCollider;
        [GetComponentInChildren(compType: typeof(BoxCollider))] public GameObject childBoxColliderGo;
        [GetComponentInChildren(compType: typeof(Dummy))] public BoxCollider childAnotherType;
    }
}
