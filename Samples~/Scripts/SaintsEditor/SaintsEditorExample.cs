using SaintsField.Playa;
using UnityEngine;

namespace SaintsField.Samples.Scripts.SaintsEditor
{
    public class SaintsEditorExample : MonoBehaviour
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
        [Ordered] public string other;
    }
}
