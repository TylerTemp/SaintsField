using UnityEngine;

namespace SaintsField.Samples.Scripts.SaintsArrayExamples
{
    public class SaintsArrayIssueRichLabel: MonoBehaviour
    {
        [FieldLabelText("<color=cyan><label /><icon=star.png />"), SaintsArray]
        public SaintsArray<GameObject> withAttr;
    }
}
