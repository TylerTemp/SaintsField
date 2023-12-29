using SaintsField.Unsaintly;
using UnityEngine;

namespace SaintsField.Samples.Scripts.Unsaintly
{
    public class General : MonoBehaviour
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
    }
}
