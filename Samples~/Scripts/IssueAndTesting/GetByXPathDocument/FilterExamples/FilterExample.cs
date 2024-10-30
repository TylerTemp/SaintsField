using SaintsField.Samples.Scripts.SaintsEditor;
using UnityEngine;

namespace SaintsField.Samples.Scripts.IssueAndTesting.GetByXPathDocument.FilterExamples
{
    public class FilterExample : SaintsMonoBehaviour
    {
        // find the first main camera in the scene
        [GetByXPath("scene:://[@{tag} = MainCamera]")] public Camera mainCamera;
    }
}
