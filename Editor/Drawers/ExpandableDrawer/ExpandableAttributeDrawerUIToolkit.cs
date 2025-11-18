#if UNITY_2021_3_OR_NEWER
using System;
using System.Collections.Generic;
using System.Reflection;
using SaintsField.Editor.Core;
using SaintsField.Editor.Linq;
using SaintsField.Editor.Utils;
using SaintsField.Interfaces;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

namespace SaintsField.Editor.Drawers.ExpandableDrawer
{
    public partial class ExpandableAttributeDrawer
    {

        private static string NameFoldout(SerializedProperty property) =>
            $"{property.propertyPath}__ExpandableAttributeDrawer_Foldout";

        private static string NameProps(SerializedProperty property) =>
            $"{property.propertyPath}__ExpandableAttributeDrawer_Props";

        protected override VisualElement CreatePostOverlayUIKit(SerializedProperty property,
            ISaintsAttribute saintsAttribute, int index,
            VisualElement container, object parent)
        {
            Foldout foldOut = new Foldout
            {
                style =
                {
                    // backgroundColor = Color.green,
                    // left = -5,
                    position = Position.Absolute,
                    width = LabelBaseWidth - IndentWidth,
                },
                name = NameFoldout(property),
                // This is broken and not fixed even in Unity 6k
                // viewDataKey = SerializedUtils.GetUniqueId(property),
                // userData = container,
                // value = false,
                value = property.isExpanded,
            };
            Toggle toggle = foldOut.Q<Toggle>();
            if (toggle != null)
            {
                toggle.userData = container;
            }

            return foldOut;
        }

        private static readonly Color BackgroundColor = EColor.EditorEmphasized.GetColor();

        protected override VisualElement CreateBelowUIToolkit(SerializedProperty property,
            ISaintsAttribute saintsAttribute, int index,
            IReadOnlyList<PropertyAttribute> allAttributes,
            VisualElement container, FieldInfo info, object parent)
        {
            VisualElement visualElement = new VisualElement
            {
                style =
                {
                    display = DisplayStyle.None,
                    backgroundColor = BackgroundColor,
                },
                name = NameProps(property),
                userData = null,
            };

            visualElement.AddToClassList(ClassAllowDisable);

            return visualElement;
        }

        protected override void OnAwakeUIToolkit(SerializedProperty property, ISaintsAttribute saintsAttribute, int index,
            IReadOnlyList<PropertyAttribute> allAttributes, VisualElement container, Action<object> onValueChangedCallback, FieldInfo info, object parent)
        {
            Foldout foldOut = container.Q<Foldout>(NameFoldout(property));

            UIToolkitUtils.AddContextualMenuManipulator(foldOut, property, () => Util.PropertyChangedCallback(property, info, onValueChangedCallback));

            VisualElement nameProp = container.Q<VisualElement>(NameProps(property));

            foldOut.RegisterValueChangedCallback(v =>
            {
                UpdatePropsDisplay(v.newValue);
            });
            UpdatePropsDisplay(foldOut.value);

            UpdateDisplay();
            SaintsEditorApplicationChanged.OnAnyEvent.AddListener(UpdateDisplay);
            foldOut.RegisterCallback<DetachFromPanelEvent>(_ => SaintsEditorApplicationChanged.OnAnyEvent.RemoveListener(UpdateDisplay));
            foldOut.TrackSerializedObjectValue(property.serializedObject, _ => UpdateDisplay());

            return;

            void UpdateDisplay()
            {
                object refreshParent = SerializedUtils.GetFieldInfoAndDirectParent(property).parent;
                if (refreshParent == null)
                {
                    Debug.LogWarning($"{property.propertyPath} parent disposed unexpectedly.");
                    return;
                }

                // if (!foldOut.value)
                // {
                //     return;
                // }

                VisualElement propsElement = container.Q<VisualElement>(NameProps(property));
                SerializedObject curObject = (SerializedObject)propsElement.userData;

                SerializedObject serObject = GetSerObject(property, info, refreshParent);

                if (EqualSerObject(serObject, curObject))
                {
                    return;
                }

                DisplayStyle foldoutDisplay = serObject == null ? DisplayStyle.None : DisplayStyle.Flex;
                if (foldOut.style.display != foldoutDisplay)
                {
                    foldOut.style.display = foldoutDisplay;
                }

                propsElement.userData = serObject;
                propsElement.Clear();
                if (serObject == null)
                {
                    return;
                }

                if (serObject.targetObject is GameObject go)
                {
                    if (serObject.targetObjects.Length > 1)
                    {
                        propsElement.Add(new HelpBox("Multiple GameObjects inspecting is not supported. Only the first one is shown.", HelpBoxMessageType.Warning));
                    }

                    // wtf Unity you can not inspect GameObject?
                    foreach ((Component comp, int compIndex) in go.GetComponents<Component>().WithIndex())
                    {
                        VisualElement subContainer = new VisualElement
                        {
                            style =
                            {
                                backgroundColor = BackgroundColor,
                                marginTop = 2,
                                marginBottom = 2,
                            },
                        };

                        string name = ObjectNames.NicifyVariableName(comp.GetType().Name);
                        Foldout foldout = new Foldout
                        {
                            text = name,
                            // value = true,
                            style =
                            {
                                marginLeft = 15,
                            },
                            viewDataKey = $"{SerializedUtils.GetUniqueId(property)}_{comp}{compIndex}",
                        };
                        InspectorElement inspectorElement = new InspectorElement(comp);

                        foldout.Add(inspectorElement);
                        subContainer.Add(foldout);
                        propsElement.Add(subContainer);

                        VisualElement foldoutContent = foldout.Q<VisualElement>(classes: "unity-foldout__content");
                        if (foldoutContent != null)
                        {
                            foldoutContent.style.marginLeft = 0;
                        }
                    }

                    propsElement.style.backgroundColor = Color.clear;
                }
                else
                {
                    InspectorElement inspectorElement = new InspectorElement(serObject)
                    {
                        userData = serObject,
                    };

                    propsElement.Add(inspectorElement);
                    propsElement.style.backgroundColor = BackgroundColor;
                }
            }

            void UpdatePropsDisplay(bool newValue)
            {
                property.isExpanded = newValue;
                // Debug.Log($"UpdatePropsDisplay={newValue}");
                DisplayStyle display =
                    newValue ? DisplayStyle.Flex : DisplayStyle.None;
                if (nameProp.style.display != display)
                {
                    nameProp.style.display = display;
                }
            }
        }
    }
}
#endif
