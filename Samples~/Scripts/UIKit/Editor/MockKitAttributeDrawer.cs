using System.Drawing;
using UnityEditor;
using UnityEngine.UIElements;
using Color = UnityEngine.Color;

namespace SaintsField.Samples.Scripts.UIKit.Editor
{
    [CustomPropertyDrawer(typeof(MockKitAttribute))]
    public class MockKitAttributeDrawer: PropertyDrawer
    {
        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            VisualElement root = new VisualElement
            {
                style =
                {
                    // flexDirection = FlexDirection.Row,
                    // marginLeft = 3,
                    // marginRight = -2,
                    // marginTop = 1,
                    // marginBottom = 1,
                    // flexGrow = 1,
                },
            };

            root.AddToClassList("unity-base-field");
            // root.AddToClassList("unity-base-text-field");
            // root.AddToClassList("unity-text-field");
            // root.AddToClassList("unity-base-field__inspector-field");

            Label label = new Label(property.displayName)
            {

            };

            // label.AddToClassList("unity-text-element");
            // label.AddToClassList("unity-label");
            label.AddToClassList("unity-base-field__label");
            // label.AddToClassList("unity-base-text-field__label");
            // label.AddToClassList("unity-text-field__label");
            root.Add(label);

            // VisualElement textFieldContainer = new VisualElement();
            // textFieldContainer.AddToClassList("unity-base-text-field__input");
            // textFieldContainer.AddToClassList("unity-base-text-field__input--single-line");
            // textFieldContainer.AddToClassList("unity-base-field__input");
            // textFieldContainer.AddToClassList("unity-text-field__input");

            // TextField textField = new TextField()
            // {
            //     style =
            //     {
            //         flexGrow = 1,
            //         marginLeft = 0,
            //         marginRight = 0,
            //     }
            // };

            VisualElement textField = new VisualElement
            {
                style =
                {
                    // backgroundColor = Color.green,
                    flexGrow = 1,
                    borderLeftWidth = 1,
                    borderRightWidth = 1,
                    borderLeftColor = Color.green,
                    borderRightColor = Color.green,
                },
            };
            // textField.AddToClassList("unity-text-element");
            // textField.AddToClassList("unity-text-element--inner-input-field-component");

            // textField.AddToClassList("unity-base-text-field__input");
            // textField.AddToClassList("unity-base-text-field__input--single-line");
            // textField.AddToClassList("unity-base-field__input");
            // textField.AddToClassList("unity-text-field__input");
            root.Add(textField);

            return root;
        }
    }
}
