using UnityEngine;

namespace SaintsField.Samples.Scripts.UIKit
{
    public class UIKitLabelTest : MonoBehaviour
    {
        [Tooltip("Some Tips"), UIKitLabel] public int intDrag;

        [Tooltip("Textarea Tips"), UIKitAutoResizeTextArea] public string textArea;
    }
}
