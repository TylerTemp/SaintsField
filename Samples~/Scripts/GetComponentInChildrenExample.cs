using SaintsField.Samples.Scripts.SaintsEditor;
using UnityEngine;

namespace SaintsField.Samples.Scripts
{
    public class GetComponentInChildrenExample: MonoBehaviour
    {
        [GetComponentInChildren] public BoxCollider myChildBoxCollider;
        [GetComponentInChildren(compType: typeof(BoxCollider))] public GameObject myChildBoxColliderGo;
        [GetComponentInChildren(compType: typeof(Dummy))] public BoxCollider myChildAnotherType;

        [GetByXPath(".", ".//*")] public BoxCollider myChildBoxColliderXPath;
        [GetByXPath("[@{GetComponent(BoxCollider)}]"), GetByXPath("//*[@{GetComponent(BoxCollider)}]")] public GameObject myChildBoxColliderGoXPath;
        [GetByXPath("[@{GetComponent(Dummy)}]"), GetByXPath("//*[@{GetComponent(Dummy)}]")] public BoxCollider myChildAnotherTypeXPath;
    }
}
