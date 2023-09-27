using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace ExtInspector.Editor.Utils
{
    public static class Util
    {
        public static void BeginBoxGroup_Layout(string label = "")
        {
            EditorGUILayout.BeginVertical(GUI.skin.box);
            if (!string.IsNullOrEmpty(label))
            {
                EditorGUILayout.LabelField(label, EditorStyles.boldLabel);
            }
        }

        // public static void NonSerializedField_Layout(UnityEngine.Object target, FieldInfo field)
        // {
        //     object value = field.GetValue(target);
        //
        //     if (value == null)
        //     {
        //         string warning = string.Format("{0} is null. {1} doesn't support reference types with null value", ObjectNames.NicifyVariableName(field.Name), typeof(ShowNonSerializedFieldAttribute).Name);
        //         HelpBox_Layout(warning, MessageType.Warning, context: target);
        //     }
        //     else if (!Field_Layout(value, ObjectNames.NicifyVariableName(field.Name)))
        //     {
        //         string warning = string.Format("{0} doesn't support {1} types", typeof(ShowNonSerializedFieldAttribute).Name, field.FieldType.Name);
        //         HelpBox_Layout(warning, MessageType.Warning, context: target);
        //     }
        // }

        // public static GUIContent GetLabel(SerializedProperty property)
        // {
        //     LabelAttribute labelAttribute = GetAttribute<LabelAttribute>(property);
        //     string labelText = (labelAttribute == null)
        //         ? property.displayName
        //         : labelAttribute.Label;
        //
        //     GUIContent label = new GUIContent(labelText);
        //     return label;
        // }

        // public static void PropertyField_Layout(SerializedProperty property, bool includeChildren)
        // {
        //     Rect dummyRect = new Rect();
        //     PropertyField_Implementation(dummyRect, property, includeChildren, DrawPropertyField_Layout);
        // }
        //
        // private static void PropertyField_Implementation(Rect rect, SerializedProperty property, bool includeChildren, PropertyFieldFunction propertyFieldFunction)
        // {
        //     SpecialCaseDrawerAttribute specialCaseAttribute = PropertyUtility.GetAttribute<SpecialCaseDrawerAttribute>(property);
        //     if (specialCaseAttribute != null)
        //     {
        //         specialCaseAttribute.GetDrawer().OnGUI(rect, property);
        //     }
        //     else
        //     {
        //         // Check if visible
        //         bool visible = PropertyUtility.IsVisible(property);
        //         if (!visible)
        //         {
        //             return;
        //         }
        //
        //         // Validate
        //         ValidatorAttribute[] validatorAttributes = PropertyUtility.GetAttributes<ValidatorAttribute>(property);
        //         foreach (var validatorAttribute in validatorAttributes)
        //         {
        //             validatorAttribute.GetValidator().ValidateProperty(property);
        //         }
        //
        //         // Check if enabled and draw
        //         EditorGUI.BeginChangeCheck();
        //         bool enabled = PropertyUtility.IsEnabled(property);
        //
        //         using (new EditorGUI.DisabledScope(disabled: !enabled))
        //         {
        //             propertyFieldFunction.Invoke(rect, property, PropertyUtility.GetLabel(property), includeChildren);
        //         }
        //
        //         // Call OnValueChanged callbacks
        //         if (EditorGUI.EndChangeCheck())
        //         {
        //             PropertyUtility.CallOnValueChangedCallbacks(property);
        //         }
        //     }
        // }
        //
        // private static void DrawPropertyField_Layout(Rect rect, SerializedProperty property, GUIContent label, bool includeChildren)
        // {
        //     EditorGUILayout.PropertyField(property, label, includeChildren);
        // }
        public static void EndBoxGroup_Layout()
        {
            EditorGUILayout.EndVertical();
        }
    }
}
