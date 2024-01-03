using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

[CustomPropertyDrawer(typeof(UIKitPropDec))]
public class UIKitPropDecDrawer : PropertyDrawer
{
    public override VisualElement CreatePropertyGUI(SerializedProperty property)
    {
        // Create property container element.
        VisualElement container = new VisualElement();

        // Create property fields.
        PropertyField amountField = new PropertyField(property);

        // Add fields to the container.
        container.Add(amountField);

        return container;
    }
}
