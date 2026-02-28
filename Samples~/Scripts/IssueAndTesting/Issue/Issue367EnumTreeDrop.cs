using UnityEngine;

namespace SaintsField.Samples.Scripts.IssueAndTesting.Issue
{
    public class Issue367EnumTreeDrop : SaintsMonoBehaviour
    {
        private enum ScaleMode { None, Uniform, NonUniform }

        [BelowText("<field/>"), OnValueChanged(":Debug.Log")]
        [SerializeField] private ScaleMode _scaleMode;

    }
}
