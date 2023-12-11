using UnityEngine;

namespace SaintsField.Samples.Scripts
{
    public class GetComponentExample: MonoBehaviour
    {
        [GetComponent] public GetComponentExample selfScript;
        [GetComponent] public Dummy otherScript;
        [GetComponent] public BoxCollider otherComponent;
        [GetComponent] public GameObject selfGameObject;
        [GetComponent] public Transform selfTransform;
    }
}
