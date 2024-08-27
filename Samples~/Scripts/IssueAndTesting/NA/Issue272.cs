using UnityEngine;

namespace SaintsField.Samples.Scripts.IssueAndTesting.NA
{
    public class Issue272 : MonoBehaviour
    {
        [
#if SAINTSFIELD_SAMPLE_NAUGHYTATTRIBUTES
            NaughtyAttributes.Expandable,
            NaughtyAttributes.ReadOnly,
#else
            InfoBox("NaughtyAttributes not installed"),
#endif
        ]
        public Scriptable naScriptable;

        // Hummm, better idea for the behavior?
        [
            GetScriptableObject,
            Expandable,
            ReadOnly,
        ]
        public Scriptable scriptable;
    }
}
