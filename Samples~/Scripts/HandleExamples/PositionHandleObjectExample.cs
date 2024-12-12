using UnityEngine;

namespace SaintsField.Samples.Scripts.HandleExamples
{
    public class PositionHandleObjectExample : MonoBehaviour
    {
        [GetComponentInChildren(excludeSelf: true), PositionHandle, DrawLabel("$" + nameof(LabelName))]
        public MeshRenderer[] meshChildren;

        private string LabelName(MeshRenderer target, int index) => $"{target.name}[{index}]";
    }
}
