using UnityEngine;

namespace SaintsField.Samples.Scripts.RichLabelExamples
{
    public class RichLabelFieldExample : MonoBehaviour
    {
        [Separator("Field")]
        // read field value
        [FieldRichLabel("<color=lime><field/>")] public string fieldLabel;
        // read the `_subLabel` field/function from the field
        [FieldRichLabel("<field._subLabel/>"), GetComponentInChildren, Expandable] public SubField subField;
        // again read the property
        [FieldRichLabel("<color=lime><field.gameObject.name/>")] public SubField subFieldName;

        [Separator("Field Null")]
        // not found target will be rendered as empty string
        [FieldRichLabel("<field._subLabel/>")] public SubField notFoundField;
        [FieldRichLabel("<field._noSuch/>"), GetComponentInChildren] public SubField notFoundField2;

        [Separator("Formatter")]
        // format as percent
        [FieldRichLabel("<field=P2/>"), PropRange(0f, 1f)] public float percent;
        // format `doubleVal` field as exponential
        [FieldRichLabel("<field.doubleVal=E/>")] public SubField subFieldCurrency;
    }
}
