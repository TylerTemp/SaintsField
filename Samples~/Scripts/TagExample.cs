using SaintsField.Playa;
using UnityEngine;

namespace SaintsField.Samples.Scripts
{
    public class TagExample: SaintsMonoBehaviour
    {
        [SerializeField, Tag][FieldLabelText("<icon=star.png /><label/>"), OnValueChanged(nameof(OnValueChanged))] private string _tag;

        private void OnValueChanged(string s) => Debug.Log(s);

        [SerializeField, Tag] private string _tag2;

        [FieldReadOnly]
        [SerializeField, Tag][FieldLabelText("<icon=star.png /><label/>")] private string _tagDisabled;

        [ShowInInspector, Tag]
        private string ShowTag
        {
            get => _tag;
            set => _tag = value;
        }
    }
}
