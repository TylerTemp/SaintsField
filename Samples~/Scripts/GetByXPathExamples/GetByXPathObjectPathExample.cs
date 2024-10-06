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
        [GetByXPath("./NoSuchTarget!"), ShowIf(nameof(noSuchTarget)), ReadOnly] public GameObject noSuchTarget;
        [GetByXPath(".//Must*/Child"), ShowIf(nameof(hasSuchTarget)), ReadOnly] public GameObject hasSuchTarget;

        [GetByXPath(".//Child[@{activeSelf}]")] public GameObject[] childActive;

        [GetByXPath("scene:://@{GetComponent(Camera)}[@{tag} = 'MainCamera']")] public Camera mainCamera;
        [GetByXPath("//*[@layer = 'Ignore Raycast']")] public GameObject ignoreRaycast;
    }
}
