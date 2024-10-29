using SaintsField.Samples.Scripts.SaintsEditor;
using UnityEngine;

namespace SaintsField.Samples.Scripts.IssueAndTesting.GetByXPathDocument.AxisExamples
{
    public class ParentsExample : SaintsMonoBehaviour
    {
        // search all parents that starts with `Sub`
        [GetByXPath("ancestor:://Sub*")] public Transform ancestorStartsWithSub;

        // search object itself, and all it's parents, with letter `This`
        [GetByXPath("ancestor-or-self::*This*")] public Transform[] parentsSelfWithLetter1;
    }
}
