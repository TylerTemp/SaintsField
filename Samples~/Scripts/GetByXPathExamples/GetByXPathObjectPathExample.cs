using SaintsField.Samples.Scripts.SaintsEditor;
using UnityEngine;

namespace SaintsField.Samples.Scripts.GetByXPathExamples
{
    public class GetByXPathObjectPathExample : SaintsMonoBehaviour
    {
        [GetByXPath(".//Must*/Child")] public GameObject child;
        [GetByXPath(EXP.NoPicker, ".//Must*/Child")] public GameObject noPicker;
        [GetByXPath(EXP.Silent, ".//Must*/Child")] public GameObject refreshButton;
        [GetByXPath(EXP.Silent, ".//No/Such/Child")] public GameObject removeButton;

        [GetByXPath(EXP.Message, ".//Must*/Child")] public GameObject messageMismatch;
        [GetByXPath(EXP.Message, ".//No/Such/Child")] public GameObject messageNotFound;

        // Show if sign the auto target, hide otherwise
        // Note: this only works for UI Toolkit. For IMGUI, the ShowIf/HideIf will work first, and completely disable the other attributes
        [GetByXPath("./NoSuchTarget!"), ShowIf(nameof(noSuchTarget)), ReadOnly] public GameObject noSuchTarget;
        [GetByXPath(".//Must*/Child"), ShowIf(nameof(hasSuchTargetSoShow)), ReadOnly] public GameObject hasSuchTargetSoShow;

        [GetByXPath(".//Child[@{activeSelf}]")] public GameObject[] childActive;

        [GetByXPath("scene:://@{GetComponent(Camera)}[@{tag} = 'MainCamera']")] public Camera mainCamera;
        [GetByXPath("//*[@layer = 'Ignore Raycast']")] public GameObject ignoreRaycast;
    }
}
