using UnityEngine;

namespace SaintsField.Samples.Scripts
{
    public class TagExample: MonoBehaviour
    {
        [SerializeField, Tag][FieldLabelText("<icon=star.png /><label/>")] private string _tag;
        [SerializeField, Tag] private string _tag2;

        [ReadOnly]
        [SerializeField, Tag][FieldLabelText("<icon=star.png /><label/>")] private string _tagDisabled;
    }
}
