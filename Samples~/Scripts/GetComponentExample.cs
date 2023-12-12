using UnityEngine;

namespace SaintsField.Samples.Scripts
{
    public class GetComponentExample: MonoBehaviour
    {
        [GetComponent] public BoxCollider otherComponent;
        [GetComponent] public GameObject selfGameObject;  // get the GameObject itself
        [GetComponent] public RectTransform selfRectTransform;  // useful for UI

        [GetComponent] public GetComponentExample selfScript;  // yeah you can get your script itself
        [GetComponent] public Dummy otherScript;  // other script
    }
}
