using ExtInspector.Standalone;
using UnityEditor;
using UnityEngine;

namespace ExtInspector.Editor.Standalone
{
    [CustomPropertyDrawer(typeof(RichLabelAttribute))]
    public class RichLabelAttributeDrawer: SaintsPropertyDrawer
    {
        private readonly RichTextDrawer _richTextDrawer = new RichTextDrawer();
        // private IReadOnlyList<RichText.RichTextPayload> _cachedResult = null;

        ~RichLabelAttributeDrawer()
        {
            _richTextDrawer.Dispose();
        }

        protected override float GetLabelFieldHeight(SerializedProperty property, GUIContent label,
            ISaintsAttribute saintsAttribute)
        {
            RichLabelAttribute targetAttribute = (RichLabelAttribute)saintsAttribute;
            return targetAttribute.RichTextXml is null
                ? 0
                : base.GetPropertyHeight(property, label);
        }

        protected override bool DrawLabel(Rect position, SerializedProperty property, GUIContent label, ISaintsAttribute saintsAttribute)
        {
            // label = EditorGUI.BeginProperty(position, label, property);

            RichLabelAttribute targetAttribute = (RichLabelAttribute)saintsAttribute;

            // (Rect labelRect, Rect propertyRect) =
            //     RectUtils.SplitWidthRect(EditorGUI.IndentedRect(position), EditorGUIUtility.labelWidth);

            string labelXml = targetAttribute.RichTextXml;

            if (labelXml is null)
            {
                return false;
            }

// #if EXT_INSPECTOR_LOG
//             Debug.Log($"RichLabelAttributeDrawer: {labelXml}");
// #endif

            _richTextDrawer.DrawChunks(position, label, RichTextDrawer.ParseRichXml(labelXml, label.text));

            // EditorGUI.PropertyField(propertyRect, property, GUIContent.none);

            // EditorGUI.EndProperty();
            return true;
        }
    }
}
