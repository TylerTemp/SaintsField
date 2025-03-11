using UnityEngine;

namespace SaintsField.Samples.Scripts.RichLabelExamples
{
    public class FullWidthRichLabelFieldExample : MonoBehaviour
    {
        [Separator("Field")]
        [BelowRichLabel("<color=gray><field/>")] public string fieldLabel;
        [BelowRichLabel("<color=gray><field._subLabel/>"), GetComponentInChildren, Expandable] public SubField subField;
        [BelowRichLabel("<color=gray><field.gameObject.name/>")] public SubField subFieldName;

        [Separator("Field Null")]
        [BelowRichLabel("<field._subLabel/>")] public SubField notFoundField;
        [BelowRichLabel("<field._noSuch/>"), GetComponentInChildren] public SubField notFoundField2;

        [Separator("Formatter")]

        [BelowRichLabel("<color=gray><field=P2/>"), PropRange(0f, 1f)] public float percent;
        [BelowRichLabel("<color=gray><field.doubleVal=E/>")] public SubField subFieldCurrency;
    }
}
