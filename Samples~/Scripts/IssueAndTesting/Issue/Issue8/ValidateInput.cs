using UnityEngine;

namespace SaintsField.Samples.Scripts.IssueAndTesting.Issue.Issue8
{
    public class ValidateInput : ValidateInputBase
    {
        [ValidateInput(nameof(OnValidateCallback))]
        public int validate;
    }
}
