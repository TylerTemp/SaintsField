#if UNITY_2021_3_OR_NEWER

using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using SaintsField.Editor.Core;
using SaintsField.Editor.Utils;
using SaintsField.Interfaces;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace SaintsField.Editor.Drawers.ButtonDrawers.DecButtonDrawer
{
    public partial class DecButtonAttributeDrawer
    {


        // private static string ClassButton(SerializedProperty property) => $"{property.propertyPath}__Button";
        private static string ClassLabelContainer(SerializedProperty property, int index) => $"{property.propertyPath}__{index}__LabelContainer";
        private static string ClassLabelError(SerializedProperty property, int index) => $"{property.propertyPath}__{index}__LabelError";
        private static string ClassExecError(SerializedProperty property, int index) => $"{property.propertyPath}__{index}__ExecError";
        private static string NameButtonRotator(SerializedProperty property, int index) => $"{property.propertyPath}__{index}__ButtonRatator";

        protected static VisualElement DrawUIToolkit(SerializedProperty property, ISaintsAttribute saintsAttribute,
            int index, FieldInfo info, object parent, VisualElement container)
        {
            // Button buttonElement = null;
            IVisualElementScheduledItem buttonTask = null;
            HashSet<IEnumerator> enumerators = new HashSet<IEnumerator>();
            Image buttonRotator = new Image
            {
                image = Util.LoadResource<Texture2D>("refresh.png"),
                style =
                {
                    position = Position.Absolute,
                    width = EditorGUIUtility.singleLineHeight - 2,
                    height = EditorGUIUtility.singleLineHeight - 2,
                    left = 1,
                    top = 1,
                    opacity = 0.5f,
                    display = DisplayStyle.None,
                },
                tintColor = EColor.Lime.GetColor(),
                name = NameButtonRotator(property, index),
            };
            Button buttonElement = new Button(() =>
            {
                HelpBox helpBox = container.Query<HelpBox>(className: ClassExecError(property, index)).First();
                string buttonError = "";
                // ReSharper disable once PossibleNullReferenceException
                // ReSharper disable once AccessToModifiedClosure
                // HashSet<IEnumerator> enumerators = (HashSet<IEnumerator>)buttonElement.userData;
                foreach ((string eachError, object buttonResult) in CallButtonFunc(property, (DecButtonAttribute) saintsAttribute, info, parent))
                {
                    // Debug.Log($"{eachError}/{buttonResult}");
                    if (eachError == "")
                    {
                        // Debug.Log(buttonResult is IEnumerator);
                        if (buttonResult is IEnumerator enumerator)
                        {
                            enumerators.Add(enumerator);
                        }
                    }
                    else
                    {
                        buttonError += eachError;
                    }
                }

                buttonTask?.Pause();

                if (enumerators.Count > 0)
                {
                    // ReSharper disable once PossibleNullReferenceException
                    // ReSharper disable once AccessToModifiedClosure
                    buttonTask = container.schedule.Execute(() =>
                    {
                        HashSet<IEnumerator> completedEnumerators = new HashSet<IEnumerator>();

                        foreach (IEnumerator enumerator in enumerators)
                        {
                            if (!enumerator.MoveNext())
                            {
                                completedEnumerators.Add(enumerator);
                            }
                        }

                        enumerators.ExceptWith(completedEnumerators);
                        bool show = enumerators.Count > 0;

                        DisplayStyle style = show? DisplayStyle.Flex : DisplayStyle.None;
                        if(buttonRotator.style.display != style)
                        {
                            buttonRotator.style.display = style;
                        }
                    }).Every(1);
                }

                helpBox.style.display = buttonError == ""? DisplayStyle.None: DisplayStyle.Flex;
                helpBox.text = buttonError;
            })
            {
                style =
                {
                    height = EditorGUIUtility.singleLineHeight,
                    flexGrow = 1,
                },
                userData = new HashSet<IEnumerator>(),
            };

            buttonElement.Add(buttonRotator);

            VisualElement labelContainer = new VisualElement
            {
                style =
                {
                    flexDirection = FlexDirection.Row,
                    // flexGrow = 1,
                    justifyContent = Justify.Center,  // horizontal
                    alignItems = Align.Center,  // vertical
                },
                userData = "",
            };
            labelContainer.AddToClassList(ClassLabelContainer(property, index));
            // labelContainer.Add(new Label("test label"));

            buttonElement.Add(labelContainer);
            // button.AddToClassList();
            buttonElement.AddToClassList(ClassAllowDisable);
            return buttonElement;
        }

        protected override void OnAwakeUIToolkit(SerializedProperty property, ISaintsAttribute saintsAttribute, int index,
            IReadOnlyList<PropertyAttribute> allAttributes, VisualElement container, Action<object> onValueChangedCallback, FieldInfo info, object parent)
        {
            Image buttonRotator = container.Q<Image>(name: NameButtonRotator(property, index));
            // UIToolkitUtils.TriggerRotate(buttonRotator);
            UIToolkitUtils.KeepRotate(buttonRotator);
            buttonRotator.schedule.Execute(() => UIToolkitUtils.TriggerRotate(buttonRotator));
            // Debug.Log("TriggerRotate");
        }

        protected static HelpBox DrawLabelError(SerializedProperty property, int index) => DrawError(ClassLabelError(property, index));

        protected static HelpBox DrawExecError(SerializedProperty property, int index) => DrawError(ClassExecError(property, index));

        private static HelpBox DrawError(string className)
        {
            HelpBox helpBox = new HelpBox("", HelpBoxMessageType.Error)
            {
                style =
                {
                    display = DisplayStyle.None,
                },
            };
            helpBox.AddToClassList(className);
            helpBox.AddToClassList(ClassAllowDisable);
            return helpBox;
        }

        protected override void OnUpdateUIToolkit(SerializedProperty property, ISaintsAttribute saintsAttribute,
            int index,
            IReadOnlyList<PropertyAttribute> allAttributes,
            VisualElement container, Action<object> onValueChangedCallback, FieldInfo info)
        {
            // if (parent == null)
            // {
            //     return;
            // }

            VisualElement labelContainer = container.Query<VisualElement>(className: ClassLabelContainer(property, index)).First();
            string oldXml = (string)labelContainer.userData;
            DecButtonAttribute decButtonAttribute = (DecButtonAttribute) saintsAttribute;

            object parent = SerializedUtils.GetFieldInfoAndDirectParent(property).parent;
            (string xmlError, string newXml) = RichTextDrawer.GetLabelXml(property, decButtonAttribute.ButtonLabel, decButtonAttribute.IsCallback, info, parent);

            // ReSharper disable once ConvertIfStatementToNullCoalescingAssignment
            if (newXml == null)
            {
                newXml = ObjectNames.NicifyVariableName(decButtonAttribute.FuncName);
            }

            HelpBox helpBox = container.Query<HelpBox>(className: ClassLabelError(property, index)).First();
            helpBox.style.display = xmlError == ""? DisplayStyle.None: DisplayStyle.Flex;
            helpBox.text = xmlError;

            if (oldXml == newXml)
            {
                return;
            }

            // Debug.Log($"update xml={newXml}");

            labelContainer.userData = newXml;
            labelContainer.Clear();
            IEnumerable<RichTextDrawer.RichTextChunk> richChunks = RichTextDrawer.ParseRichXml(newXml, property.displayName, property, info, parent);
            foreach (VisualElement visualElement in RichTextDrawer.DrawChunksUIToolKit(richChunks))
            {
                labelContainer.Add(visualElement);
            }
        }
    }
}
#endif
