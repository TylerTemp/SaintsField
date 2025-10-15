using UnityEngine;

namespace SaintsField.Samples.Scripts.IssueAndTesting.NA
{
    public class Issue322 : MonoBehaviour
    {
        // work with NaughtyAttributes
        [FieldLabelText("<color=lime><label/>")]
#if SAINTSFIELD_SAMPLE_NAUGHYTATTRIBUTES
        [NaughtyAttributes.ResizableTextArea, NaughtyAttributes.Label(" ")]
#endif
        public string thisIsAVariable;

        // work alone
        [FieldLabelText("<color=#ff0066><label/>")]
        public Vector3 vectorXYZ;

        // work with Unity's built-in attributes
        [FieldLabelText("<color=pink><label/>")]
        [Range(0, 1)]
        public float range;
    }
}
