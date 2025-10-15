using UnityEngine;

namespace SaintsField.Samples.Scripts.SaintsArrayExamples
{
    public class SaintsListIssueFallback : MonoBehaviour
    {
        [FieldLabelText("<color=cyan><label /><icon=star.png />"), FieldBelowText("Hi")]
        public SaintsList<GameObject> noAttrFallback;
    }
}
