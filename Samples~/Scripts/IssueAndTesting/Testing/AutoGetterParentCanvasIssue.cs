using SaintsField.Samples.Scripts.SaintsEditor;
using UnityEngine;

namespace SaintsField.Samples.Scripts.IssueAndTesting.Testing
{
    public class AutoGetterParentCanvasIssue : SaintsMonoBehaviour
    {
        [GetComponentInParent(excludeSelf: true)] public Canvas canvas;
        [GetComponentInParents] public Canvas canvasS;
        [GetByXPath("ancestor::")] public Canvas canvasX;
    }
}
