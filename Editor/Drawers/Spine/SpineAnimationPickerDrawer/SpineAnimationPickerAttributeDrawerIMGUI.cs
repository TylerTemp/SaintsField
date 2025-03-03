using System.Collections.Generic;
using System.Reflection;
using SaintsField.Editor.Drawers.AdvancedDropdownDrawer;
using SaintsField.Editor.Utils;
using SaintsField.Interfaces;
using SaintsField.Spine;
using Spine.Unity;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace SaintsField.Editor.Drawers.Spine.SpineAnimationPickerDrawer
{
    public partial class SpineAnimationPickerAttributeDrawer
    {
        private class CachedInfo
        {
            public string Error = "";
            public bool Changed = false;
            public object ChangedValue = null;
        }

        private static readonly Dictionary<string, CachedInfo> CachedInfoIMGUI = new Dictionary<string, CachedInfo>();

        private static Texture2D _iconDropdownIMGUI;

        private static Texture2D IconDropdownIMGUI
        {
            get
            {
                if(_iconDropdownIMGUI == null)
                {
                    _iconDropdownIMGUI = Util.LoadResource<Texture2D>(IconDropdownPath);
                }

                return _iconDropdownIMGUI;
            }
        }

        private static Texture2D _iconIMGUI;
        private static Texture2D IconIMGUI
        {
            get
            {
                if(_iconIMGUI == null)
                {
                    _iconIMGUI = Util.LoadResource<Texture2D>(IconPath);
                }

                return _iconIMGUI;
            }
        }

        protected override float GetPostFieldWidth(Rect position, SerializedProperty property, GUIContent label,
            ISaintsAttribute saintsAttribute, int index, OnGUIPayload onGuiPayload, FieldInfo info, object parent)
        {
            return EditorGUIUtility.singleLineHeight;
        }

        private static GUIStyle _buttonStyle;
        private static GUIStyle ButtonStyle
        {
            get
            {
                // ReSharper disable once ConvertIfStatementToNullCoalescingExpression
                if (_buttonStyle == null)
                {
                    _buttonStyle = new GUIStyle(EditorStyles.miniButton)
                    {
                        padding = new RectOffset(3, 3, 3, 3),
                    };
                }

                return _buttonStyle;
            }
        }

        protected override bool DrawPostFieldImGui(Rect position, Rect fullRect, SerializedProperty property,
            GUIContent label,
            ISaintsAttribute saintsAttribute, int index, IReadOnlyList<PropertyAttribute> allAttributes,
            OnGUIPayload onGUIPayload, FieldInfo info, object parent)
        {
            if(!CachedInfoIMGUI.TryGetValue(SerializedUtils.GetUniqueId(property), out CachedInfo cachedInfo))
            {

                return false;
            }

            if (cachedInfo.Changed)
            {
                onGUIPayload.SetValue(cachedInfo.ChangedValue);
                cachedInfo.Changed = false;
            }

            if(GUI.Button(position, IconDropdownIMGUI, ButtonStyle))
            {
                SpineAnimationPickerAttribute spineAnimationPickerAttribute = (SpineAnimationPickerAttribute)saintsAttribute;
                (string error, SkeletonDataAsset skeletonDataAsset) = SpineUtils.GetSkeletonDataAsset(spineAnimationPickerAttribute.SkeletonTarget, property, info, parent);
                if (error != "")
                {
#if SAINTSFIELD_DEBUG
                    Debug.LogError(error);
#endif
                    cachedInfo.Error = error;
                    return true;
                }

                AdvancedDropdownMetaInfo dropdownMetaInfo = property.propertyType == SerializedPropertyType.String
                    ? GetMetaInfoString(property.stringValue, skeletonDataAsset)
                    : GetMetaInfoAsset(property.objectReferenceValue as AnimationReferenceAsset, skeletonDataAsset);

                Vector2 size = AdvancedDropdownUtil.GetSizeIMGUI(dropdownMetaInfo.DropdownListValue, fullRect.width);

                // OnGUIPayload targetPayload = onGUIPayload;
                SaintsAdvancedDropdownIMGUI dropdown = new SaintsAdvancedDropdownIMGUI(
                    dropdownMetaInfo.DropdownListValue,
                    size,
                    fullRect,
                    new AdvancedDropdownState(),
                    curItem =>
                    {
                        // ReSharper disable once ConvertIfStatementToSwitchStatement
                        if (property.propertyType == SerializedPropertyType.String)
                        {
                            string curValue = (string)curItem;
                            property.stringValue = curValue;
                            property.serializedObject.ApplyModifiedProperties();
                            // onValueChangedCallback(curValue);
                            cachedInfo.Changed = true;
                            cachedInfo.ChangedValue = curValue;
                        }
                        else
                        {
                            AnimationReferenceAsset curValue = (AnimationReferenceAsset)curItem;
                            property.objectReferenceValue = curValue;
                            property.serializedObject.ApplyModifiedProperties();
                            // onValueChangedCallback(curValue);
                            cachedInfo.Changed = true;
                            cachedInfo.ChangedValue = curValue;
                        }
                    },
                    _ => IconIMGUI);
                dropdown.Show(fullRect);
                dropdown.BindWindowPosition();
            }
            return true;
        }

        protected override bool WillDrawBelow(SerializedProperty property, ISaintsAttribute saintsAttribute,
            int index,
            FieldInfo info,
            object parent)
        {
            string key = SerializedUtils.GetUniqueId(property);
            if(!CachedInfoIMGUI.TryGetValue(key, out CachedInfo cachedInfo))
            {
                string mismatchError = GetTypeMismatchError(property, info);

                CachedInfoIMGUI[key] = cachedInfo = new CachedInfo
                {
                    Error = mismatchError,
                };

                NoLongerInspectingWatch(property.serializedObject.targetObject, key, () =>
                {
                    CachedInfoIMGUI.Remove(key);
                });
            }

            return cachedInfo.Error != "";
        }

        protected override float GetBelowExtraHeight(SerializedProperty property, GUIContent label, float width,
            ISaintsAttribute saintsAttribute, int index, FieldInfo info, object parent)
        {
            if(!CachedInfoIMGUI.TryGetValue(SerializedUtils.GetUniqueId(property), out CachedInfo cachedInfo) || cachedInfo.Error == "")
            {
                return 0f;
            }

            return ImGuiHelpBox.GetHeight(cachedInfo.Error, width, MessageType.Error);
        }

        protected override Rect DrawBelow(Rect position, SerializedProperty property, GUIContent label,
            ISaintsAttribute saintsAttribute, int index, IReadOnlyList<PropertyAttribute> allAttributes,
            OnGUIPayload onGuiPayload, FieldInfo info, object parent)
        {
            if(!CachedInfoIMGUI.TryGetValue(SerializedUtils.GetUniqueId(property), out CachedInfo cachedInfo) || cachedInfo.Error == "")
            {
                return position;
            }

            return ImGuiHelpBox.Draw(position, cachedInfo.Error, MessageType.Error);
        }
    }
}
