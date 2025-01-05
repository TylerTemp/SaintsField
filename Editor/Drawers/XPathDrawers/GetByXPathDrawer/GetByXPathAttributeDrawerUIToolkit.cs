#if UNITY_2021_3_OR_NEWER
using System;
using System.Collections.Generic;
using System.Reflection;
using SaintsField.Editor.Utils;
using SaintsField.Utils;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

namespace SaintsField.Editor.Drawers.XPathDrawers.GetByXPathDrawer
{
    public partial class GetByXPathAttributeDrawer
    {
        private static string ClassArrayContainer(SerializedProperty property, int index) => $"{property.propertyPath}_{index}__GetByXPath";
        private static string ClassContainer(SerializedProperty property) => $"{property.propertyPath}__GetByXPath";
        // private static string ClassAttributesContainer(SerializedProperty property) => $"{property.propertyPath}__GetByXPath_Attributes";
        private static string NameContainer(SerializedProperty property, int index) => $"{property.propertyPath}_{index}__GetByXPath";

        private static string NameHelpBox(SerializedProperty property, int index) => $"{property.propertyPath}_{index}__GetByXPath_HelpBox";
        private static string NameResignButton(SerializedProperty property, int index) => $"{property.propertyPath}_{index}__GetByXPath_ResignButton";
        private static string NameRemoveButton(SerializedProperty property, int index) => $"{property.propertyPath}_{index}__GetByXPath_RemoveButton";
        private static string NameSelectorButton(SerializedProperty property, int index) => $"{property.propertyPath}_{index}__GetByXPath_SelectorButton";

        // private const string ClassGetByXPath = "saints-field-get-by-xpath-attribute-drawer";

        protected override VisualElement CreatePostFieldUIToolkit(SerializedProperty property,
            ISaintsAttribute saintsAttribute, int index, VisualElement container, FieldInfo info, object parent)
        {
            string className = ClassContainer(property);
            // GetByXPathAttribute getByXPathAttribute = (GetByXPathAttribute)saintsAttribute;

            VisualElement root = new VisualElement
            {
                style =
                {
                    flexDirection = FlexDirection.Row,
                    flexGrow = 0,
                    flexShrink = 1,
                },
                name = NameContainer(property, index),
            };
            root.AddToClassList(className);

            Button refreshButton = new Button
            {
                style =
                {
                    height = SingleLineHeight,
                    width = SingleLineHeight,
                    display = DisplayStyle.None,
                    paddingLeft = 0,
                    paddingRight = 0,
                    paddingTop = 0,
                    paddingBottom = 0,
                    marginLeft = 0,
                    marginRight = 0,
                    marginTop = 0,
                    marginBottom = 0,
                },
                name = NameResignButton(property, index),
            };
            refreshButton.Add(new Image
            {
                image = Util.LoadResource<Texture2D>("refresh.png"),
            });

            Button removeButton = new Button
            {
                style =
                {
                    height = SingleLineHeight,
                    width = SingleLineHeight,
                    display = DisplayStyle.None,
                    paddingLeft = 0,
                    paddingRight = 0,
                    paddingTop = 0,
                    paddingBottom = 0,
                    marginLeft = 0,
                    marginRight = 0,
                    marginTop = 0,
                    marginBottom = 0,
                },
                name = NameRemoveButton(property, index),
            };
            removeButton.Add(new Image
            {
                image = Util.LoadResource<Texture2D>("close.png"),
            });

            Button selectorButton = new Button
            {
                text = "●",
                style =
                {
                    width = SingleLineHeight,
                    display = DisplayStyle.None,
                    marginLeft = 0,
                    marginRight = 0,
                },
                name = NameSelectorButton(property, index),
            };

            root.Add(refreshButton);
            root.Add(removeButton);
            root.Add(selectorButton);
            root.AddToClassList(ClassAllowDisable);

            return root;
        }

        protected override VisualElement CreateBelowUIToolkit(SerializedProperty property,
            ISaintsAttribute saintsAttribute, int index, VisualElement container, FieldInfo info, object parent)
        {
            HelpBox helpBox = new HelpBox("", HelpBoxMessageType.Error)
            {
                style =
                {
                    display = DisplayStyle.None,
                },
                name = NameHelpBox(property, index),
            };
            helpBox.AddToClassList(ClassAllowDisable);
            return helpBox;
        }

        protected override void OnAwakeUIToolkit(SerializedProperty property, ISaintsAttribute saintsAttribute, int index, VisualElement container,
            Action<object> onValueChangedCallback, FieldInfo info, object parent)
        {
            if (EditorApplication.isPlaying)
            {
                ImGuiSharedCache.Clear();
                return;
            }

            string arrayRemovedKey = SerializedUtils.GetUniqueIdArray(property);

            // watch selection
            Object curInspectingTarget = property.serializedObject.targetObject;
            container.RegisterCallback<DetachFromPanelEvent>(_ =>
            {
                // ReSharper disable once InvertIf
                if (Array.IndexOf(Selection.objects, curInspectingTarget) == -1)
                {
#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_GET_BY_XPATH
                    Debug.Log($"#GetByXPath# CleanUp {arrayRemovedKey}");
#endif
                    ImGuiSharedCache.Remove(arrayRemovedKey);
                }
            });

            bool configExists = ImGuiSharedCache.TryGetValue(arrayRemovedKey, out GetByXPathGenericCache genericCache);
            bool needUpdate = !configExists;
            if (configExists)
            {
                double curTime = EditorApplication.timeSinceStartup;
                double loopInterval = SaintsFieldConfigUtil.GetByXPathLoopIntervalMs();
                needUpdate = curTime - genericCache.UpdatedLastTime > loopInterval / 1000f;
                // if(needUpdate)
                // {
                //     Debug.Log($"needUpdate: {curTime - genericCache.UpdatedLastTime} > {loopInterval / 1000f}");
                // }
            }

            if (needUpdate)
            {
                if (genericCache == null)
                {
                    genericCache = new GetByXPathGenericCache();
                }
#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_GET_BY_XPATH
                Debug.Log($"#GetByXPath# UpdateImGuiSharedCache for {arrayRemovedKey} ({property.propertyPath}), firstTime={!configExists}");
#endif
                UpdateSharedCache(genericCache, !configExists, property, info, false);
                ImGuiSharedCache[arrayRemovedKey] = genericCache;
            }

            if (!ReferenceEquals(genericCache.GetByXPathAttributes[0], saintsAttribute))
            {
                return;
            }

            int propertyIndex = SerializedUtils.PropertyPathIndex(property.propertyPath);
            PropertyCache propertyCache = genericCache.IndexToPropertyCache[propertyIndex];

            VisualElement root = container.Q<VisualElement>(NameContainer(property, index));

            if (propertyCache.Error != "")
            {
                SetErrorMessage(propertyCache.Error, root, property, index);
                return;
            }

            Button refreshButton = root.Q<Button>(NameResignButton(property, index));
            Button removeButton = root.Q<Button>(NameRemoveButton(property, index));
            Button selectorButton = root.Q<Button>(NameSelectorButton(property, index));

            refreshButton.clicked += () =>
            {
                DoSignPropertyCache(genericCache.IndexToPropertyCache[propertyIndex]);
                property.serializedObject.ApplyModifiedProperties();
                refreshButton.style.display = DisplayStyle.None;
                onValueChangedCallback.Invoke(genericCache.IndexToPropertyCache[propertyIndex].OriginalValue);
            };

            removeButton.clicked += () =>
            {
                DoSignPropertyCache(genericCache.IndexToPropertyCache[propertyIndex]);
                property.serializedObject.ApplyModifiedProperties();
                removeButton.style.display = DisplayStyle.None;
                onValueChangedCallback.Invoke(genericCache.IndexToPropertyCache[propertyIndex].OriginalValue);
            };

            GetByXPathAttribute getByXPathAttribute = genericCache.GetByXPathAttributes[0];

            if (getByXPathAttribute.UsePickerButton)
            {
                selectorButton.style.display = DisplayStyle.Flex;
                selectorButton.clicked += () =>
                {
                    object updatedParent = SerializedUtils.GetFieldInfoAndDirectParent(property).parent;
                    if (updatedParent == null)
                    {
                        Debug.LogWarning($"{property.propertyPath} parent disposed unexpectedly");
                        return;
                    }

                    OpenPicker(property, info, genericCache.GetByXPathAttributes, genericCache.ExpectedType, genericCache.ExpectedInterface,
                        newValue =>
                        {
                            propertyCache.TargetValue = newValue;
                            DoSignPropertyCache(propertyCache);
                            property.serializedObject.ApplyModifiedProperties();
                            onValueChangedCallback.Invoke(newValue);
                        }, updatedParent);
                };
            }
            IVisualElementScheduledItem task = container.schedule.Execute(() =>
            {
                // ActualUpdateUIToolkit(property, index, container, onValueChangedCallback, info, true);
                int loop = SaintsFieldConfigUtil.GetByXPathLoopIntervalMs();
                if (loop > 0)
                {
                    container.schedule.Execute(() =>
                        ActualUpdateUIToolkit(property, index, container, onValueChangedCallback,
                            info)).Every(loop);
                }
                else
                {
                    ActualUpdateUIToolkit(property, index, container, onValueChangedCallback, info);
                }
            });
            int delay = SaintsFieldConfigUtil.GetByXPathDelayMs();

            if (delay > 0)
            {
                task.StartingIn(delay);
            }
        }

        // private string _toastInfoUIToolkit = "";
        // private static readonly HashSet<string> ToastInfoUIToolkit = new HashSet<string>();

        private static void ActualUpdateUIToolkit(SerializedProperty property, int index,
            VisualElement container, Action<object> onValueChanged, FieldInfo info)
        {
            if (EditorApplication.isPlaying)
            {
                return;
            }

            string arrayRemovedKey = SerializedUtils.GetUniqueIdArray(property);

            bool configExists = ImGuiSharedCache.TryGetValue(arrayRemovedKey, out GetByXPathGenericCache genericCache);
            bool needUpdate = !configExists;
            if (configExists)
            {
                double curTime = EditorApplication.timeSinceStartup;
                double loopInterval = SaintsFieldConfigUtil.GetByXPathLoopIntervalMsIMGUI();
                needUpdate = curTime - genericCache.UpdatedLastTime > loopInterval / 1000f;
                // if(needUpdate)
                // {
                //     Debug.Log($"needUpdate: {curTime - genericCache.UpdatedLastTime} > {loopInterval / 1000f}");
                // }
            }

            if (needUpdate)
            {
                if (genericCache == null)
                {
                    genericCache = new GetByXPathGenericCache();
                }
#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_GET_BY_XPATH
                Debug.Log($"#GetByXPath# UpdateImGuiSharedCache for {arrayRemovedKey} ({property.propertyPath}), firstTime={!configExists}");
#endif
                UpdateSharedCache(genericCache, !configExists, property, info, false);
                ImGuiSharedCache[arrayRemovedKey] = genericCache;
            }

            VisualElement firstRoot = container.Q<VisualElement>(NameContainer(property, index));
            if (firstRoot == null)
            {
#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_GET_BY_XPATH
                Debug.Log($"{property.propertyPath} no root");
#endif
                return;
            }

            int propertyIndex = SerializedUtils.PropertyPathIndex(property.propertyPath);
            // update information for this property
            if (!genericCache.IndexToPropertyCache.TryGetValue(propertyIndex, out PropertyCache propertyCache))
            {
                return;
            }
            GetByXPathAttribute firstAttribute = genericCache.GetByXPathAttributes[0];
            Button refreshButton = container.Q<Button>(NameResignButton(property, index));
            Button removeButton = container.Q<Button>(NameRemoveButton(property, index));
            if (propertyCache.MisMatch)
            {
                if (firstAttribute.UseResignButton)
                {


                    bool targetIsNull = propertyCache.TargetIsNull;
                    if (targetIsNull)
                    {
                        if (removeButton.style.display != DisplayStyle.Flex)
                        {
                            removeButton.style.display = DisplayStyle.Flex;
                        }

                        if (refreshButton.style.display != DisplayStyle.None)
                        {
                            refreshButton.style.display = DisplayStyle.None;
                        }
                    }
                    else
                    {
                        if (removeButton.style.display != DisplayStyle.None)
                        {
                            removeButton.style.display = DisplayStyle.None;
                        }

                        if (refreshButton.style.display != DisplayStyle.Flex)
                        {
                            refreshButton.style.display = DisplayStyle.Flex;
                        }
                    }
                }
                else if (firstAttribute.UseErrorMessage)
                {
                    SetErrorMessage(
                        GetMismatchErrorMessage(propertyCache.OriginalValue, propertyCache.TargetValue, propertyCache.TargetIsNull),
                        firstRoot,
                        property,
                        index);
                }
            }
            else
            {
                SetErrorMessage(
                    "",
                    firstRoot,
                    property,
                    index);
                if (removeButton.style.display != DisplayStyle.None)
                {
                    removeButton.style.display = DisplayStyle.None;
                }
                if (refreshButton.style.display != DisplayStyle.None)
                {
                    refreshButton.style.display = DisplayStyle.None;
                }
            }
        }

        protected override void OnValueChanged(SerializedProperty property, ISaintsAttribute saintsAttribute, int index,
            VisualElement container,
            FieldInfo info,
            object parent,
            Action<object> onValueChangedCallback,
            object newValue)
        {
            // ActualUpdateUIToolkit(property, index, container, onValueChangedCallback,
            //     info, false);
        }

        private static IEnumerable<(bool hasRoot, VisualElement root, bool hasValue, object value)> ZipTwoLongest(IEnumerable<VisualElement> left, IEnumerable<object> right)
        {

            // IEnumerator<T> leftEnumerator = left.GetEnumerator();
            // IEnumerator<T> rightEnumerator = right.GetEnumerator();

            // ReSharper disable once ConvertToUsingDeclaration
            using(IEnumerator<VisualElement> leftEnumerator = left.GetEnumerator())
            using(IEnumerator<object> rightEnumerator = right.GetEnumerator())
            {
                bool hasLeft = leftEnumerator.MoveNext();
                bool hasRight = rightEnumerator.MoveNext();

                while (hasLeft || hasRight)
                {
                    // ReSharper disable once ConvertIfStatementToSwitchStatement
                    if (hasLeft && hasRight)
                    {
                        yield return (true, leftEnumerator.Current, true, rightEnumerator.Current);
                    }
                    else if (hasLeft)
                    {
                        yield return (true, leftEnumerator.Current, false, default);
                    }
                    else
                    {
                        yield return (false, default, true, rightEnumerator.Current);
                    }

                    hasLeft = leftEnumerator.MoveNext();
                    hasRight = rightEnumerator.MoveNext();
                }
            }
        }

        private static void UpdateErrorMessage(GetByXPathAttribute getByXPathAttribute, VisualElement root, CheckFieldResult checkFieldResult, SerializedProperty property, int index)
        {
            string error = checkFieldResult.Error;
            // ReSharper disable once MergeIntoPattern
            if(checkFieldResult.Error == "" && getByXPathAttribute.UseErrorMessage)
            {
                if(checkFieldResult.MisMatch)
                {
                    error = $"Expected {(Util.IsNull(checkFieldResult.TargetValue)? "nothing": checkFieldResult.TargetValue)}, but got {(Util.IsNull(checkFieldResult.OriginalValue)? "Null": checkFieldResult.OriginalValue)}";
                }
            }

            HelpBox helpBox = root.Q<HelpBox>(NameHelpBox(property, index));
            if (helpBox.text == error)
            {
                return;
            }

            helpBox.style.display = error == "" ? DisplayStyle.None : DisplayStyle.Flex;
            helpBox.text = error;
        }

        private static void SetErrorMessage(string error, VisualElement root, SerializedProperty property, int index)
        {
            HelpBox helpBox = root.Q<HelpBox>(NameHelpBox(property, index));
            if (helpBox.text == error)
            {
                return;
            }

            helpBox.style.display = error == "" ? DisplayStyle.None : DisplayStyle.Flex;
            helpBox.text = error;
        }


    }
}
#endif
