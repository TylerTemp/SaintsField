using System.Collections.Generic;
using System.Reflection;
using SaintsField.Editor.Drawers.AdvancedDropdownDrawer;
using SaintsField.Editor.Utils;
using SaintsField.Interfaces;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace SaintsField.Editor.Drawers.I2Loc.LocalizedStringPickerDrawer
{
    public partial class LocalizedStringPickerAttributeDrawer
    {
        protected override float GetPostFieldWidth(Rect position, SerializedProperty property, GUIContent label, ISaintsAttribute saintsAttribute,
            int index, OnGUIPayload onGuiPayload, FieldInfo info, object parent)
        {
            return EditorGUIUtility.singleLineHeight;
        }

        private static Texture2D _iconDown;


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

        private class CachedImGui
        {
            public bool Changed;
            public object Value;
        }

        private static readonly Dictionary<string, CachedImGui> AsyncChangedCache = new Dictionary<string, CachedImGui>();

        protected override bool DrawPostFieldImGui(Rect position, Rect fullRect, SerializedProperty property, GUIContent label,
            ISaintsAttribute saintsAttribute, int index, IReadOnlyList<PropertyAttribute> allAttributes, OnGUIPayload onGUIPayload, FieldInfo info,
            object parent)
        {
            if (!_iconDown)
            {
                _iconDown = Util.LoadResource<Texture2D>("classic-dropdown-gray.png");
            }

            string key = SerializedUtils.GetUniqueId(property);
            if(!AsyncChangedCache.TryGetValue(key, out CachedImGui cachedValue))
            {
                cachedValue = AsyncChangedCache[key] = new CachedImGui();
                NoLongerInspectingWatch(property.serializedObject.targetObject, key, () =>
                {
                    AsyncChangedCache.Remove(key);
                });
            }

            if (cachedValue.Changed)
            {
                onGUIPayload.SetValue(cachedValue.Value);
                cachedValue.Changed = false;
            }

            // ReSharper disable once InvertIf
            if(GUI.Button(position, _iconDown, ButtonStyle))
            {
                string curValue = property.propertyType == SerializedPropertyType.String
                    ? property.stringValue
                    : property.FindPropertyRelative("mTerm").stringValue;
                AdvancedDropdownMetaInfo metaInfo = GetMetaInfo(curValue, false);
                Vector2 size = AdvancedDropdownUtil.GetSizeIMGUI(metaInfo.DropdownListValue, position.width);

                // OnGUIPayload targetPayload = onGUIPayload;
                SaintsAdvancedDropdownIMGUI dropdown = new SaintsAdvancedDropdownIMGUI(
                    metaInfo.DropdownListValue,
                    size,
                    position,
                    new AdvancedDropdownState(),
                    curItem =>
                    {
                        string newValue = (string)curItem;
                        SetValue(property, newValue);
                        property.serializedObject.ApplyModifiedProperties();
                        if(property.propertyType == SerializedPropertyType.String)
                        {
                            cachedValue.Changed = true;
                            cachedValue.Value = newValue;
                            return;
                        }

                        object noCacheParent = SerializedUtils.GetFieldInfoAndDirectParent(property).parent;
                        if (noCacheParent == null)
                        {
                            Debug.LogWarning("Property disposed unexpectedly, skip onChange callback.");
                            return;
                        }

                        (string error, int _, object reflectedValue) = Util.GetValue(property, info, noCacheParent);
                        if (error != "")
                        {
                            Debug.LogError(error);
                            return;
                        }
                        cachedValue.Changed = true;
                        cachedValue.Value = reflectedValue;
                    },
                    _ => null);
                dropdown.Show(position);
                dropdown.BindWindowPosition();
            }
            return true;
        }

        protected override bool WillDrawBelow(SerializedProperty property, ISaintsAttribute saintsAttribute, int index, FieldInfo info,
            object parent)
        {
            return MismatchError(property) != "";
        }

        protected override float GetBelowExtraHeight(SerializedProperty property, GUIContent label, float width, ISaintsAttribute saintsAttribute,
            int index, FieldInfo info, object parent)
        {
            string error = MismatchError(property);
            return error == "" ? 0 : ImGuiHelpBox.GetHeight(error, width, MessageType.Error);
        }

        protected override Rect DrawBelow(Rect position, SerializedProperty property, GUIContent label, ISaintsAttribute saintsAttribute,
            int index, IReadOnlyList<PropertyAttribute> allAttributes, OnGUIPayload onGuiPayload, FieldInfo info, object parent)
        {
            string error = MismatchError(property);
            return error == ""? position : ImGuiHelpBox.Draw(position, error, MessageType.Error);
        }
    }
}
