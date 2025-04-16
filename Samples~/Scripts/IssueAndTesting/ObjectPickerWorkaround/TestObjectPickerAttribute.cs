using SaintsField.Interfaces;
using UnityEngine;

namespace SaintsField.Samples.Scripts.IssueAndTesting.ObjectPickerWorkaround
{
    public class TestObjectPickerAttribute : PropertyAttribute, ISaintsAttribute
    {
        public SaintsAttributeType AttributeType => SaintsAttributeType.Other;
        public string GroupBy => "";
    }
}
