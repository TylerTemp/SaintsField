using UnityEngine;

namespace SaintsField.Samples.Scripts.SaintsArrayExamples
{
    public class SaintsListIssueFallback : MonoBehaviour
    {
        [FieldRichLabel("<color=cyan><label /><icon=star.png />"), BelowRichLabel("Hi")]
        public SaintsList<GameObject> noAttrFallback;
    }
}
