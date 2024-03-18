using UnityEngine;

namespace SaintsField.Samples.Scripts.IssueAndTesting.Issue.Issue8
{
    public class MaterialToggleBase : MonoBehaviour
    {
        [SerializeField] private Renderer targetRenderer;

        public Renderer TargetRenderer => targetRenderer;

        [SerializeField, MaterialToggle] private Material mat1;

    }
}
