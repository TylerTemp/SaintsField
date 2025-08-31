#if UNITY_2021_3_OR_NEWER
using System;
using System.Collections.Generic;
using System.Reflection;
using SaintsField.Editor.UIToolkitElements;
using SaintsField.Editor.Utils;
using SaintsField.Interfaces;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using SaintsField.Editor.Drawers.AdvancedDropdownDrawer;

namespace SaintsField.Editor.Drawers.LayerDrawer
{
    public partial class LayerAttributeDrawer
    {
        // private static string NameLayer(SerializedProperty property) => $"{property.propertyPath}__Layer";
        private static string NameHelpBox(SerializedProperty property) => $"{property.propertyPath}__Layer_HelpBox";

        protected override VisualElement CreateFieldUIToolKit(SerializedProperty property,
            ISaintsAttribute saintsAttribute,
            IReadOnlyList<PropertyAttribute> allAttributes,
            VisualElement container, FieldInfo info, object parent)
        {
            // ReSharper disable once SwitchStatementHandlesSomeKnownEnumValuesWithDefault
            switch (property.propertyType)
            {
                case SerializedPropertyType.Integer:
                {
                    LayerIntDropdownElement intDropdownElement = new LayerIntDropdownElement();
                    intDropdownElement.BindProperty(property);
                    return new IntDropdownField(GetPreferredLabel(property), intDropdownElement);
                }
                case SerializedPropertyType.String:
                {
                    LayerStringDropdownElement layerStringStringDropdown = new LayerStringDropdownElement();
                    layerStringStringDropdown.BindProperty(property);
                    return new StringDropdownField(GetPreferredLabel(property), layerStringStringDropdown);
                }
                default:
                    return new VisualElement();
            }
            // int curSelected = property.propertyType == SerializedPropertyType.Integer
            //     ? property.intValue
            //     : LayerMask.NameToLayer(property.stringValue);
            //
            // LayerField layerField = new LayerField(GetPreferredLabel(property), curSelected)
            // {
            //     name = NameLayer(property),
            // };
            // layerField.AddToClassList(BaseField<Object>.alignedFieldUssClassName);
            // layerField.AddToClassList(ClassAllowDisable);
            //
            // return layerField;
        }

        protected override VisualElement CreateBelowUIToolkit(SerializedProperty property,
            ISaintsAttribute saintsAttribute, int index,
            IReadOnlyList<PropertyAttribute> allAttributes,
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
            HelpBox helpBox = container.Q<HelpBox>(NameHelpBox(property));
            RefreshHelpBox(property, helpBox);

            // ReSharper disable once SwitchStatementHandlesSomeKnownEnumValuesWithDefault
            switch (property.propertyType)
            {
                case SerializedPropertyType.Integer:
                {
                    IntDropdownField intDropdownField = container.Q<IntDropdownField>();
                    AddContextualMenuManipulator(intDropdownField, property, onValueChangedCallback, info, parent);

                    intDropdownField.Button.clicked += () => MakeDropdown(property, intDropdownField, onValueChangedCallback, info, parent);
                }
                    break;
                case SerializedPropertyType.String:
                {
                    StringDropdownField layerStringField = container.Q<StringDropdownField>();
                    AddContextualMenuManipulator(layerStringField, property, onValueChangedCallback, info, parent);

                    layerStringField.Button.clicked += () => MakeDropdown(property, layerStringField, onValueChangedCallback, info, parent);
                }
                    break;
                default:
                    return;
            }

            // LayerField layerField = container.Q<LayerField>(NameLayer(property));
            //
            // layerField.labelElement.AddManipulator(new ContextualMenuManipulator(evt =>
            // {
            //     evt.menu.AppendAction("Copy Property Path", _ => EditorGUIUtility.systemCopyBuffer = property.propertyPath);
            //
            //     bool separator = false;
            //
            //     if (property.propertyType == SerializedPropertyType.String)
            //     {
            //         if (ClipboardHelper.CanCopySerializedProperty(SerializedPropertyType.String))
            //         {
            //             separator = true;
            //             evt.menu.AppendSeparator();
            //             evt.menu.AppendAction("Copy Layer Name", _ => EditorGUIUtility.systemCopyBuffer = property.stringValue,
            //                 string.IsNullOrEmpty(property.stringValue)
            //                     ? DropdownMenuAction.Status.Disabled
            //                     : DropdownMenuAction.Status.Normal);
            //         }
            //
            //         (bool hasStringReflectionPaste, bool hasStringValuePaste) = ClipboardHelper.CanPasteSerializedProperty(SerializedPropertyType.String);
            //         if (hasStringReflectionPaste)
            //         {
            //             if (!separator)
            //             {
            //                 evt.menu.AppendSeparator();
            //                 separator = true;
            //             }
            //
            //             if (hasStringValuePaste)
            //             {
            //                 PropertyInfo propertyInfo = ClipboardHelper.EnsurePropertyInfo(SerializedPropertyType.String);
            //                 bool canPasteAsLayerName = false;
            //                 string canPasteValue = string.Empty;
            //                 if (propertyInfo != null && propertyInfo.CanRead)
            //                 {
            //                     canPasteValue = (string)propertyInfo.GetValue(null);
            //                     if (!string.IsNullOrEmpty(canPasteValue))
            //                     {
            //                         foreach (LayerUtils.LayerInfo layerInfo in LayerUtils.GetAllLayers())
            //                         {
            //                             if (layerInfo.Name == canPasteValue)
            //                             {
            //                                 canPasteAsLayerName = true;
            //                                 break;
            //                             }
            //                         }
            //                     }
            //                 }
            //                 evt.menu.AppendAction("Paste Layer Name", _ =>
            //                 {
            //                     property.stringValue = canPasteValue;
            //                     property.serializedObject.ApplyModifiedProperties();
            //                     onValueChangedCallback(property.stringValue);
            //                 }, canPasteAsLayerName? DropdownMenuAction.Status.Normal: DropdownMenuAction.Status.Disabled);
            //             }
            //         }
            //
            //         (bool hasIntReflectionPaste, bool hasIntValuePaste) = ClipboardHelper.CanPasteSerializedProperty(SerializedPropertyType.Integer);
            //         if (hasIntReflectionPaste)
            //         {
            //             if (!separator)
            //             {
            //                 evt.menu.AppendSeparator();
            //                 separator = true;
            //             }
            //
            //             if (hasIntValuePaste)
            //             {
            //                 PropertyInfo propertyInfo = ClipboardHelper.EnsurePropertyInfo(SerializedPropertyType.Integer);
            //                 bool canPasteAsLayerValue = false;
            //                 string canPasteValue = string.Empty;
            //                 if (propertyInfo != null && propertyInfo.CanRead)
            //                 {
            //                     int canPasteNumber = (int)(long)propertyInfo.GetValue(null);
            //                     foreach (LayerUtils.LayerInfo layerInfo in LayerUtils.GetAllLayers())
            //                     {
            //                         if (layerInfo.Value == canPasteNumber)
            //                         {
            //                             canPasteAsLayerValue = true;
            //                             canPasteValue = layerInfo.Name;
            //                             break;
            //                         }
            //                     }
            //                 }
            //                 evt.menu.AppendAction("Paste Layer Number", _ =>
            //                 {
            //                     property.stringValue = canPasteValue;
            //                     property.serializedObject.ApplyModifiedProperties();
            //                     onValueChangedCallback(canPasteValue);
            //                 }, canPasteAsLayerValue ? DropdownMenuAction.Status.Normal : DropdownMenuAction.Status.Disabled);
            //             }
            //         }
            //     }
            //     else if (property.propertyType == SerializedPropertyType.Integer)
            //     {
            //         PropertyInfo propertyInfoInteger = ClipboardHelper.EnsurePropertyInfo(SerializedPropertyType.Integer);
            //         if (ClipboardHelper.CanCopySerializedProperty(SerializedPropertyType.Integer))
            //         {
            //             bool canPaste = propertyInfoInteger != null && propertyInfoInteger.CanWrite;
            //
            //             separator = true;
            //             evt.menu.AppendSeparator();
            //             evt.menu.AppendAction("Copy Layer Number", _ => propertyInfoInteger?.SetValue(null, property.intValue),
            //                 canPaste
            //                     ? DropdownMenuAction.Status.Normal
            //                     : DropdownMenuAction.Status.Disabled);
            //         }
            //
            //         (bool hasStringReflectionPaste, bool hasStringValuePaste) = ClipboardHelper.CanPasteSerializedProperty(SerializedPropertyType.String);
            //         if (hasStringReflectionPaste)
            //         {
            //             if (!separator)
            //             {
            //                 evt.menu.AppendSeparator();
            //                 separator = true;
            //             }
            //
            //             if (hasStringValuePaste)
            //             {
            //                 PropertyInfo propertyInfo = ClipboardHelper.EnsurePropertyInfo(SerializedPropertyType.String);
            //                 bool canPasteAsLayerName = false;
            //                 int canPasteValue = 0;
            //                 if (propertyInfo != null && propertyInfo.CanRead)
            //                 {
            //                     string clipboardValue = (string)propertyInfo.GetValue(null);
            //                     if (!string.IsNullOrEmpty(clipboardValue))
            //                     {
            //                         foreach (LayerUtils.LayerInfo layerInfo in GetAllLayers())
            //                         {
            //                             if (layerInfo.Name == clipboardValue)
            //                             {
            //                                 canPasteAsLayerName = true;
            //                                 canPasteValue = layerInfo.Value;
            //                                 break;
            //                             }
            //                         }
            //                     }
            //                 }
            //                 evt.menu.AppendAction("Paste Layer Name", _ =>
            //                 {
            //                     property.intValue = canPasteValue;
            //                     property.serializedObject.ApplyModifiedProperties();
            //                     onValueChangedCallback(canPasteValue);
            //                 }, canPasteAsLayerName? DropdownMenuAction.Status.Normal: DropdownMenuAction.Status.Disabled);
            //             }
            //         }
            //
            //         (bool hasIntReflectionPaste, bool hasIntValuePaste) = ClipboardHelper.CanPasteSerializedProperty(SerializedPropertyType.Integer);
            //         if (hasIntReflectionPaste)
            //         {
            //             if (!separator)
            //             {
            //                 evt.menu.AppendSeparator();
            //                 separator = true;
            //             }
            //
            //             if (hasIntValuePaste)
            //             {
            //                 PropertyInfo propertyInfo = ClipboardHelper.EnsurePropertyInfo(SerializedPropertyType.Integer);
            //                 bool canPasteAsLayerValue = false;
            //                 int canPasteValue = 0;
            //                 if (propertyInfo != null && propertyInfo.CanRead)
            //                 {
            //                     int clipboardValue = (int)(long)propertyInfo.GetValue(null);
            //                     foreach (LayerUtils.LayerInfo layerInfo in GetAllLayers())
            //                     {
            //                         if (layerInfo.Value == clipboardValue)
            //                         {
            //                             canPasteAsLayerValue = true;
            //                             canPasteValue = layerInfo.Value;
            //                             break;
            //                         }
            //                     }
            //                 }
            //                 evt.menu.AppendAction("Paste Layer Number", _ =>
            //                 {
            //                     property.intValue = canPasteValue;
            //                     property.serializedObject.ApplyModifiedProperties();
            //                     onValueChangedCallback(canPasteValue);
            //                 }, canPasteAsLayerValue ? DropdownMenuAction.Status.Normal : DropdownMenuAction.Status.Disabled);
            //             }
            //         }
            //     }
            // }));
            //
            // layerField.RegisterValueChangedCallback(evt =>
            // {
            //     if (property.propertyType == SerializedPropertyType.Integer)
            //     {
            //         property.intValue = evt.newValue;
            //         property.serializedObject.ApplyModifiedProperties();
            //         ReflectUtils.SetValue(property.propertyPath, property.serializedObject.targetObject, info, parent,
            //             evt.newValue);
            //         onValueChangedCallback.Invoke(evt.newValue);
            //     }
            //     else
            //     {
            //         string newValue = LayerMask.LayerToName(evt.newValue);
            //         property.stringValue = newValue;
            //         property.serializedObject.ApplyModifiedProperties();
            //         ReflectUtils.SetValue(property.propertyPath, property.serializedObject.targetObject, info, parent,
            //             newValue);
            //         onValueChangedCallback.Invoke(newValue);
            //     }
            // });


            // layerField.TrackPropertyValue(property, newProp =>
            // {
            //     bool noError = RefreshHelpBox(newProp, helpBox);
            //     // ReSharper disable once InvertIf
            //     if (noError)
            //     {
            //         int curSelected = property.propertyType == SerializedPropertyType.Integer
            //             ? property.intValue
            //             : LayerMask.NameToLayer(property.stringValue);
            //         layerField.SetValueWithoutNotify(curSelected);
            //     }
            // });
        }

        private static void AddContextualMenuManipulator(VisualElement root, SerializedProperty property,
            Action<object> onValueChangedCallback, FieldInfo info, object parent)
        {
            UIToolkitUtils.AddContextualMenuManipulator(root, property,
                () => Util.PropertyChangedCallback(property, info, onValueChangedCallback));

            bool isString = property.propertyType == SerializedPropertyType.String;
            root.AddManipulator(new ContextualMenuManipulator(evt =>
            {
                string clipboardText = EditorGUIUtility.systemCopyBuffer;
                if (string.IsNullOrEmpty(clipboardText))
                {
                    return;
                }

                bool canBeInt = int.TryParse(clipboardText, out int clipboardInt);

                if (isString)
                {
                    foreach (LayerUtils.LayerInfo layerInfo in LayerUtils.GetAllLayers())
                    {
                        if (layerInfo.Name == clipboardText
                            || canBeInt && layerInfo.Value == clipboardInt)
                        {
                            evt.menu.AppendAction($"Paste \"{layerInfo.Name}\"", _ =>
                            {
                                property.stringValue = layerInfo.Name;
                                ReflectUtils.SetValue(property.propertyPath, property.serializedObject.targetObject, info, parent, layerInfo.Name);
                                property.serializedObject.ApplyModifiedProperties();
                                onValueChangedCallback.Invoke(layerInfo.Name);
                            });
                            return;
                        }
                    }
                }
                else
                {
                    foreach (LayerUtils.LayerInfo layerInfo in LayerUtils.GetAllLayers())
                    {
                        if (layerInfo.Name == clipboardText
                            || canBeInt && layerInfo.Value == clipboardInt)
                        {
                            evt.menu.AppendAction($"Paste \"{layerInfo.Name}\"({layerInfo.Value})", _ =>
                            {
                                property.intValue = layerInfo.Value;
                                ReflectUtils.SetValue(property.propertyPath, property.serializedObject.targetObject, info, parent, layerInfo.Value);
                                property.serializedObject.ApplyModifiedProperties();
                                onValueChangedCallback.Invoke(layerInfo.Value);
                            });
                            return;
                        }
                    }
                }
            }));


        }

        private static void MakeDropdown(SerializedProperty property, VisualElement root, Action<object> onValueChangedCallback, FieldInfo info, object parent)
        {
            bool isString = property.propertyType == SerializedPropertyType.String;
            AdvancedDropdownList<LayerUtils.LayerInfo> dropdown = new AdvancedDropdownList<LayerUtils.LayerInfo>();
            if (isString)
            {
                dropdown.Add("[Empty String]", new LayerUtils.LayerInfo(string.Empty, -1));
                dropdown.AddSeparator();
            }

            bool hasSelected = false;
            LayerUtils.LayerInfo selected = new LayerUtils.LayerInfo("", -9999);
            foreach (LayerUtils.LayerInfo layerInfo in LayerUtils.GetAllLayers())
            {
                dropdown.Add(layerInfo.Name, layerInfo);
                // ReSharper disable once InvertIf
                if (isString && layerInfo.Name == property.stringValue
                    || !isString && layerInfo.Value == property.intValue)
                {
                    hasSelected = true;
                    selected = layerInfo;
                }
            }

            dropdown.AddSeparator();
            dropdown.Add("Edit Layers...", new LayerUtils.LayerInfo("", -2), false, "d_editicon.sml");

            AdvancedDropdownMetaInfo metaInfo = new AdvancedDropdownMetaInfo
            {
                CurValues = hasSelected ? new object[] { selected } : Array.Empty<object>(),
                DropdownListValue = dropdown,
                SelectStacks = Array.Empty<AdvancedDropdownAttributeDrawer.SelectStack>(),
            };

            (Rect worldBound, float maxHeight) = SaintsAdvancedDropdownUIToolkit.GetProperPos(root.worldBound);

            SaintsAdvancedDropdownUIToolkit sa = new SaintsAdvancedDropdownUIToolkit(
                metaInfo,
                root.worldBound.width,
                maxHeight,
                false,
                (_, curItem) =>
                {
                    LayerUtils.LayerInfo layerInfo = (LayerUtils.LayerInfo)curItem;
                    switch (layerInfo.Value)
                    {
                        case -1:
                        {
                            Debug.Assert(isString);
                            property.stringValue = "";
                            ReflectUtils.SetValue(property.propertyPath, property.serializedObject.targetObject, info, parent, "");
                            property.serializedObject.ApplyModifiedProperties();
                            onValueChangedCallback.Invoke("");
                        }
                            break;
                        case -2:
                        {
                            Selection.activeObject = AssetDatabase.LoadMainAssetAtPath("ProjectSettings/TagManager.asset");
                        }
                            break;
                        default:
                        {
                            if (isString)
                            {
                                property.stringValue = layerInfo.Name;
                                ReflectUtils.SetValue(property.propertyPath, property.serializedObject.targetObject, info, parent, layerInfo.Name);
                                property.serializedObject.ApplyModifiedProperties();
                                onValueChangedCallback.Invoke(layerInfo.Name);
                            }
                            else
                            {
                                property.intValue = layerInfo.Value;
                                ReflectUtils.SetValue(property.propertyPath, property.serializedObject.targetObject, info, parent, layerInfo.Value);
                                property.serializedObject.ApplyModifiedProperties();
                                onValueChangedCallback.Invoke(layerInfo.Value);
                            }
                        }
                            break;
                    }
                }
            );

            // DebugPopupExample.SaintsAdvancedDropdownUIToolkit = sa;
            // var editorWindow = EditorWindow.GetWindow<DebugPopupExample>();
            // editorWindow.Show();

            UnityEditor.PopupWindow.Show(worldBound, sa);
        }

        private static void RefreshHelpBox(SerializedProperty property, HelpBox helpBox)
        {
            string errorMessage = GetErrorMessage(property);
            // bool noError = string.IsNullOrEmpty(errorMessage);
            if (helpBox.text == errorMessage)
            {
                return;
            }

            helpBox.text = errorMessage;
            helpBox.style.display = string.IsNullOrEmpty(errorMessage)? DisplayStyle.None: DisplayStyle.Flex;
            // return;
        }

//         private static (bool found, LayerUtils.LayerInfo info) FindLayerFromCopy()
//         {
//             IReadOnlyList<LayerUtils.LayerInfo> allLayers = LayerUtils.GetAllLayers();
//
//             string pasteName = EditorGUIUtility.systemCopyBuffer;
//
//             foreach (LayerUtils.LayerInfo layerInfo in allLayers)
//             {
//                 if(layerInfo.Name == pasteName)
//                 {
//                     return (true, layerInfo);
//                 }
//             }
// #if SAINTSFIELD_NEWTONSOFT_JSON
//             int intValue;
//             try
//             {
//                 intValue = JsonConvert.DeserializeObject<int>(pasteName);
//             }
//             catch (Exception)
//             {
//                 return (false, default);
//             }
//
//             foreach (LayerUtils.LayerInfo layerInfo in allLayers)
//             {
//                 if (layerInfo.Value == intValue)
//                 {
//                     return (true, layerInfo);
//                 }
//             }
// #endif
//             return (false, default);
//         }

    }
}
#endif
