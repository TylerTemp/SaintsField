using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using SaintsField.DropdownBase;
using SaintsField.Editor.Drawers.AdvancedDropdownDrawer;
using SaintsField.Editor.Drawers.ExpandableDrawer;
using SaintsField.Editor.Utils;
using SaintsField.Interfaces;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace SaintsField.Editor.Drawers.EnumFlagsDrawers
{
    public partial class FlagsDropdownAttributeDrawer
    {
        private string _error = "";

        private readonly Dictionary<string, Texture2D> _iconCache = new Dictionary<string, Texture2D>();

        ~FlagsDropdownAttributeDrawer()
        {
            _iconCache.Clear();
        }

        protected override float GetFieldHeight(SerializedProperty property, GUIContent label,
            float width,
            ISaintsAttribute saintsAttribute,
            FieldInfo info,
            bool hasLabelWidth, object parent)
        {
            return EditorGUIUtility.singleLineHeight;
        }

        protected override void DrawField(Rect position, SerializedProperty property, GUIContent label,
            ISaintsAttribute saintsAttribute, IReadOnlyList<PropertyAttribute> allAttributes, OnGUIPayload onGUIPayload,
            FieldInfo info, object parent)
        {
            EnumFlagsMetaInfo metaInfo = EnumFlagsUtil.GetMetaInfo(property, info);
            AdvancedDropdownMetaInfo dropdownMetaInfo = GetMetaInfo(property.intValue, metaInfo.AllCheckedInt, metaInfo.BitValueToName);
            _error = dropdownMetaInfo.Error;

            #region Dropdown

            Rect leftRect = EditorGUI.PrefixLabel(position, label);

            GUI.SetNextControlName(FieldControlName);
            string display = GetSelectedNames(metaInfo.BitValueToName, property.intValue);
            // Debug.Assert(false, "Here");
            // ReSharper disable once InvertIf
            if (EditorGUI.DropdownButton(leftRect, new GUIContent(display), FocusType.Keyboard))
            {
                float minHeight = AdvancedDropdownAttribute.MinHeight;
                float itemHeight = EditorGUIUtility.singleLineHeight;
                float titleHeight = AdvancedDropdownAttribute.TitleHeight;
                Vector2 size;
                if (minHeight < 0)
                {
                    // if(AdvancedDropdownAttribute.UseTotalItemCount)
                    // {
                    float totalItemCount = GetValueItemCounts(dropdownMetaInfo.DropdownListValue);
                    // Debug.Log(totalItemCount);
                    size = new Vector2(position.width, totalItemCount * itemHeight + titleHeight);
                    // }
                    // else
                    // {
                    //     float maxChildCount = GetDropdownPageHeight(metaInfo.DropdownListValue, itemHeight, advancedDropdownAttribute.SepHeight).Max();
                    //     size = new Vector2(position.width, maxChildCount + titleHeight);
                    // }
                }
                else
                {
                    size = new Vector2(position.width, minHeight);
                }

                // Vector2 size = new Vector2(position.width, maxChildCount * EditorGUIUtility.singleLineHeight + 31f);
                SaintsAdvancedDropdownIMGUI dropdown = new SaintsAdvancedDropdownIMGUI(
                    dropdownMetaInfo.DropdownListValue,
                    size,
                    position,
                    new AdvancedDropdownState(),
                    curItem =>
                    {
                        int selectedValue = (int)curItem;
                        int newMask = selectedValue == 0
                            ? 0
                            : EnumFlagsUtil.ToggleBit(property.intValue, selectedValue);

                        ReflectUtils.SetValue(property.propertyPath, property.serializedObject.targetObject, info, parent, newMask);
                        Util.SignPropertyValue(property, info, parent, newMask);
                        property.serializedObject.ApplyModifiedProperties();
                        onGUIPayload.SetValue(newMask);
                        if(ExpandableIMGUIScoop.IsInScoop)
                        {
                            property.serializedObject.ApplyModifiedProperties();
                        }
                    },
                    GetIcon);
                dropdown.Show(position);
                dropdown.BindWindowPosition();
            }

            #endregion
        }

        private static int GetValueItemCounts(IAdvancedDropdownList dropdownList)
        {
            if (dropdownList.isSeparator)
            {
                return 0;
            }

            if(dropdownList.ChildCount() == 0)
            {
                return 1;
            }

            return dropdownList.children.Sum(child => GetValueItemCounts(child));
        }

        private Texture2D GetIcon(string icon)
        {
            if (_iconCache.TryGetValue(icon, out Texture2D result))
            {
                return result;
            }

            result = Util.LoadResource<Texture2D>(icon);
            if (result == null)
            {
                return null;
            }
            if (result.width == 1 && result.height == 1)
            {
                return null;
            }
            _iconCache[icon] = result;
            return result;
        }

        protected override bool WillDrawBelow(SerializedProperty property,
            IReadOnlyList<PropertyAttribute> allAttributes, ISaintsAttribute saintsAttribute,
            int index,
            FieldInfo info,
            object parent) => _error != "";

        protected override float GetBelowExtraHeight(SerializedProperty property, GUIContent label, float width,
            IReadOnlyList<PropertyAttribute> allAttributes,
            ISaintsAttribute saintsAttribute, int index, FieldInfo info, object parent) => _error == "" ? 0 : ImGuiHelpBox.GetHeight(_error, width, MessageType.Error);

        protected override Rect DrawBelow(Rect position, SerializedProperty property, GUIContent label,
            ISaintsAttribute saintsAttribute, int index, IReadOnlyList<PropertyAttribute> allAttributes,
            OnGUIPayload onGuiPayload, FieldInfo info, object parent) => _error == "" ? position : ImGuiHelpBox.Draw(position, _error, MessageType.Error);

    }
}
