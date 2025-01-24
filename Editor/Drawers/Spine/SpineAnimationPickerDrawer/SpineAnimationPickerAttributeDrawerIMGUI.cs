using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using SaintsField.Editor.Core;
using SaintsField.Editor.Drawers.AdvancedDropdownDrawer;
using SaintsField.Editor.Utils;
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

        private static Texture2D _iconIMGUI;

        private static Texture2D IconIMGUI
        {
            get
            {
                if(_iconIMGUI == null)
                {
                    _iconIMGUI = EditorGUIUtility.Load("Spine/icon-animation.png") as Texture2D;
                }

                return _iconIMGUI;
            }
        }

        protected override float GetPostFieldWidth(Rect position, SerializedProperty property, GUIContent label,
            ISaintsAttribute saintsAttribute, int index, OnGUIPayload onGuiPayload, FieldInfo info, object parent)
        {
            return EditorGUIUtility.singleLineHeight;
        }

        protected override bool DrawPostFieldImGui(Rect position, SerializedProperty property, GUIContent label,
            ISaintsAttribute saintsAttribute, int index, IReadOnlyList<PropertyAttribute> allAttributes,
            OnGUIPayload onGUIPayload, FieldInfo info, object parent)
        {
            if(!CachedInfoIMGUI.TryGetValue(property.propertyPath, out CachedInfo cachedInfo))
            {
                return false;
            }

            if (cachedInfo.Changed)
            {
                onGUIPayload.SetValue(cachedInfo.ChangedValue);
                cachedInfo.Changed = false;
            }

            if(GUI.Button(position, IconIMGUI, EditorStyles.miniButton))
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

                Vector2 size = AdvancedDropdownUtil.GetSizeIMGUI(dropdownMetaInfo.DropdownListValue, position.width);

                // OnGUIPayload targetPayload = onGUIPayload;
                SaintsAdvancedDropdownIMGUI dropdown = new SaintsAdvancedDropdownIMGUI(
                    dropdownMetaInfo.DropdownListValue,
                    size,
                    position,
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
                dropdown.Show(position);
                dropdown.BindWindowPosition();
            }
            return true;
        }

        protected override bool WillDrawBelow(SerializedProperty property, ISaintsAttribute saintsAttribute,
            int index,
            FieldInfo info,
            object parent)
        {
            if(!CachedInfoIMGUI.TryGetValue(property.propertyPath, out CachedInfo cachedInfo))
            {
                string dismatchError = GetTypeMismatchError(property, info);
                string key = SerializedUtils.GetUniqueId(property);
                CachedInfoIMGUI[key] = cachedInfo = new CachedInfo
                {
                    Error = dismatchError,
                };

                NoLongerInspectingWatch(property.serializedObject.targetObject, () =>
                {
                    CachedInfoIMGUI.Remove(key);
                });
            }

            return cachedInfo.Error != "";
        }

        protected override float GetBelowExtraHeight(SerializedProperty property, GUIContent label, float width,
            ISaintsAttribute saintsAttribute, int index, FieldInfo info, object parent)
        {
            if(!CachedInfoIMGUI.TryGetValue(property.propertyPath, out CachedInfo cachedInfo) || cachedInfo.Error == "")
            {
                return 0f;
            }

            return ImGuiHelpBox.GetHeight(cachedInfo.Error, width, MessageType.Error);
        }

        protected override Rect DrawBelow(Rect position, SerializedProperty property, GUIContent label,
            ISaintsAttribute saintsAttribute, int index, IReadOnlyList<PropertyAttribute> allAttributes,
            OnGUIPayload onGuiPayload, FieldInfo info, object parent)
        {
            if(!CachedInfoIMGUI.TryGetValue(property.propertyPath, out CachedInfo cachedInfo) || cachedInfo.Error == "")
            {
                return position;
            }

            return ImGuiHelpBox.Draw(position, cachedInfo.Error, MessageType.Error);
        }
    }
}
