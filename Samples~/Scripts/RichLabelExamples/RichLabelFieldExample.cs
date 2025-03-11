using UnityEngine;

namespace SaintsField.Samples.Scripts.RichLabelExamples
{
    public class RichLabelFieldExample : MonoBehaviour
    {
        [Separator("Field")]
        [RichLabel("<color=lime><field/>")] public string fieldLabel;
        [RichLabel("<field._subLabel/>"), GetComponentInChildren, Expandable] public SubField subField;
        [RichLabel("<color=lime><field.gameObject.name/>")] public SubField subFieldName;

        [Separator("Field Null")]
        [RichLabel("<field._subLabel/>")] public SubField notFoundField;
        [RichLabel("<field._noSuch/>"), GetComponentInChildren] public SubField notFoundField2;

        [Separator("Formatter")]

        [RichLabel("<field=P2/>"), PropRange(0f, 1f)] public float percent;
        [RichLabel("<field.doubleVal=E/>")] public SubField subFieldCurrency;
    }
}
