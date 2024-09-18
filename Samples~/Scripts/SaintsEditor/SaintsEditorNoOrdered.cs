using SaintsField.Playa;
using UnityEngine;

namespace SaintsField.Samples.Scripts.SaintsEditor
{
    public class SaintsEditorNoOrdered : SaintsMonoBehaviour
    {
        public string myStartField;
        [ShowInInspector] public const float MyConstFloat = 3.14f;
        [ShowInInspector] public static readonly Color MyColor = Color.green;

        [ShowInInspector]
        public Color AutoColor
        {
            get => Color.green;
            // ReSharper disable once ValueParameterNotUsed
            set
            {
                // nothing
            }
        }

        [Button]
        private void EditorButton()
        {
            Debug.Log("EditorButton");
        }

        [Button("Label")]
        private void EditorLabeledButton()
        {
            Debug.Log("EditorLabeledButton");
        }

        public string myOtherFieldUnderneath;
        [BelowRichLabel(nameof(_testEnum), true)]
        public string other;

        private enum TestEnum
        {
            First = 1,
            Second = 2,
        }

        [ShowInInspector]
        private TestEnum _testEnum;

        [Button("Set Value")]
        private void SetEnumValue()
        {
            // Debug.Log(_testEnum);
            _testEnum = _testEnum ==  TestEnum.First? TestEnum.Second : TestEnum.First;
        }
    }
}
