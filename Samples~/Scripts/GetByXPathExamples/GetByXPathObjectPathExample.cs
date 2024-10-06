using UnityEngine;

namespace SaintsField.Samples.Scripts.GetByXPathExamples
{
    public class GetByXPathObjectPathExample : MonoBehaviour
    {
        [GetByXPath(".//Must*/Child")] public GameObject child;
        [GetByXPath(EXP.NoPicker, ".//Must*/Child")] public GameObject noPicker;
        [GetByXPath(EXP.Silent, ".//Must*/Child")] public GameObject refreshButton;
        [GetByXPath(EXP.Silent, ".//No/Such/Child")] public GameObject removeButton;

        [GetByXPath(EXP.Message, ".//Must*/Child")] public GameObject messageMismatch;
        [GetByXPath(EXP.Message, ".//No/Such/Child")] public GameObject messageNotFound;
    }
}
