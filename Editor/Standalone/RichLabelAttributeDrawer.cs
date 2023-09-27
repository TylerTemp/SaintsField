using ExtInspector.Editor.Utils;
using ExtInspector.Standalone;
using UnityEditor;
using UnityEngine;

namespace ExtInspector.Editor.Standalone
{
    [CustomPropertyDrawer(typeof(RichLabelAttribute))]
    public class RichLabelAttributeDrawer: PropertyDrawer
    {
        private readonly RichTextDrawer _richTextDrawer = new RichTextDrawer();
        // private IReadOnlyList<RichText.RichTextPayload> _cachedResult = null;

        ~RichLabelAttributeDrawer()
        {
            _richTextDrawer.Dispose();
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            label = EditorGUI.BeginProperty(position, label, property);

            RichLabelAttribute targetAttribute = (RichLabelAttribute)attribute;

            (Rect labelRect, Rect propertyRect) =
                RectUtils.SplitWidthRect(EditorGUI.IndentedRect(position), EditorGUIUtility.labelWidth);

            string labelXml = targetAttribute.RichTextXml;
// #if EXT_INSPECTOR_LOG
//             Debug.Log($"RichLabelAttributeDrawer: {labelXml}");
// #endif

            _richTextDrawer.DrawChunks(labelRect, label, RichTextDrawer.ParseRichXml(labelXml, label.text));

            EditorGUI.PropertyField(propertyRect, property, GUIContent.none);

            EditorGUI.EndProperty();
        }
    }
}
