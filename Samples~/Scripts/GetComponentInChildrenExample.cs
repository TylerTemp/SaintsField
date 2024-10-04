using UnityEngine;

namespace SaintsField.Samples.Scripts
{
    public class GetComponentInChildrenExample: MonoBehaviour
    {
        [GetComponentInChildren] public BoxCollider myChildBoxCollider;
        [GetComponentInChildren(compType: typeof(BoxCollider))] public GameObject myChildBoxColliderGo;
        [GetComponentInChildren(compType: typeof(Dummy))] public BoxCollider myChildAnotherType;

        [Separator("GetByXPath")]

        [GetByXPath(".", ".//*")] public BoxCollider myChildBoxColliderXPath;
        [GetByXPath("[@{GetComponent(BoxCollider)}]", "//*[@{GetComponent(BoxCollider)}]")]  public GameObject myChildBoxColliderGoXPath;
        [GetByXPath("[@{GetComponent(Dummy)}]", "//*[@{GetComponent(Dummy)}]")] public BoxCollider myChildAnotherTypeXPath;
    }
}
