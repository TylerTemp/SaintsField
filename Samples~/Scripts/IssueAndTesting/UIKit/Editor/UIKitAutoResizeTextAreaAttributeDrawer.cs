#if UNITY_EDITOR && UNITY_2021_3_OR_NEWER
using SaintsField.Samples.Scripts.UIKit;
using UnityEditor;
using UnityEngine.UIElements;

namespace SaintsField.Samples.Scripts.IssueAndTesting.UIKit.Editor
{
    [CustomPropertyDrawer(typeof(UIKitAutoResizeTextAreaAttribute))]
    public class UIKitAutoResizeTextAreaAttributeDrawer: PropertyDrawer
    {
        private TextField _textField;

        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            VisualElement root = new VisualElement();

            _textField = new TextField(property.displayName)
            {
                multiline = true,
                // lineType = LineType.MultiLineNewline,
                // whiteSpace = WhiteSpace.Normal,
                value = property.stringValue,
            };
            _textField.style.whiteSpace = WhiteSpace.Normal;
            // _textField.style.line
            // _textField.value = "123";

            _textField.RegisterValueChangedCallback(evt =>
            {
                property.stringValue = evt.newValue;
                // property.serializedObject.ApplyModifiedProperties();
            });

            root.Add(_textField);

            return root;
        }
    }
}
#endif
