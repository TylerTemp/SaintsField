using UnityEngine;

namespace SaintsField.Samples.Scripts
{
    public class GetComponentExample: MonoBehaviour
    {
        [GetComponent(EXP.NoAutoResignToValue | EXP.NoAutoResignToNull | EXP.NoPicker), PostFieldButton(nameof(DebugShow), "D")] public BoxCollider otherComponent;
        [GetComponent] public GameObject selfGameObject;  // get the GameObject itself
        [GetComponent] public RectTransform selfRectTransform;  // useful for UI

        [GetComponent] public GetComponentExample selfScript;  // yeah you can get your script itself
        [GetComponent] public Dummy otherScript;  // other script
        [GetComponent] public SpriteRenderer noSuch;  // other script

        [Separator("GetByXPath")]
        // alternative
        [GetByXPath(".")] public BoxCollider otherComponentAlternative;
        [GetByXPath(".")] public GameObject selfGameObjectAlternative;  // get the GameObject itself
        [GetByXPath(".")] public RectTransform selfRectTransformAlternative;  // useful for UI

        [GetByXPath(".")] public GetComponentExample selfScriptAlternative;  // yeah you can get your script itself
        [GetByXPath(".")] [GetComponent] public Dummy otherScriptAlternative;  // other script

        private void DebugShow(object o) => Debug.Log(o);
    }
}
