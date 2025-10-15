using UnityEngine;

namespace SaintsField.Samples.Scripts.RichLabelExamples
{
    public class FullWidthRichLabelFieldExample : MonoBehaviour
    {
        [FieldSeparator("Field")]
        [FieldBelowText("<color=gray><field/>")] public string fieldLabel;
        [FieldBelowText("<color=gray><field._subLabel/>"), GetComponentInChildren, Expandable] public SubField subField;
        [FieldBelowText("<color=gray><field.gameObject.name/>")] public SubField subFieldName;

        [FieldSeparator("Field Null")]
        [FieldBelowText("<field._subLabel/>")] public SubField notFoundField;
        [FieldBelowText("<field._noSuch/>"), GetComponentInChildren] public SubField notFoundField2;

        [FieldSeparator("Formatter")]

        [FieldBelowText("<color=gray><field=P2/>"), PropRange(0f, 1f)] public float percent;
        [FieldBelowText("<color=gray><field.doubleVal=E/>")] public SubField subFieldCurrency;
    }
}
