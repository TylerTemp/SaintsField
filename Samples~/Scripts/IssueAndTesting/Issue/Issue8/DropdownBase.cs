using UnityEngine;

namespace SaintsField.Samples.Scripts.IssueAndTesting.Issue.Issue8
{
    public class DropdownBase : MonoBehaviour
    {
        [SerializeField] private string label;

        [Dropdown(nameof(GetDropdownItems)), RichLabel(nameof(GetLabel), true)]
        public float floatV;

        private string GetLabel() => string.IsNullOrEmpty(label) ? null : label;

        private DropdownList<float> GetDropdownItems()
        {
            return new DropdownList<float>
            {
                { "1", 1.0f },
                { "2", 2.0f },
                { "3/1", 3.1f },
                { "3/2", 3.2f },
            };
        }
    }
}
