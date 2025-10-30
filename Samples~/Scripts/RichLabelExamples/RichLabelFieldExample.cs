using UnityEngine;

namespace SaintsField.Samples.Scripts.RichLabelExamples
{
    public class RichLabelFieldExample : MonoBehaviour
    {
        [FieldSeparator("Field")]
        // read field value
        [FieldLabelText("<color=lime><field/>")] public string fieldLabel;
        // read the `_subLabel` field/function from the field
        [FieldLabelText("<field._subLabel/>"), GetComponentInChildren, Expandable] public SubField subField;
        // again read the property
        [FieldLabelText("<color=lime><field.gameObject.name/>")] public SubField subFieldName;

        [FieldSeparator("Field Null")]
        // not found target will be rendered as empty string
        [FieldLabelText("<field._subLabel/>")] public SubField notFoundField;
        [FieldLabelText("<field._noSuch/>"), GetComponentInChildren] public SubField notFoundField2;

        [FieldSeparator("Formatter")]
        // format as percent
        [FieldLabelText("<field=P2/>"), PropRange(0f, 1f)] public float percent;
        // format `doubleVal` field as exponential
        [FieldLabelText("<field.doubleVal=E/>")] public SubField subFieldCurrency;
    }
}
