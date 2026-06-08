using System.Collections.Generic;
using System.Reflection;
using SaintsField.Editor.Utils;
using SaintsField.Interfaces;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace SaintsField.Editor.Drawers.AdvancedDropdownDrawer
{
    public partial class AdvancedDropdownAttributeDrawer
    {
        private class InfoIMGUI
        {
            public string Error = "";
        }

        private static readonly Dictionary<string, InfoIMGUI> InfoCacheIMGUI = new Dictionary<string, InfoIMGUI>();

        private readonly Dictionary<string, Texture2D> _iconCache = new Dictionary<string, Texture2D>();

        ~AdvancedDropdownAttributeDrawer()
        {
            _iconCache.Clear();
        }

        private static InfoIMGUI EnsureKey(SerializedProperty property)
        {
            string key = SerializedUtils.GetUniqueId(property);
            if (InfoCacheIMGUI.TryGetValue(key, out InfoIMGUI infoCache))
            {
                return infoCache;
            }

            InfoCacheIMGUI[key] = infoCache = new InfoIMGUI();
            NoLongerInspectingWatch(property.serializedObject.targetObject, key, () =>
            {
                InfoCacheIMGUI.Remove(key);
            });
            return infoCache;
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
            ISaintsAttribute saintsAttribute, IReadOnlyList<PropertyAttribute> allAttributes,
            FieldInfo info, object parent)
        {
            InfoIMGUI cachedInfo = EnsureKey(property);
            AdvancedDropdownAttribute advancedDropdownAttribute = (AdvancedDropdownAttribute)saintsAttribute;
            AdvancedDropdownMetaInfo metaInfo = GetMetaInfo(property, advancedDropdownAttribute, info, parent, true);
            cachedInfo.Error = metaInfo.Error;

            #region Dropdown

            Rect leftRect = EditorGUI.PrefixLabel(position, label);

            GUI.SetNextControlName(FieldControlName);
            string display = GetMetaStackDisplay(metaInfo);
            // Debug.Assert(false, "Here");
            // ReSharper disable once InvertIf
            if (EditorGUI.DropdownButton(leftRect, new GUIContent(display), FocusType.Keyboard))
            {
                // float minHeight = AdvancedDropdownAttribute.MinHeight;
                // float itemHeight = AdvancedDropdownAttribute.ItemHeight > 0
                //     ? AdvancedDropdownAttribute.ItemHeight
                //     : EditorGUIUtility.singleLineHeight;
                // float titleHeight = AdvancedDropdownAttribute.TitleHeight;
                // Vector2 size;
                // if (minHeight < 0)
                // {
                //     if(AdvancedDropdownAttribute.UseTotalItemCount)
                //     {
                //         float totalItemCount = GetValueItemCounts(metaInfo.DropdownListValue);
                //         // Debug.Log(totalItemCount);
                //         size = new Vector2(position.width, totalItemCount * itemHeight + titleHeight);
                //     }
                //     else
                //     {
                //         float maxChildCount = AdvancedDropdownUtil.GetDropdownPageHeight(metaInfo.DropdownListValue, itemHeight, AdvancedDropdownAttribute.SepHeight).Max();
                //         size = new Vector2(position.width, maxChildCount + titleHeight);
                //     }
                // }
                // else
                // {
                //     size = new Vector2(position.width, minHeight);
                // }

                Vector2 size = AdvancedDropdownUtil.GetSizeIMGUI(metaInfo.DropdownListValue, position.width);

                // OnGUIPayload targetPayload = onGUIPayload;
                SaintsAdvancedDropdownIMGUI dropdown = new SaintsAdvancedDropdownIMGUI(
                    metaInfo.DropdownListValue,
                    size,
                    position,
                    new AdvancedDropdownState(),
                    curItem =>
                    {
                        ReflectUtils.SetValue(property.propertyPath, property.serializedObject.targetObject, info, parent, curItem);
                        Util.SignPropertyValue(property, info, parent, curItem);
                        property.serializedObject.ApplyModifiedProperties();

                        TriggerChangedIMGUI(property, curItem);

                        // AsyncChangedCache[key] = curItem;
                        // Debug.Log($"Advanced Changed: {AsyncChangedCache[key].changed}/{AsyncChangedCache[key].GetHashCode()}");
                        // if(ExpandableIMGUIScoop.IsInScoop)
                        // {
                        //     property.serializedObject.ApplyModifiedProperties();
                        // }
                    },
                    GetIcon);
                dropdown.Show(position);
                dropdown.BindWindowPosition();
            }

            #endregion
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
            object parent) => EnsureKey(property).Error != "";

        protected override float GetBelowExtraHeight(SerializedProperty property, GUIContent label, float width,
            IReadOnlyList<PropertyAttribute> allAttributes,
            ISaintsAttribute saintsAttribute, int index, FieldInfo info, object parent)
        {
            InfoIMGUI cachedInfo = EnsureKey(property);
            return cachedInfo.Error == "" ? 0 : ImGuiHelpBox.GetHeight(cachedInfo.Error, width, MessageType.Error);
        }

        protected override Rect DrawBelow(Rect position, SerializedProperty property, GUIContent label,
            ISaintsAttribute saintsAttribute, int index, IReadOnlyList<PropertyAttribute> allAttributes,
            FieldInfo info, object parent)
        {
            InfoIMGUI cachedInfo = EnsureKey(property);
            return cachedInfo.Error == "" ? position : ImGuiHelpBox.Draw(position, cachedInfo.Error, MessageType.Error);
        }
    }
}
