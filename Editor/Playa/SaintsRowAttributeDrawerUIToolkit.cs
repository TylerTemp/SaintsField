﻿#if UNITY_2021_3_OR_NEWER && !SAINTSFIELD_UI_TOOLKIT_DISABLE
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using SaintsField.Editor.Utils;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace SaintsField.Editor.Playa
{
    public partial class SaintsRowAttributeDrawer
    {
        public const string SaintsRowClass = "saintsfield-saintsrow";

        public static VisualElement CreateElement(SerializedProperty property, string label, FieldInfo info, SaintsRowAttribute saintsRowAttribute, IMakeRenderer makeRenderer, IDOTweenPlayRecorder doTweenPlayRecorder)
        {
            VisualElement root;
            if (saintsRowAttribute?.Inline ?? false)
            {
                root = new VisualElement
                {
                    style =
                    {
                        flexGrow = 1,
                    },
                };
            }
            else
            {
                Foldout foldout = new Foldout
                {
                    text = property.displayName,
                    value = property.isExpanded,
                    style =
                    {
                        flexGrow = 1,
                    },
                };
                foldout.RegisterValueChangedCallback(evt =>
                {
                    property.isExpanded = evt.newValue;
                });

                foldout.AddManipulator(new ContextualMenuManipulator(evt =>
                {
                    evt.menu.AppendAction("Copy Property Path", _ => EditorGUIUtility.systemCopyBuffer = property.propertyPath);
                    bool seperated = false;
                    if (ClipboardHelper.EnsureSetSerializedPropertyMethod())
                    {
                        seperated = true;
                        evt.menu.AppendSeparator();
                        evt.menu.AppendAction("Copy", _ =>
                        {
                            ClipboardHelper.SetSerializedProperty(property);
                        });
                    }

                    // (bool hasReflectionOk, bool hasResult) = ClipboardHelper.HasSerializedProperty();

                    // ReSharper disable once InvertIf
                    if (ClipboardHelper.EnsureHasSerializedPropertyMethod() && ClipboardHelper.EnsureGetSerializedPropertyMethod())
                    {
                        if (!seperated)
                        {
                            // seperated = true;
                            evt.menu.AppendSeparator();
                        }

                        if (ClipboardHelper.HasSerializedProperty())
                        {
                            evt.menu.AppendAction("Paste", _ =>
                            {
                                ClipboardHelper.GetSerializedProperty(property);
                                property.serializedObject.ApplyModifiedProperties();
                            });
                        }
                        else
                        {
                            evt.menu.AppendAction("Paste", _ => { }, DropdownMenuAction.Status.Disabled);
                        }
                    }

                    // ClipboardContextMenuHelper.SetupPropertyCopyPaste(property, evt.menu, null);
                }));

                root = foldout;
            }

            root.AddToClassList(SaintsRowClass);

            FillElement(root, property, info, makeRenderer, doTweenPlayRecorder);

            if (property.propertyType == SerializedPropertyType.ManagedReference)
            {
                string propPath = property.propertyPath;
                root.userData = property.managedReferenceId;
                root.schedule.Execute(() =>
                    {
                        long curId = (long) root.userData;
                        // ReSharper disable once InvertIf
                        if (curId != property.managedReferenceId)
                        {
                            // Debug.Log($"Changed {curId} -> {property.managedReferenceId}/{property.managedReferenceFieldTypename}");
                            root.userData = property.managedReferenceId;
                            root.Clear();

                            SerializedProperty newProp = property.serializedObject.FindProperty(propPath);

                            FillElement(root, newProp, info, makeRenderer, doTweenPlayRecorder);
                        }
                        // Debug.Log(property.managedReferenceId);
                    })
                    .Every(100);
            }
            return root;
        }

        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            // Debug.Log($"Render {property.propertyPath}");
            // Debug.Log(property.isExpanded);

            SaintsRowAttribute saintsRowAttribute = attribute as SaintsRowAttribute;

            return CreateElement(property,
#if UNITY_2022_3_OR_NEWER
                preferredLabel
#else
                property.displayName
#endif
            , fieldInfo, saintsRowAttribute, this, this);
        }

        private static void FillElement(VisualElement root, SerializedProperty property, FieldInfo info, IMakeRenderer makeRenderer, IDOTweenPlayRecorder doTweenPlayRecorder)
        {
            object value = null;
            if (property.propertyType == SerializedPropertyType.ManagedReference)
            {
                value = property.managedReferenceValue;
                if (value == null)
                {
                    return;
                }
            }
            else
            {
                string error = "";
                object parentValue = SerializedUtils.GetFieldInfoAndDirectParent(property).parent;
                if (parentValue == null)
                {
                    error = $"Parent of {property.propertyPath} not found";
                }
                else
                {
                    (string getValueError, int _, object getValue) = Util.GetValue(property, info, parentValue);
                    if (getValueError != "")
                    {
                        error = getValueError;
                    }
                    else
                    {
                        value = getValue;
                    }
                }

                if (error != "")
                {
                    root.Add(new HelpBox(error, HelpBoxMessageType.Error));
                    return;
                }

                // value = getValue;
            }

            Debug.Assert(value != null);

            Dictionary<string, SerializedProperty> serializedFieldNames = GetSerializableFieldInfo(property)
                .ToDictionary(each => each.name, each => each.property);

            // SaintsRowAttribute saintsRowAttribute = (SaintsRowAttribute)attribute;

             IReadOnlyList<ISaintsRenderer> renderer =
                 SaintsEditor.HelperGetRenderers(serializedFieldNames, property.serializedObject, makeRenderer, value);

//             // Debug.Log($"{renderer.Count}");
//
//             // VisualElement bodyElement = SaintsEditor.CreateVisualElement(renderer);


             VisualElement bodyElement = new VisualElement();
             // bodyElement.Add(new Label(property.displayName));

             // // this works just fine
             // foreach (KeyValuePair<string,SerializedProperty> kv in serializedFieldNames)
             // {
             //     Debug.Log($"{kv.Key} -> {kv.Value.propertyPath}");
             //     // bodyElement.Add(new Label(kv.Key));
             //     var prop = new PropertyField(kv.Value);
             //     prop.Bind(property.serializedObject);
             //     bodyElement.Add(prop);
             // }

             // this... fixed by adding Bind()... wtf...
             foreach (ISaintsRenderer saintsRenderer in renderer)
             {
                 VisualElement rendererElement = saintsRenderer.CreateVisualElement();
                 if (rendererElement != null)
                 {
                     // Debug.Log($"add {saintsRenderer}: {rendererElement}");
                     bodyElement.Add(rendererElement);
                 }
             }

             root.Add(bodyElement);

             // Foldout foldout = new Foldout
             // {
             //     value = true,
             // };
             // foldout.Add(bodyElement);
             // root.Add(foldout);
             //
             // return;

//             foreach (ISaintsRenderer saintsRenderer in renderer)
//             {
//                 VisualElement rendererElement = saintsRenderer.CreateVisualElement();
//                 if (rendererElement != null)
//                 {
//                     Debug.Log($"add {saintsRenderer}: {rendererElement}");
//                     bodyElement.Add(rendererElement);
//                 }
//             }
//
#if DOTWEEN && !SAINTSFIELD_DOTWEEN_DISABLED
            bodyElement.RegisterCallback<AttachToPanelEvent>(_ => SaintsEditor.AddInstance(doTweenPlayRecorder));
            bodyElement.RegisterCallback<DetachFromPanelEvent>(_ => SaintsEditor.RemoveInstance(doTweenPlayRecorder));
#endif

            // if (saintsRowAttribute?.Inline ?? false)
            // {
            //     root.Add(bodyElement);
            //     return;
            // }

            // bodyElement.style.paddingLeft = SaintsPropertyDrawer.IndentWidth;

            // Debug.Log(property.isExpanded);

            // Foldout toggle = new Foldout
            // {
            //     text = property.displayName,
            //     // value = property.isExpanded,
            //     value = true,
            // };
            //
            // // bodyElement.style.display = property.isExpanded ? DisplayStyle.Flex : DisplayStyle.None;
            // toggle.RegisterValueChangedCallback(evt =>
            // {
            //     property.isExpanded = evt.newValue;
            //     // bodyElement.style.display = evt.newValue ? DisplayStyle.Flex : DisplayStyle.None;
            // });
            //
            // toggle.Add(bodyElement);
            //
            // root.Add(toggle);
            // root.Add(bodyElement);

        }
    }
}
#endif
