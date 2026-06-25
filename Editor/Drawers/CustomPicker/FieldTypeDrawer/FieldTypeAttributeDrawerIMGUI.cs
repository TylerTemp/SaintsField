using System;
using System.Collections.Generic;
using System.Reflection;
using SaintsField.Editor.Utils;
using SaintsField.Interfaces;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace SaintsField.Editor.Drawers.CustomPicker.FieldTypeDrawer
{
    public partial class FieldTypeAttributeDrawer
    {
        private static Texture2D _pickIcon;

        private sealed class InfoIMGUI
        {
            public string Error = "";
        }

        private static readonly Dictionary<string, InfoIMGUI> InfoCacheIMGUI = new Dictionary<string, InfoIMGUI>();

        private static InfoIMGUI EnsureKey(SerializedProperty property)
        {
            string key = SerializedUtils.GetUniqueId(property);
            if (InfoCacheIMGUI.TryGetValue(key, out InfoIMGUI cache))
            {
                return cache;
            }

            InfoCacheIMGUI[key] = cache = new InfoIMGUI();
            NoLongerInspectingWatch(property.serializedObject.targetObject, key, () => InfoCacheIMGUI.Remove(key));
            return cache;
        }

        protected override float GetFieldHeight(SerializedProperty property, GUIContent label,
            float width,
            int index,
            ISaintsAttribute saintsAttribute, FieldInfo info, bool hasLabelWidth, object parent) => EditorGUIUtility.singleLineHeight;

        protected override void DrawField(Rect position, SerializedProperty property, GUIContent label,
            ISaintsAttribute saintsAttribute, IReadOnlyList<PropertyAttribute> allAttributes,
            FieldInfo info, object parent)
        {
            InfoIMGUI cache = EnsureKey(property);
            FieldTypeAttribute fieldTypeAttribute = (FieldTypeAttribute)saintsAttribute;
            Type fieldType = SerializedUtils.IsArrayOrDirectlyInsideArray(property)? ReflectUtils.GetElementType(info.FieldType): info.FieldType;
            Type requiredComp = fieldTypeAttribute.CompType ?? fieldType;
            Object requiredValue;
            try
            {
                requiredValue = GetValue(property, fieldType, requiredComp);
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                cache.Error = e.Message;
                RawDefaultDrawer(position, property, allAttributes, label, info);
                DrawOverrideRichText(position, label, overrideRichTextChunks);
                return;
            }

            cache.Error = property.objectReferenceValue != null && requiredValue == null
                ? $"{property.objectReferenceValue} has no component {fieldType}"
                : "";

            EPick editorPick = fieldTypeAttribute.EditorPick;
            bool customPicker = fieldTypeAttribute.CustomPicker;

            using (EditorGUI.ChangeCheckScope changed = new EditorGUI.ChangeCheckScope())
            {
                Rect fieldRect = customPicker
                    ? new Rect(position)
                    {
                        width = position.width - 20,
                    }
                    : position;

                Object fieldResult =
                    EditorGUI.ObjectField(fieldRect, label, requiredValue, requiredComp, editorPick.HasFlagFast(EPick.Scene));
                DrawOverrideRichText(fieldRect, label, overrideRichTextChunks);
                if (changed.changed)
                {
                    Object result = GetNewValue(fieldResult, fieldType, requiredComp);
                    property.objectReferenceValue = result;
                    property.serializedObject.ApplyModifiedProperties();
                    TriggerChangedIMGUI(property, result);

                    cache.Error = fieldResult != null && result == null
                        ? $"{fieldResult} has no component {fieldType}"
                        : "";
                }
            }

            if (customPicker)
            {
                Rect overrideButtonRect = new Rect(position.x + position.width - 21, position.y, 21, position.height);
                _pickIcon ??= Util.LoadResource<Texture2D>("d_pick");
                if (GUI.Button(overrideButtonRect, new GUIContent(_pickIcon)))
                {
                    IReadOnlyList<GameObject> rootGameObjects = Array.Empty<GameObject>();
                    if (property.serializedObject.targetObject is Component component)
                    {
                        rootGameObjects = Util.SceneRootGameObjectsOf(component.gameObject) ?? Array.Empty<GameObject>();
                    }

                    FieldTypeSelectWindow.Open(property.objectReferenceValue, editorPick, fieldType, requiredComp,
                        rootGameObjects, fieldResult =>
                        {
                            property.objectReferenceValue = fieldResult;
                            property.serializedObject.ApplyModifiedProperties();
                            cache.Error = "";
                            TriggerChangedIMGUI(property, fieldResult);
                        });
                }
            }
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
            string error = EnsureKey(property).Error;
            return error == "" ? 0 : ImGuiHelpBox.GetHeight(error, width, MessageType.Error);
        }

        protected override Rect DrawBelow(Rect position, SerializedProperty property, GUIContent label,
            ISaintsAttribute saintsAttribute, int index, IReadOnlyList<PropertyAttribute> allAttributes,
            FieldInfo info, object parent) => ImGuiHelpBox.Draw(position, EnsureKey(property).Error, MessageType.Error);
    }
}
