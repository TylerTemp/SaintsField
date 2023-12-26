#if SAINTSFIELD_ADDRESSABLE && !SAINTSFIELD_ADDRESSABLE_DISABLE
using System.Collections.Generic;
using System.Linq;
using SaintsField.Addressable;
using SaintsField.Editor.Core;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEngine;

namespace SaintsField.Editor.Drawers.Addressable
{
    [CustomPropertyDrawer(typeof(AddressableLabelAttribute))]
    public class AddressableLabelAttributeDrawer: SaintsPropertyDrawer
    {
        protected override float GetFieldHeight(SerializedProperty property, GUIContent label, ISaintsAttribute saintsAttribute,
            bool hasLabelWidth) => EditorGUIUtility.singleLineHeight;

        protected override void DrawField(Rect position, SerializedProperty property, GUIContent label, ISaintsAttribute saintsAttribute)
        {
            // Debug.Log(AddressableAssetSettingsDefaultObject.Settings);
            // ReSharper disable once Unity.NoNullPropagation
            List<string> labels = AddressableAssetSettingsDefaultObject.Settings?.GetLabels() ?? new List<string>();

            // ReSharper disable once ConvertToUsingDeclaration
            using(EditorGUI.ChangeCheckScope changed = new EditorGUI.ChangeCheckScope())
            {
                int index = labels.IndexOf(property.stringValue);
                int newIndex = EditorGUI.Popup(position, label, index, labels.Select(each => new GUIContent(each)).ToArray());
                if (changed.changed)
                {
                    property.stringValue = labels[newIndex];
                }
            }
        }
    }
}
#endif
