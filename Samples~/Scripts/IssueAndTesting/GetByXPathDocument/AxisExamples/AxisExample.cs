using SaintsField.Samples.Scripts.SaintsEditor;
using UnityEngine;

namespace SaintsField.Samples.Scripts.IssueAndTesting.GetByXPathDocument.AxisExamples
{
    public class AxisExample : SaintsMonoBehaviour
    {
        // search current scene that ends with `Camera`
        [GetByXPath("scene:://*Camera")] public Camera[] sceneCameras;
        // get the first folder starts with `Issue`, and get all the prefabs
        [GetByXPath("assets:://Issue*/*.prefab")] public GameObject[] prefabs;
    }
}
