using System.Collections;
using System.Collections.Generic;
using SaintsField.Editor.Core;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

[CustomPropertyDrawer(typeof(UIKitPropDec))]
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
        // PropertyField amountField = new PropertyField(property);
        //
        // // Add fields to the container.
        // container.Add(amountField);
        //
        // return container;

        return new PropertyField(property);
        // return new TextField(property.displayName);
    }
}
