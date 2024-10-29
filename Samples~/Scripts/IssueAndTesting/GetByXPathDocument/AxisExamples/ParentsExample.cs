using SaintsField.Samples.Scripts.SaintsEditor;
using UnityEngine;

namespace SaintsField.Samples.Scripts.IssueAndTesting.GetByXPathDocument.AxisExamples
{
    public class ParentsExample : SaintsMonoBehaviour
    {
        // search all parents that starts with `Sub`
        [GetByXPath("ancestor:://Sub*")] public Transform ancestorStartsWithSub;
        // search current scene that ends with `Camera`
        [GetByXPath("scene:://*Camera")] public Camera[] sceneCameras;
        // get the first folder with name `Anim`, and get all the animations
        [GetByXPath("assets:://Anim/*.anim")] public Animation[] animations;
    }
}
