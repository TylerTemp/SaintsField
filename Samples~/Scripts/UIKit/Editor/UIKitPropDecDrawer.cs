using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace SaintsField.Samples.Scripts.UIKit.Editor
{
    [CustomPropertyDrawer(typeof(UIKitPropDecAttribute))]
    public class UIKitPropDecDrawer : PropertyDrawer
    {
        // private RichTextDrawer

        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
// #if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_DRAW_PROCESS_CORE
//         Debug.Log($"Create property gui {property.serializedObject.targetObject} {property.propertyPath}/{this}");
// #endif

            // // Create property container element.
            // VisualElement container = new VisualElement();
            //
            // // Create property fields.
            // IntegerField amountField = new IntegerField("int");
            //
            // // Add fields to the container.
            // container.Add(amountField);
            //
            // container.Add(new PropertyField(property, "prop"));
            //
            // return container;

            return new PropertyField(property);
            // return new TextField(property.displayName);
        }
    }
}
