using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

// IngredientDrawer
namespace SaintsField.Samples.Scripts.UIKit.Editor
{
    [CustomPropertyDrawer(typeof(Ingredient))]
    public class IngredientDrawer : PropertyDrawer
    {
        // Draw the property inside the given rect
        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            // Create property container element.
            VisualElement container = new VisualElement();

            // Create property fields.
            PropertyField amountField = new PropertyField(property.FindPropertyRelative("amount"));
            PropertyField unitField = new PropertyField(property.FindPropertyRelative("unit"));
            PropertyField nameField = new PropertyField(property.FindPropertyRelative("name"), "Fancy Name");

            // Add fields to the container.
            container.Add(amountField);
            container.Add(unitField);
            container.Add(nameField);

            return container;
        }
    }
}
