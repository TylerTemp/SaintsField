using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using SaintsField.Interfaces;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEngine;

namespace SaintsField.Editor.Drawers.Addressable.AddressableLabelDrawer
{
    public partial class AddressableLabelAttributeDrawer
    {
        protected override float GetFieldHeight(SerializedProperty property, GUIContent label,
            float width,
            ISaintsAttribute saintsAttribute,
            FieldInfo info,
            bool hasLabelWidth, object parent) => EditorGUIUtility.singleLineHeight;

        protected override void DrawField(Rect position, SerializedProperty property, GUIContent label,
            ISaintsAttribute saintsAttribute, IReadOnlyList<PropertyAttribute> allAttributes, OnGUIPayload onGUIPayload,
            FieldInfo info, object parent)
        {
            // ReSharper disable once Unity.NoNullPropagation
            List<string> labels = AddressableAssetSettingsDefaultObject.Settings?.GetLabels() ?? new List<string>();

            GUIContent[] contents = labels
                .Select(each => new GUIContent(each.Replace('/', '\u2215').Replace('&', '＆')))
                .Concat(new []
                {
                    GUIContent.none,
                    new GUIContent("Edit Addressable Group..."),
                })
                .ToArray();

            // ReSharper disable once ConvertToUsingDeclaration
            using(EditorGUI.ChangeCheckScope changed = new EditorGUI.ChangeCheckScope())
            {
                int index = labels.IndexOf(property.stringValue);
                int newIndex = EditorGUI.Popup(position, label, index, contents);
                // ReSharper disable once InvertIf
                if(changed.changed)
                {
                    if (newIndex < labels.Count)
                    {
                        property.stringValue = labels[newIndex];
                    }
                    else
                    {
                        AddressableUtil.OpenLabelEditor();
                    }
                }
            }
        }
    }
}
