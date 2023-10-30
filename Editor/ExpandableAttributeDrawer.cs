using ExtInspector.Editor.Standalone;
using ExtInspector.Editor.Utils;
using UnityEditor;
using UnityEngine;

namespace ExtInspector.Editor
{
    [CustomPropertyDrawer(typeof(ExpandableAttribute))]
    public class ExpandableAttributeDrawer: DecToggleAttributeDrawer
    {
        private string _error = "";

        private const string ExpandedXml = "<icon=caret-down-solid.png />";
        private const string UnExpandedXml = "<icon=caret-right-solid.png />";

        private bool _expanded = true;

        private float _width = -1;

        private UnityEditor.Editor _editor;

        // private float GetWidth(Rect position)
        // {
        //     if (_width >= 0)
        //     {
        //         return _width;
        //     }
        //     float xmlWidth = RichTextDrawer.GetWidth(new GUIContent(), position.height, RichTextDrawer.ParseRichXml(UnExpandedXml, ""));
        //     if (xmlWidth > 0)
        //     {
        //         return _width = xmlWidth;
        //     }
        //
        //     return position.height;
        // }

        ~ExpandableAttributeDrawer()
        {
            if (_editor)
            {
                Object.DestroyImmediate(_editor);
            }
        }

        protected override (bool isActive, Rect position) DrawPreLabel(Rect position, SerializedProperty property, GUIContent label, ISaintsAttribute saintsAttribute)
        {
            // Debug.Log("DrawPreLabel!!!!!");
            // float width = GetWidth(position);

            GUIStyle style = new GUIStyle(EditorStyles.foldout);
            float foldoutWidth = style.CalcSize(GUIContent.none).x;

            (Rect foldoutRect, Rect leftRect) = RectUtils.SplitWidthRect(position, foldoutWidth);
            _expanded = EditorGUI.Foldout(foldoutRect, _expanded, GUIContent.none, true, style);

            // Draw(useRect, property, label, _expanded? ExpandedXml: UnExpandedXml, _expanded, (newIsActive) => _expanded = newIsActive);

            return (true, leftRect);
        }

        protected override bool WillDrawBelow(Rect position, SerializedProperty property, GUIContent label, ISaintsAttribute saintsAttribute)
        {
            return true;
        }

        protected override float GetBelowExtraHeight(SerializedProperty property, GUIContent label, float width, ISaintsAttribute saintsAttribute)
        {
            return _expanded ? 100 : 0;
        }

        protected override Rect DrawBelow(Rect position, SerializedProperty property, GUIContent label, ISaintsAttribute saintsAttribute)
        {
            Debug.Log($"DRAW BELOW {position}");

           ScriptableObject scriptableObject = property.objectReferenceValue as ScriptableObject;
            // if (!_expanded || scriptableObject == null)
            // {
            //     return position;
            // }

            GUI.Box(position, GUIContent.none);

            // _editor ??= UnityEditor.Editor.CreateEditor(scriptableObject);
            // _editor.OnInspectorGUI();

            SerializedObject serializedObject = new SerializedObject(scriptableObject);
            serializedObject.Update();

            using (var iterator = serializedObject.GetIterator())
            {
                float yOffset = EditorGUIUtility.singleLineHeight;
                Debug.Log(yOffset);

                if (iterator.NextVisible(true))
                {
                    do
                    {
                        SerializedProperty childProperty = serializedObject.FindProperty(iterator.name);
                        if (childProperty.name.Equals("m_Script", System.StringComparison.Ordinal))
                        {
                            continue;
                        }

                        float childHeight = GetPropertyHeight(childProperty, new GUIContent(childProperty.displayName));
                        Rect childRect = new Rect()
                        {
                            x = position.x,
                            y = position.y + yOffset,
                            width = position.width,
                            height = childHeight,
                        };

                        // NaughtyEditorGUI.PropertyField(childRect, childProperty, true);
                        EditorGUI.PropertyField(childRect, childProperty, true);

                        yOffset += childHeight;
                    }
                    while (iterator.NextVisible(false));
                }
            }

            serializedObject.ApplyModifiedProperties();

            return position;
        }

    }
}
