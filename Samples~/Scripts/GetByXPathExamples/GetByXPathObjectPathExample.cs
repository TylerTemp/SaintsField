using UnityEngine;

namespace SaintsField.Samples.Scripts.GetByXPathExamples
{
    public class GetByXPathObjectPathExample : MonoBehaviour
    {
        [GetByXPath(".//Must*/Child")] public GameObject child;
    }
}
