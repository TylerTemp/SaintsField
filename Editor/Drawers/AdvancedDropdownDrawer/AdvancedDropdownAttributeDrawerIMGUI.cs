using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using SaintsField.DropdownBase;
using SaintsField.Editor.Utils;
using SaintsField.Interfaces;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace SaintsField.Editor.Drawers.AdvancedDropdownDrawer
{
    public partial class AdvancedDropdownAttributeDrawer
    {

        private string _error = "";

        private readonly Dictionary<string, Texture2D> _iconCache = new Dictionary<string, Texture2D>();

        ~AdvancedDropdownAttributeDrawer()
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

        private static string GetKey(SerializedProperty property) => SerializedUtils.GetUniqueId(property);

        private static readonly Dictionary<string, object> AsyncChangedCache = new Dictionary<string, object>();

        protected override void DrawField(Rect position, SerializedProperty property, GUIContent label,
            ISaintsAttribute saintsAttribute, IReadOnlyList<PropertyAttribute> allAttributes, OnGUIPayload onGUIPayload,
            FieldInfo info, object parent)
        {
            string key = GetKey(property);
            if (AsyncChangedCache.TryGetValue(key, out object changedValue))
            {
                onGUIPayload.SetValue(changedValue);
                AsyncChangedCache.Remove(key);
            }

            AdvancedDropdownAttribute advancedDropdownAttribute = (AdvancedDropdownAttribute)saintsAttribute;
            AdvancedDropdownMetaInfo metaInfo = GetMetaInfo(property, advancedDropdownAttribute, info, parent, true);
            _error = metaInfo.Error;

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
                        AsyncChangedCache[key] = curItem;
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

            int count = 0;
            foreach (IAdvancedDropdownList child in dropdownList.children)
            {
                count += GetValueItemCounts(child);
            }

            return count;

            // if(dropdownList.ChildCount() == 0)
            // {
            //     Debug.Log(1);
            //     yield return 1;
            //     yield break;
            // }
            //
            // // Debug.Log(dropdownList.ChildCount());
            // // yield return dropdownList.children.Count(each => each.ChildCount() == 0);
            // foreach (IAdvancedDropdownList eachChild in dropdownList.children)
            // {
            //     foreach (int subChildCount in GetChildCounts(eachChild))
            //     {
            //         if(subChildCount > 0)
            //         {
            //             Debug.Log(subChildCount);
            //             yield return subChildCount;
            //         }
            //     }
            // }
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
