using UnityEngine;

namespace SaintsField.Samples.Scripts.RichLabelExamples
{
    public class RichLabelFieldExample : MonoBehaviour
    {
        [Separator("Field")]
        // read field value
        [RichLabel("<color=lime><field/>")] public string fieldLabel;
        // read the `_subLabel` field/function from the field
        [RichLabel("<field._subLabel/>"), GetComponentInChildren, Expandable] public SubField subField;
        // again read the property
        [RichLabel("<color=lime><field.gameObject.name/>")] public SubField subFieldName;

        [Separator("Field Null")]
        // not found target will be rendered as empty string
        [RichLabel("<field._subLabel/>")] public SubField notFoundField;
        [RichLabel("<field._noSuch/>"), GetComponentInChildren] public SubField notFoundField2;

        [Separator("Formatter")]
        // format as percent
        [RichLabel("<field=P2/>"), PropRange(0f, 1f)] public float percent;
        // format `doubleVal` field as exponential
        [RichLabel("<field.doubleVal=E/>")] public SubField subFieldCurrency;
    }
}
