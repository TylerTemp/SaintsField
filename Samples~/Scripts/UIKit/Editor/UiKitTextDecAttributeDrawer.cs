using UnityEditor;
using UnityEngine.UIElements;

namespace SaintsField.Samples.Scripts.UIKit.Editor
{
    [CustomPropertyDrawer(typeof(UiKitTextDecAttribute))]
    public class UiKitTextDecAttributeDrawer: PropertyDrawer
    {
        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
// #if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_DRAW_PROCESS_CORE
//         Debug.Log($"Create property gui {property.serializedObject.targetObject} {property.propertyPath}/{this}");
// #endif

            // // Create property container element.
            // VisualElement container = new VisualElement();
            //
            // // Create property fields.
            // PropertyField amountField = new PropertyField(property);
            //
            // // Add fields to the container.
            // container.Add(amountField);
            //
            // return container;

            return new TextField(property.displayName)
            {
                value = property.stringValue,
            };
            // return new TextField(property.displayName);
        }
    }
}
