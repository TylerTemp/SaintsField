#if UNITY_2021_3_OR_NEWER
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using SaintsField.Editor.Utils;
using SaintsField.Interfaces;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;
#if SAINTSFIELD_NEWTONSOFT_JSON
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
#endif

namespace SaintsField.Editor.Drawers.LayerDrawer
{
    public partial class LayerAttributeDrawer
    {
        private static string NameLayer(SerializedProperty property) => $"{property.propertyPath}__Layer";
        private static string NameHelpBox(SerializedProperty property) => $"{property.propertyPath}__Layer_HelpBox";

        protected override VisualElement CreateFieldUIToolKit(SerializedProperty property,
            ISaintsAttribute saintsAttribute,
            IReadOnlyList<PropertyAttribute> allAttributes,
            VisualElement container, FieldInfo info, object parent)
        {
            int curSelected = property.propertyType == SerializedPropertyType.Integer
                ? property.intValue
                : LayerMask.NameToLayer(property.stringValue);

            LayerField layerField = new LayerField(GetPreferredLabel(property), curSelected)
            {
                name = NameLayer(property),
            };
            layerField.AddToClassList(BaseField<Object>.alignedFieldUssClassName);
            layerField.AddToClassList(ClassAllowDisable);

            return layerField;
        }

        protected override VisualElement CreateBelowUIToolkit(SerializedProperty property, ISaintsAttribute saintsAttribute, int index,
            VisualElement container, FieldInfo info, object parent)
        {
            return new HelpBox("", HelpBoxMessageType.Error)
            {
                style =
                {
                    display = DisplayStyle.None,
                    flexGrow = 1,
                },
                name = NameHelpBox(property),
            };
        }

        protected override void OnAwakeUIToolkit(SerializedProperty property, ISaintsAttribute saintsAttribute,
            int index, IReadOnlyList<PropertyAttribute> allAttributes, VisualElement container,
            Action<object> onValueChangedCallback, FieldInfo info, object parent)
        {
            LayerField layerField = container.Q<LayerField>(NameLayer(property));

            layerField.labelElement.AddManipulator(new ContextualMenuManipulator(evt =>
            {
                evt.menu.AppendAction("Copy Property Path", _ => EditorGUIUtility.systemCopyBuffer = property.propertyPath);

                bool separator = false;

                if (property.propertyType == SerializedPropertyType.String)
                {
                    if (ClipboardHelper.CanCopySerializedProperty(SerializedPropertyType.String))
                    {
                        separator = true;
                        evt.menu.AppendSeparator();
                        evt.menu.AppendAction("Copy Layer Name", _ => EditorGUIUtility.systemCopyBuffer = property.stringValue,
                            string.IsNullOrEmpty(property.stringValue)
                                ? DropdownMenuAction.Status.Disabled
                                : DropdownMenuAction.Status.Normal);
                    }

                    (bool hasStringReflectionPaste, bool hasStringValuePaste) = ClipboardHelper.CanPasteSerializedProperty(SerializedPropertyType.String);
                    if (hasStringReflectionPaste)
                    {
                        if (!separator)
                        {
                            evt.menu.AppendSeparator();
                            separator = true;
                        }

                        if (hasStringValuePaste)
                        {
                            PropertyInfo propertyInfo = ClipboardHelper.EnsurePropertyInfo(SerializedPropertyType.String);
                            bool canPasteAsLayerName = false;
                            string canPasteValue = string.Empty;
                            if (propertyInfo != null && propertyInfo.CanRead)
                            {
                                canPasteValue = (string)propertyInfo.GetValue(null);
                                if (!string.IsNullOrEmpty(canPasteValue))
                                {
                                    foreach (LayerInfo layerInfo in GetAllLayers())
                                    {
                                        if (layerInfo.Name == canPasteValue)
                                        {
                                            canPasteAsLayerName = true;
                                            break;
                                        }
                                    }
                                }
                            }
                            evt.menu.AppendAction("Paste Layer Name", _ =>
                            {
                                property.stringValue = canPasteValue;
                                property.serializedObject.ApplyModifiedProperties();
                                onValueChangedCallback(property.stringValue);
                            }, canPasteAsLayerName? DropdownMenuAction.Status.Normal: DropdownMenuAction.Status.Disabled);
                        }
                    }

                    (bool hasIntReflectionPaste, bool hasIntValuePaste) = ClipboardHelper.CanPasteSerializedProperty(SerializedPropertyType.Integer);
                    if (hasIntReflectionPaste)
                    {
                        if (!separator)
                        {
                            evt.menu.AppendSeparator();
                            separator = true;
                        }

                        if (hasIntValuePaste)
                        {
                            PropertyInfo propertyInfo = ClipboardHelper.EnsurePropertyInfo(SerializedPropertyType.Integer);
                            bool canPasteAsLayerValue = false;
                            string canPasteValue = string.Empty;
                            if (propertyInfo != null && propertyInfo.CanRead)
                            {
                                int canPasteNumber = (int)(long)propertyInfo.GetValue(null);
                                foreach (LayerInfo layerInfo in GetAllLayers())
                                {
                                    if (layerInfo.Value == canPasteNumber)
                                    {
                                        canPasteAsLayerValue = true;
                                        canPasteValue = layerInfo.Name;
                                        break;
                                    }
                                }
                            }
                            evt.menu.AppendAction("Paste Layer Number", _ =>
                            {
                                property.stringValue = canPasteValue;
                                property.serializedObject.ApplyModifiedProperties();
                                onValueChangedCallback(canPasteValue);
                            }, canPasteAsLayerValue ? DropdownMenuAction.Status.Normal : DropdownMenuAction.Status.Disabled);
                        }
                    }
                }
                else if (property.propertyType == SerializedPropertyType.Integer)
                {
                    PropertyInfo propertyInfoInteger = ClipboardHelper.EnsurePropertyInfo(SerializedPropertyType.Integer);
                    if (ClipboardHelper.CanCopySerializedProperty(SerializedPropertyType.Integer))
                    {
                        bool canPaste = propertyInfoInteger != null && propertyInfoInteger.CanWrite;

                        separator = true;
                        evt.menu.AppendSeparator();
                        evt.menu.AppendAction("Copy Layer Number", _ => propertyInfoInteger?.SetValue(null, property.intValue),
                            canPaste
                                ? DropdownMenuAction.Status.Normal
                                : DropdownMenuAction.Status.Disabled);
                    }

                    (bool hasStringReflectionPaste, bool hasStringValuePaste) = ClipboardHelper.CanPasteSerializedProperty(SerializedPropertyType.String);
                    if (hasStringReflectionPaste)
                    {
                        if (!separator)
                        {
                            evt.menu.AppendSeparator();
                            separator = true;
                        }

                        if (hasStringValuePaste)
                        {
                            PropertyInfo propertyInfo = ClipboardHelper.EnsurePropertyInfo(SerializedPropertyType.String);
                            bool canPasteAsLayerName = false;
                            int canPasteValue = 0;
                            if (propertyInfo != null && propertyInfo.CanRead)
                            {
                                string clipboardValue = (string)propertyInfo.GetValue(null);
                                if (!string.IsNullOrEmpty(clipboardValue))
                                {
                                    foreach (LayerInfo layerInfo in GetAllLayers())
                                    {
                                        if (layerInfo.Name == clipboardValue)
                                        {
                                            canPasteAsLayerName = true;
                                            canPasteValue = layerInfo.Value;
                                            break;
                                        }
                                    }
                                }
                            }
                            evt.menu.AppendAction("Paste Layer Name", _ =>
                            {
                                property.intValue = canPasteValue;
                                property.serializedObject.ApplyModifiedProperties();
                                onValueChangedCallback(canPasteValue);
                            }, canPasteAsLayerName? DropdownMenuAction.Status.Normal: DropdownMenuAction.Status.Disabled);
                        }
                    }

                    (bool hasIntReflectionPaste, bool hasIntValuePaste) = ClipboardHelper.CanPasteSerializedProperty(SerializedPropertyType.Integer);
                    if (hasIntReflectionPaste)
                    {
                        if (!separator)
                        {
                            evt.menu.AppendSeparator();
                            separator = true;
                        }

                        if (hasIntValuePaste)
                        {
                            PropertyInfo propertyInfo = ClipboardHelper.EnsurePropertyInfo(SerializedPropertyType.Integer);
                            bool canPasteAsLayerValue = false;
                            int canPasteValue = 0;
                            if (propertyInfo != null && propertyInfo.CanRead)
                            {
                                int clipboardValue = (int)(long)propertyInfo.GetValue(null);
                                foreach (LayerInfo layerInfo in GetAllLayers())
                                {
                                    if (layerInfo.Value == clipboardValue)
                                    {
                                        canPasteAsLayerValue = true;
                                        canPasteValue = layerInfo.Value;
                                        break;
                                    }
                                }
                            }
                            evt.menu.AppendAction("Paste Layer Number", _ =>
                            {
                                property.intValue = canPasteValue;
                                property.serializedObject.ApplyModifiedProperties();
                                onValueChangedCallback(canPasteValue);
                            }, canPasteAsLayerValue ? DropdownMenuAction.Status.Normal : DropdownMenuAction.Status.Disabled);
                        }
                    }
                }
            }));

            layerField.RegisterValueChangedCallback(evt =>
            {
                if (property.propertyType == SerializedPropertyType.Integer)
                {
                    property.intValue = evt.newValue;
                    property.serializedObject.ApplyModifiedProperties();
                    ReflectUtils.SetValue(property.propertyPath, property.serializedObject.targetObject, info, parent,
                        evt.newValue);
                    onValueChangedCallback.Invoke(evt.newValue);
                }
                else
                {
                    string newValue = LayerMask.LayerToName(evt.newValue);
                    property.stringValue = newValue;
                    property.serializedObject.ApplyModifiedProperties();
                    ReflectUtils.SetValue(property.propertyPath, property.serializedObject.targetObject, info, parent,
                        newValue);
                    onValueChangedCallback.Invoke(newValue);
                }
            });

            HelpBox helpBox = container.Q<HelpBox>(NameHelpBox(property));
            RefreshHelpBox(property, helpBox);
            layerField.TrackPropertyValue(property, newProp =>
            {
                bool noError = RefreshHelpBox(newProp, helpBox);
                // ReSharper disable once InvertIf
                if (noError)
                {
                    int curSelected = property.propertyType == SerializedPropertyType.Integer
                        ? property.intValue
                        : LayerMask.NameToLayer(property.stringValue);
                    layerField.SetValueWithoutNotify(curSelected);
                }
            });
        }

        private static bool RefreshHelpBox(SerializedProperty property, HelpBox helpBox)
        {
            string errorMessage = GetErrorMessage(property);
            bool noError = string.IsNullOrEmpty(errorMessage);
            if (helpBox.text == errorMessage)
            {
                return noError;
            }

            helpBox.text = errorMessage;
            helpBox.style.display = string.IsNullOrEmpty(errorMessage)? DisplayStyle.None: DisplayStyle.Flex;
            return noError;
        }

        private static (bool found, LayerInfo info) FindLayerFromCopy()
        {
            IReadOnlyList<LayerInfo> allLayers = GetAllLayers();

            string pasteName = EditorGUIUtility.systemCopyBuffer;

            foreach (LayerInfo layerInfo in allLayers)
            {
                if(layerInfo.Name == pasteName)
                {
                    return (true, layerInfo);
                }
            }
#if SAINTSFIELD_NEWTONSOFT_JSON
            int intValue;
            try
            {
                intValue = JsonConvert.DeserializeObject<int>(pasteName);
            }
            catch (Exception)
            {
                return (false, default);
            }

            foreach (LayerInfo layerInfo in allLayers)
            {
                if (layerInfo.Value == intValue)
                {
                    return (true, layerInfo);
                }
            }
#endif
            return (false, default);
        }

    }
}
#endif
