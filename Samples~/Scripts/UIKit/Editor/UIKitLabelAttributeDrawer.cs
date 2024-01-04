using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace SaintsField.Samples.Scripts.UIKit.Editor
{
    [CustomPropertyDrawer(typeof(UIKitLabelAttribute))]
    public class UIKitLabelAttributeDrawer: PropertyDrawer
    {
        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            VisualElement aboveContainer = new VisualElement();

            aboveContainer.style.flexDirection = FlexDirection.Row;
            aboveContainer.style.flexWrap = Wrap.NoWrap;

            var oneHelpBox =
                new HelpBox(
                    "Some Tips Some Tips Some Tips Some Tips Some Tips Some Tips Some Tips Some Tips Some Tips Some Tips Some Tips Some Tips Some Tips Some Tips Some Tips Some Tips Some Tips ",
                    HelpBoxMessageType.Info);
            // oneHelpBox.style.width = 50;
            oneHelpBox.style.flexGrow = 1;
            aboveContainer.Add(oneHelpBox);
            var anotherHelpBox = new HelpBox("Another top", HelpBoxMessageType.Info);
            // anotherHelpBox.style.width = 50;
            anotherHelpBox.style.flexGrow = 1;
            aboveContainer.Add(anotherHelpBox);

            VisualElement fieldContainer = new VisualElement();
            aboveContainer.Add(fieldContainer);

            var prop = new PropertyField(property, " ");

            var label = new Label("property<space=10>.displayName");
            label.style.position = Position.Absolute;
            label.style.left = 0;
            label.style.top = 0;
            // prop.Add(label);

            fieldContainer.Add(label);
            fieldContainer.Add(prop);

            // GUIStyle textStyle = new GUIStyle(EditorStyles.label)
            // {
            //     richText = true,
            // };
            // Debug.Log(textStyle.CalcSize(new GUIContent("property<space=10>.displayName")).x);
            // container.Add(label);

            VisualElement root = new VisualElement();
            root.Add(aboveContainer);
            root.Add(fieldContainer);

            return root;
        }
    }
}
