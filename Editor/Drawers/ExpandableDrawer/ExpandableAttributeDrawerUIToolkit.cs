#if UNITY_2021_3_OR_NEWER
using System;
using System.Reflection;
using SaintsField.Editor.Utils;
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
                value = false,
            };

            foldOut.RegisterValueChangedCallback(v =>
            {
                property.isExpanded = v.newValue;
                container.Q<VisualElement>(NameProps(property)).style.display =
                    v.newValue ? DisplayStyle.Flex : DisplayStyle.None;
            });

            return foldOut;
        }

        private static readonly Color BackgroundColor = EColor.EditorEmphasized.GetColor();

        protected override VisualElement CreateBelowUIToolkit(SerializedProperty property,
            ISaintsAttribute saintsAttribute, int index,
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

        protected override void OnUpdateUIToolkit(SerializedProperty property, ISaintsAttribute saintsAttribute,
            int index,
            VisualElement container, Action<object> onValueChangedCallback, FieldInfo info)
        {
            object parent = SerializedUtils.GetFieldInfoAndDirectParent(property).parent;
            if (parent == null)
            {
                Debug.LogWarning($"{property.propertyPath} parent disposed unexpectedly.");
                return;
            }

            Foldout foldOut = container.Q<Foldout>(NameFoldout(property));
            if (!foldOut.value)
            {
                return;
            }

            VisualElement propsElement = container.Q<VisualElement>(NameProps(property));
            Object curObject = (Object)propsElement.userData;

            Object serObject = GetSerObject(property, info, parent);
            // Debug.Log(serObject);

            if (ReferenceEquals(serObject, curObject))
            {
                // Debug.Log($"equal: {serObject}/{curObject}");
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

            if (serObject is GameObject go)
            {
                // wtf Unity you can not inspect GameObject?
                foreach (Component comp in go.GetComponents<Component>())
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
                        value = true,
                        style =
                        {
                            marginLeft = 15,
                        },
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
                InspectorElement inspectorElement = new InspectorElement(serObject);
                propsElement.Add(inspectorElement);
                propsElement.style.backgroundColor = BackgroundColor;
            }
        }

    }
}
#endif
