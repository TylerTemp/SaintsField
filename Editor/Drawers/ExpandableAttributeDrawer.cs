using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using SaintsField.Editor.Core;
using SaintsField.Editor.Utils;
using UnityEditor;
using UnityEngine;

namespace SaintsField.Editor.Drawers
{
    [CustomPropertyDrawer(typeof(ExpandableAttribute))]
    public class ExpandableAttributeDrawer: SaintsPropertyDrawer
    {
        private string _error = "";

        private bool _expanded;

        protected override (bool isActive, Rect position) DrawPreLabel(Rect position, SerializedProperty property, GUIContent label, ISaintsAttribute saintsAttribute)
        {
            _expanded = EditorGUI.Foldout(position, _expanded, GUIContent.none, true);
            return (true, position);
        }

        protected override bool WillDrawBelow(Rect position, SerializedProperty property, GUIContent label, ISaintsAttribute saintsAttribute)
        {
            return true;
        }

        protected override float GetBelowExtraHeight(SerializedProperty property, GUIContent label, float width, ISaintsAttribute saintsAttribute)
        {
            float basicHeight = _error == "" ? 0 : HelpBox.GetHeight(_error, width, MessageType.Error);
            if (!_expanded)
            {
                return basicHeight;
            }

            ScriptableObject scriptableObject = property.objectReferenceValue as ScriptableObject;
            SerializedObject serializedObject = new SerializedObject(scriptableObject);
            float expandedHeight = GetAllField(serializedObject).Select(childProperty =>
                GetPropertyHeight(childProperty, new GUIContent(childProperty.displayName))).Sum();

            return basicHeight + expandedHeight;
        }

        protected override Rect DrawBelow(Rect position, SerializedProperty property, GUIContent label, ISaintsAttribute saintsAttribute)
        {
            ScriptableObject scriptableObject = property.objectReferenceValue as ScriptableObject;
            if (property.propertyType != SerializedPropertyType.ObjectReference)
            {
                _error = $"Expected ScriptableObject type, get {property.propertyType}";
            }
            else if (!(property.objectReferenceValue is ScriptableObject))
            {
                _error = $"Expected ScriptableObject type, get {property.objectReferenceValue.GetType()}";
            }
            else
            {
                _error = "";
            }

            Rect leftRect = position;

            if (_error != "")
            {
                leftRect = HelpBox.Draw(position, _error, MessageType.Error);
            }

            if (!_expanded || scriptableObject == null)
            {
                return leftRect;
            }

            SerializedObject serializedObject = new SerializedObject(scriptableObject);
            serializedObject.Update();

            float usedHeight = 0f;

            Rect indentedRect;
            using (new EditorGUI.IndentLevelScope(1))
            {
                indentedRect = EditorGUI.IndentedRect(leftRect);
            }

            // _editor ??= UnityEditor.Editor.CreateEditor(scriptableObject);
            // _editor.OnInspectorGUI();

            List<(Rect, SerializedProperty)> propertyRects = new List<(Rect, SerializedProperty)>();

            using(new EditorGUI.IndentLevelScope(1))
            using(new AdaptLabelWidth())
            using(new ResetIndentScoop())
            {
                foreach (SerializedProperty childProperty in GetAllField(serializedObject))
                {
                    float childHeight = GetPropertyHeight(childProperty, new GUIContent(childProperty.displayName));
                    Rect childRect = new Rect
                    {
                        x = indentedRect.x,
                        y = indentedRect.y + usedHeight,
                        width = indentedRect.width,
                        height = childHeight,
                    };

                    // EditorGUI.PropertyField(childRect, childProperty, true);
                    propertyRects.Add((childRect, childProperty));

                    usedHeight += childHeight;
                }
            }

            GUI.Box(new Rect(leftRect)
            {
                height = usedHeight,
            }, GUIContent.none);

            foreach ((Rect childRect, SerializedProperty childProperty) in propertyRects)
            {
                EditorGUI.PropertyField(childRect, childProperty, true);
            }

            serializedObject.ApplyModifiedProperties();

            return new Rect(leftRect)
            {
                y = leftRect.y + usedHeight,
                height = leftRect.height - usedHeight,
            };
        }

        private static IEnumerable<SerializedProperty> GetAllField(SerializedObject serializedScriptableObject)
        {
            using (SerializedProperty iterator = serializedScriptableObject.GetIterator())
            {
                if (!iterator.NextVisible(true))
                {
                    yield break;
                }

                do
                {
                    SerializedProperty childProperty = serializedScriptableObject.FindProperty(iterator.name);
                    if (childProperty.name.Equals("m_Script", System.StringComparison.Ordinal))
                    {
                        continue;
                    }

                    yield return childProperty;
                } while (iterator.NextVisible(false));
            }
        }
    }
}
