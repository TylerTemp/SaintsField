using SaintsField.Playa;
using UnityEngine;

namespace SaintsField.Samples.Scripts.SaintsEditor
{
    public class SaintsEditorOrdered : SaintsMonoBehaviour
    {
        [Ordered] public string myStartField;
        [ShowInInspector, Ordered] public const float MyConstFloat = 3.14f;
        [ShowInInspector, Ordered] public static readonly Color MyColor = Color.green;

        [ShowInInspector, Ordered]
        public Color AutoColor
        {
            get => Color.green;
            // ReSharper disable once ValueParameterNotUsed
            set
            {
                // nothing
            }
        }

        [Button, Ordered]
        private void EditorButton()
        {
            Debug.Log("EditorButton");
        }

        [Button("Label"), Ordered]
        private void EditorLabeledButton()
        {
            Debug.Log("EditorLabeledButton");
        }

        [Ordered] public string myOtherFieldUnderneath;
        [BelowRichLabel(nameof(_testEnum), true)]
        [Ordered] public string other;

        private enum TestEnum
        {
            First = 1,
            Second = 2,
        }

        [ShowInInspector, Ordered]
        private TestEnum _testEnum;

        [Button("Set Value"), Ordered]
        private void SetEnumValue()
        {
            // Debug.Log(_testEnum);
            _testEnum = _testEnum ==  TestEnum.First? TestEnum.Second : TestEnum.First;
        }

        [ShowInInspector] private SaintsEditorOrdered nullValue;
    }
}
