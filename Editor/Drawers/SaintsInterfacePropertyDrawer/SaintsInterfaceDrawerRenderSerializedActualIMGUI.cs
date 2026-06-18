using System;
using System.Linq;
using System.Reflection;
using SaintsField.Editor.Core;
using SaintsField.Editor.Drawers.AdvancedDropdownDrawer;
using SaintsField.Editor.Drawers.ReferencePicker;
using SaintsField.Editor.Drawers.SaintsRowDrawer;
using SaintsField.Editor.Drawers.TreeDropdownDrawer;
using SaintsField.Editor.Utils;
using SaintsField.SaintsSerialization;
using SaintsField.Utils;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace SaintsField.Editor.Drawers.SaintsInterfacePropertyDrawer
{
    public partial class SaintsInterfaceDrawer
    {
        internal float GetSerializedActualFieldHeight(SaintsSerializedActualAttribute saintsSerializedActual,
            SerializedProperty property, GUIContent label, float width, FieldInfo info, object parent)
        {
            InterfaceStatusIMGUI cache = EnsureKey(property);
            RefreshSerializedActualCache(cache, saintsSerializedActual, property, info, parent);
            if (cache.Error != "")
            {
                return ImGuiHelpBox.GetHeight(cache.Error, Mathf.Max(1f, width), MessageType.Error);
            }

            if (!cache.FieldInfo.IsVRefProp.boolValue || !cache.FieldInfo.VRefProp.isExpanded)
            {
                return SingleLineHeight;
            }

            return SingleLineHeight +
                   SaintsRowAttributeDrawer.GetManagedReferenceBodyHeight(cache.BodyInfo, width - IndentWidth);
        }

        internal bool DrawSerializedActualField(Rect position, SaintsSerializedActualAttribute saintsSerializedActual,
            SerializedProperty property, GUIContent label, FieldInfo info, object parent)
        {
            InterfaceStatusIMGUI cache = EnsureKey(property);
            RefreshSerializedActualCache(cache, saintsSerializedActual, property, info, parent);
            if (cache.Error != "")
            {
                ImGuiHelpBox.Draw(position, cache.Error, MessageType.Error);
                return true;
            }

            Rect lineRect = new Rect(position)
            {
                height = SingleLineHeight,
            };
            Rect contentRect = EditorGUI.PrefixLabel(lineRect, label);
            Rect modeRect = new Rect(contentRect)
            {
                width = ModeButtonWidth,
            };

            if (GUI.Button(modeRect, GetModeContent(cache.FieldInfo.IsVRefProp.boolValue), ModeButtonStyle))
            {
                bool nextIsVRef = !cache.FieldInfo.IsVRefProp.boolValue;
                bool changed = cache.FieldInfo.IsVRefProp.boolValue != nextIsVRef;
                cache.FieldInfo.IsVRefProp.boolValue = nextIsVRef;
                changed |= SyncInterfaceModeSideEffectsWithoutApply(cache.FieldInfo.ValueProp,
                    cache.FieldInfo.VRefProp, nextIsVRef);
                if (changed)
                {
                    cache.FieldInfo.IsVRefProp.serializedObject.ApplyModifiedProperties();
                    RefreshSerializedActualCache(cache, saintsSerializedActual, property, info, parent);
                    TriggerChangedIMGUI(property, GetCurrentValue(cache));
                }
            }

            Rect activeRect = new Rect(contentRect)
            {
                xMin = modeRect.xMax + FieldSpacing,
            };
            if (cache.FieldInfo.IsVRefProp.boolValue)
            {
                DrawSerializedActualReferenceField(position, activeRect, saintsSerializedActual, property, info, parent,
                    cache);
            }
            else
            {
                DrawSerializedActualObjectField(activeRect, saintsSerializedActual, property, info, parent, cache);
            }

            return true;
        }

        private void RefreshSerializedActualCache(InterfaceStatusIMGUI cache,
            SaintsSerializedActualAttribute saintsSerializedActual, SerializedProperty property, FieldInfo info,
            object parent)
        {
            Type targetType = ReflectUtils.SaintsSerializedActualGetType(saintsSerializedActual, parent);
            if (targetType == null)
            {
                SetSerializedActualCacheError(cache, $"Failed to get type for {property.propertyPath}");
                return;
            }

            (string error, IWrapProp saintsInterfaceProp, int arrayIndex, object _) = GetSerName(property, info);
            if (error != "")
            {
                SetSerializedActualCacheError(cache, error);
                return;
            }

            string wrapPropName = ReflectUtils.GetIWrapPropName(saintsInterfaceProp.GetType());
            SerializedProperty valueProp = property.FindPropertyRelative(wrapPropName) ??
                                           SerializedUtils.FindPropertyByAutoPropertyName(property, wrapPropName);
            if (valueProp == null)
            {
                SetSerializedActualCacheError(cache, $"{wrapPropName} not found in {property.propertyPath}");
                return;
            }

            SerializedProperty isVRefProp = property.FindPropertyRelative(nameof(SaintsSerializedProperty.IsVRef)) ??
                                            SerializedUtils.FindPropertyByAutoPropertyName(property,
                                                nameof(SaintsSerializedProperty.IsVRef));
            if (isVRefProp == null)
            {
                SetSerializedActualCacheError(cache, $"{nameof(SaintsSerializedProperty.IsVRef)} not found in {property.propertyPath}");
                return;
            }

            SerializedProperty vRefProp = property.FindPropertyRelative(nameof(SaintsSerializedProperty.VRef)) ??
                                          SerializedUtils.FindPropertyByAutoPropertyName(property,
                                              nameof(SaintsSerializedProperty.VRef));
            if (vRefProp == null)
            {
                SetSerializedActualCacheError(cache, $"{nameof(SaintsSerializedProperty.VRef)} not found in {property.propertyPath}");
                return;
            }

            cache.FieldInfo = new InterfaceFieldInfo("", arrayIndex, typeof(Object), targetType, valueProp,
                isVRefProp, vRefProp,
                typeof(SaintsSerializedProperty).GetField(nameof(SaintsSerializedProperty.VRef)));
            EnsureManagedReferenceWatcher(cache);
            EnsureExpandState(cache, info);

            cache.ReferenceLabel = Util.GetReferencePropertyLabel(cache.FieldInfo.VRefProp);
            cache.ReferenceMetaInfo = GetReferenceMetaInfo(cache.FieldInfo.VRefProp,
                GetTypesImplementingInterface(cache.FieldInfo.InterfaceType));
            SaintsRowAttributeDrawer.SyncManagedReferenceBody(cache.BodyInfo, cache.FieldInfo.VRefProp, info, parent,
                cache.FieldInfo.VRefProp.managedReferenceValue, this);
            cache.Error = "";
        }

        private static void SetSerializedActualCacheError(InterfaceStatusIMGUI cache, string error)
        {
            cache.Error = error;
            SaintsRowAttributeDrawer.ClearManagedReferenceBody(cache.BodyInfo);
        }

        private void DrawSerializedActualObjectField(Rect position,
            SaintsSerializedActualAttribute saintsSerializedActual, SerializedProperty property, FieldInfo info,
            object parent, InterfaceStatusIMGUI cache)
        {
            Rect pickerRect = new Rect(position)
            {
                x = position.xMax - PickerButtonWidth,
                width = PickerButtonWidth,
            };
            Rect fieldRect = new Rect(position)
            {
                xMax = pickerRect.xMin,
            };

            Object oldValue = cache.FieldInfo.ValueProp.objectReferenceValue;
            EditorGUI.BeginChangeCheck();
            Object newValue = EditorGUI.ObjectField(fieldRect, GUIContent.none, oldValue, cache.FieldInfo.ValueType,
                true);
            if (EditorGUI.EndChangeCheck())
            {
                (bool isMatch, Object matchedValue) = GetMatchedInterfaceValue(newValue, cache.FieldInfo.ValueType,
                    cache.FieldInfo.InterfaceType);
                if (!isMatch)
                {
                    matchedValue = oldValue;
                }

                if (cache.FieldInfo.ValueProp.objectReferenceValue != matchedValue)
                {
                    cache.FieldInfo.ValueProp.objectReferenceValue = matchedValue;
                    cache.FieldInfo.ValueProp.serializedObject.ApplyModifiedProperties();
                    RefreshSerializedActualCache(cache, saintsSerializedActual, property, info, parent);
                    TriggerChangedIMGUI(property, matchedValue);
                }
            }

            GUIContent pickerContent = PickerIcon == null
                ? GUIContent.none
                : new GUIContent(PickerIcon, "Select");
            if (!GUI.Button(pickerRect, pickerContent, ImageButtonStyle))
            {
                return;
            }

            FieldInterfaceSelectWindow.Open(cache.FieldInfo.ValueProp.objectReferenceValue, cache.FieldInfo.ValueType,
                cache.FieldInfo.InterfaceType, fieldResult =>
                {
                    if (cache.FieldInfo.ValueProp.objectReferenceValue == fieldResult)
                    {
                        return;
                    }

                    cache.FieldInfo.ValueProp.objectReferenceValue = fieldResult;
                    cache.FieldInfo.ValueProp.serializedObject.ApplyModifiedProperties();
                    RefreshSerializedActualCache(cache, saintsSerializedActual, property, info, parent);
                    TriggerChangedIMGUI(property, fieldResult);
                });
        }

        private static (bool isMatch, Object matchedValue) GetMatchedInterfaceValue(Object candidate, Type valueType,
            Type interfaceType)
        {
            if (!candidate)
            {
                return (true, null);
            }

            if (interfaceType.IsInstanceOfType(candidate))
            {
                return (true, candidate);
            }

            return GetSerializedObject(candidate, valueType, interfaceType);
        }

        private void DrawSerializedActualReferenceField(Rect position, Rect lineContentRect,
            SaintsSerializedActualAttribute saintsSerializedActual, SerializedProperty property, FieldInfo info,
            object parent, InterfaceStatusIMGUI cache)
        {
            Rect foldoutRect = new Rect(lineContentRect)
            {
                width = FoldoutWidth,
            };
            bool expanded = GUI.Toggle(foldoutRect, cache.FieldInfo.VRefProp.isExpanded, GUIContent.none,
                EditorStyles.foldout);
            if (expanded != cache.FieldInfo.VRefProp.isExpanded)
            {
                cache.FieldInfo.VRefProp.isExpanded = expanded;
            }

            Rect popupRect = new Rect(lineContentRect)
            {
                xMin = foldoutRect.xMax + FieldSpacing,
            };

            GUI.SetNextControlName(FieldControlName);
            if (GUI.Button(popupRect, GUIContent.none, EditorStyles.popup))
            {
                PopupWindow.Show(popupRect, new SaintsTreeDropdownIMGUI(
                    cache.ReferenceMetaInfo,
                    Mathf.Max(popupRect.width, 220f),
                    320f,
                    false,
                    (curItem, _) =>
                    {
                        object currentValue = cache.FieldInfo.VRefProp.managedReferenceValue;
                        object instance = curItem == null
                            ? null
                            : ReferencePickerAttributeDrawer.CopyObj(currentValue,
                                Activator.CreateInstance((Type)curItem));

                        cache.FieldInfo.VRefProp.managedReferenceValue = instance;
                        cache.FieldInfo.VRefProp.serializedObject.ApplyModifiedProperties();
                        RefreshSerializedActualCache(cache, saintsSerializedActual, property, info, parent);
                        TriggerChangedIMGUI(property, instance);
                        return null;
                    }));
            }

            Rect drawRect = new Rect(popupRect)
            {
                xMin = popupRect.xMin + 6f,
                xMax = popupRect.xMax - 18f,
            };
            _richTextDrawer.DrawChunks(drawRect,
                RichTextDrawer.ParseRichXmlWithProvider(cache.ReferenceLabel,
                    new RichTextDrawer.EmptyRichTextTagProvider()));

            if (!cache.FieldInfo.VRefProp.isExpanded)
            {
                cache.PendingManagedReferenceChanged = false;
                return;
            }

            Rect childRect = new Rect(position)
            {
                x = position.x + IndentWidth,
                y = position.y + SingleLineHeight,
                width = position.width - IndentWidth,
                height = position.height - SingleLineHeight,
            };
            SaintsRowAttributeDrawer.DrawManagedReferenceBody(childRect, cache.BodyInfo);

            if (!cache.PendingManagedReferenceChanged)
            {
                return;
            }

            cache.PendingManagedReferenceChanged = false;
            TriggerChangedIMGUI(property, cache.FieldInfo.VRefProp.managedReferenceValue);
        }
    }
}
