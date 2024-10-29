using SaintsField.Samples.Scripts.SaintsEditor;
using UnityEngine;

namespace SaintsField.Samples.Scripts.IssueAndTesting.GetByXPathDocument.AttributeExamples
{
    public class CallbackExample : SaintsMonoBehaviour
    {
        // 1.  search all the children which has `FunctionProvider`, grab the first result
        // 2.  call `GetTransforms()` as the results
        // 3.  from the results, get first direct children named "ok"
        [GetByXPath("//*@{GetComponent(FunctionProvider).GetTransforms()}/ok")] public GameObject[] getObjs;
    }
}
