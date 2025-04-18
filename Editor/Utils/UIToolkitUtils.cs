

using SaintsField.Interfaces;
#if UNITY_2021_3_OR_NEWER
using SaintsField.Editor.Drawers.SaintsRowDrawer;
using UnityEditor.UIElements;
using System.Collections.Generic;
using SaintsField.Editor.Core;
using UnityEditor;
using System;
using System.Linq;
using System.Reflection;
using SaintsField.Editor.Playa;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.UIElements;
#endif

namespace SaintsField.Editor.Utils
{
#if UNITY_2021_3_OR_NEWER
    public static class UIToolkitUtils
    {

        public const string NoDecoratorDrawerUssFile = "UIToolkit/NoDecoratorDrawer.uss";

        public static void SetPropertyFieldDrawNestingLevel1(PropertyField prop)
        {
            FieldInfo fieldInfo = typeof(PropertyField).GetField("m_DrawNestingLevel", BindingFlags.NonPublic | BindingFlags.Instance);
            if (fieldInfo != null)
            {
                fieldInfo.SetValue(prop, 1);
            }
        }

        public static void FixLabelWidthLoopUIToolkit(Label label)
        {
            // FixLabelWidthUIToolkit(label);
            // label.schedule.Execute(() => FixLabelWidthUIToolkit(label)).StartingIn(250);
            label.RegisterCallback<GeometryChangedEvent>(evt => FixLabelWidthUIToolkit((Label)evt.target));
        }

        public static void KeepRotate(VisualElement element)
        {
            StyleSheet rotateUss = Util.LoadResource<StyleSheet>("UIToolkit/SaintsRotate.uss");
            element.styleSheets.Add(rotateUss);
            element.AddToClassList("saints-rotate");
            element.RegisterCallback<TransitionEndEvent>(e =>
            {
                // Debug.Log(buttonRotator.style.rotate);
                element.RemoveFromClassList("saints-rotate");

                StyleRotate rotateReset = element.style.rotate;
                rotateReset.value = new Rotate(0);
                element.style.rotate = rotateReset;
                // Debug.Log(buttonRotator.style.rotate);

                element.schedule.Execute(() =>
                {
                    element.AddToClassList("saints-rotate");
                    element.schedule.Execute(() =>
                    {
                        TriggerRotate(element);
                        // StyleRotate rotate = element.style.rotate;
                        // rotate.value = new Rotate(360);
                        // element.style.rotate = rotate;
                        // Debug.Log($"restart to {buttonRotator.style.rotate}");
                    });
                });

                // Debug.Log(e);
            });

            // element.schedule.Execute(() =>
            // {
            //     StyleRotate rotate = element.style.rotate;
            //     rotate.value = new Rotate(360);
            //     element.style.rotate = rotate;
            // });
        }

        public static void TriggerRotate(VisualElement element)
        {
            StyleRotate rotate = element.style.rotate;
            rotate.value = new Rotate(360);
            element.style.rotate = rotate;
        }

        // ReSharper disable once SuggestBaseTypeForParameter
        public static void FixLabelWidthUIToolkit(Label label)
        {
            // label.ClearClassList();
            label.RemoveFromClassList(BaseField<object>.alignedFieldUssClassName);
            // label.style.minWidth = 200;

            StyleLength autoLength = new StyleLength(StyleKeyword.Auto);
            StyleLength curLenght = label.style.width;
            float resolvedWidth = label.resolvedStyle.width;
            // if(curLenght.value != autoLength)
            // don't ask me why we need to compare with 0, ask Unity...
            if(
                // !(curLenght.value.IsAuto()  // IsAuto() is not available in 2021.3.0f1
                !(curLenght.keyword == StyleKeyword.Auto
                  || curLenght.value == 0)
                && !float.IsNaN(resolvedWidth) && resolvedWidth > 0)
            {
                label.style.width = autoLength;
                label.style.minWidth = 0;
                // label.schedule.Execute(() => label.style.width = autoLength);
            }
        }

        public static void WaitUntilThenDo<T>(VisualElement container, Func<(bool ok, T result)> until, Action<T> thenDo, long delay=0)
        {
            (bool ok, T result) = until.Invoke();
            if (ok)
            {
                thenDo.Invoke(result);
                return;
            }

            if(delay > 1000)
            {
                return;
            }

            // if (delay <= 0)
            // {
            //     container.schedule.Execute(() =>
            //     {
            //         (bool ok, T result) = until.Invoke();
            //         if (ok)
            //         {
            //             thenDo.Invoke(result);
            //         }
            //     });
            // }

            container.schedule.Execute(() => WaitUntilThenDo(container, until, thenDo, delay+200)).StartingIn(delay);
        }

        public static void ChangeLabelLoop(VisualElement container, IEnumerable<RichTextDrawer.RichTextChunk> chunksOrNull, RichTextDrawer richTextDrawer)
        {
            // container.RegisterCallback<GeometryChangedEvent>(evt => ChangeLabel((VisualElement)evt.target, chunks));
            ChangeLabel(container, chunksOrNull, richTextDrawer, 0f);
        }

        private static void ChangeLabel(VisualElement container, IEnumerable<RichTextDrawer.RichTextChunk> chunksOrNull, RichTextDrawer richTextDrawer, float delayTime)
        {
            if (delayTime > 1f)  // stop trying after 1 second
            {
                IMGUIContainer imguiContainer = container.Q<IMGUIContainer>(className: IMGUILabelHelper.ClassName);
                if (imguiContainer?.userData is IMGUILabelHelper imguiLabelHelper)
                {
                    if (chunksOrNull is null)
                    {
                        imguiLabelHelper.NoLabel = true;
                        return;
                    }

                    // ReSharper disable once PossibleMultipleEnumeration
                    RichTextDrawer.RichTextChunk[] chunks = chunksOrNull.ToArray();
                    string labelString = string.Join("", chunks.Where(each => !each.IsIcon).Select(each => each.Content));
                    imguiLabelHelper.RichLabel = labelString;
                    imguiLabelHelper.NoLabel = false;
                }
                return;
            }

            Label label = container.Q<Label>(className: "unity-label");
            if (label == null)
            {
                container.schedule.Execute(() => ChangeLabel(container, chunksOrNull, richTextDrawer, delayTime + 0.3f));
                return;
            }

            SetLabel(label, chunksOrNull, richTextDrawer);
        }

        public static void SetLabel(Label label, IEnumerable<RichTextDrawer.RichTextChunk> chunksOrNull,
            RichTextDrawer richTextDrawer)
        {
            if (chunksOrNull == null)
            {
                label.style.display = DisplayStyle.None;
                return;
            }

            if (label.ClassListContains(SaintsPropertyDrawer.ClassNoRichLabelUpdate))
            {
                return;
            }

            label.Clear();
            label.text = "";
            label.style.flexDirection = FlexDirection.Row;
            // label.style.alignItems = Align.Center;
            // label.style.height = EditorGUIUtility.singleLineHeight;
            foreach (VisualElement richChunk in richTextDrawer.DrawChunksUIToolKit(chunksOrNull))
            {
                label.Add(richChunk);
            }
            label.style.display = DisplayStyle.Flex;
        }

        public static DropdownButtonField MakeDropdownButtonUIToolkit(string label)
        {
            Button button = new Button
            {
                style =
                {
                    height = EditorGUIUtility.singleLineHeight,
                    flexGrow = 1,
                    flexShrink = 1,

                    paddingRight = 2,
                    marginRight = 0,
                    marginLeft = 0,
                    alignItems = Align.FlexStart,
                    unityTextAlign = TextAnchor.MiddleLeft,
                },
                // name = NameButtonField(property),
                // userData = metaInfo.SelectedIndex == -1
                //     ? null
                //     : metaInfo.DropdownListValue[metaInfo.SelectedIndex].Item2,
            };

            Label buttonLabel = new Label
            {
                style =
                {
                    flexGrow = 1,
                    flexShrink = 1,
                    // paddingRight = 20,
                    // textOverflow = TextOverflow.Ellipsis,
                    // unityOverflowClipBox = OverflowClipBox.PaddingBox,
                    overflow = Overflow.Hidden,
                    marginRight = 15,
                    unityTextAlign = TextAnchor.MiddleLeft,
                },
            };

            button.Add(buttonLabel);

            DropdownButtonField dropdownButtonField = new DropdownButtonField(label, button, buttonLabel)
            {
                style =
                {
                    flexGrow = 1,
                    flexShrink = 1,
                },
            };

            // dropdownButtonField.AddToClassList("unity-base-field__aligned");
            dropdownButtonField.AddToClassList(BaseField<UnityEngine.Object>.alignedFieldUssClassName);

            dropdownButtonField.Add(new Image
            {
                image = Util.LoadResource<Texture2D>("classic-dropdown.png"),
                scaleMode = ScaleMode.ScaleToFit,
                style =
                {
                    maxWidth = 12,
                    maxHeight = EditorGUIUtility.singleLineHeight,
                    position = Position.Absolute,
                    right = 4,
                },
                pickingMode = PickingMode.Ignore,
            });

            return dropdownButtonField;
        }

        public static IEnumerable<VisualElement> FindParentName(VisualElement element, string className)
        {
            return IterUpWithSelf(element).Where(each => each.name == className);
        }

        public static IEnumerable<VisualElement> IterUpWithSelf(VisualElement element)
        {
            if(element == null)
            {
                yield break;
            }

            yield return element;

            foreach (VisualElement visualElement in IterUpWithSelf(element.parent))
            {
                yield return visualElement;
            }
        }


        /// <summary>
        /// Applies a color to a visual element via the color attribute
        /// <see href="https://github.com/v0lt13/EditorAttributes/blob/main/Editor/Scripts/Utilities/ColorUtils.cs" />
        /// </summary>
        /// <param name="visualElement">The visual element to color</param>
        /// <param name="color">The color attribute</param>
        public static void ApplyColor(VisualElement visualElement, Color color)
        {
            List<Label> labels = visualElement.Query<Label>().ToList();

            foreach (Label label in labels)
                label.style.color = color;

            List<TextElement> textElements = visualElement.Query<TextElement>().ToList();

            foreach (TextElement textElement in textElements)
                textElement.style.color = color;

            List<ScrollView> scrollViews = visualElement.Query<ScrollView>(className: "unity-collection-view__scroll-view").ToList();

            foreach (ScrollView scrollView in scrollViews)
                scrollView.style.backgroundColor = color / 3f;

            List<VisualElement> inputFields = visualElement.Query(className: "unity-property-field__input").ToList();

            foreach (VisualElement inputField in inputFields)
                inputField.style.backgroundColor = color / 3f;

            List<VisualElement> checkMarks = visualElement.Query(className: "unity-toggle__checkmark").ToList();

            foreach (VisualElement checkMark in checkMarks)
            {
                checkMark.style.unityBackgroundImageTintColor = color;
                checkMark.parent.style.backgroundColor = StyleKeyword.Initial;
            }
        }

#if UNITY_2021_3_OR_NEWER && !SAINTSFIELD_UI_TOOLKIT_DISABLE
        public static VisualElement CreateOrUpdateFieldProperty(
            SerializedProperty property,
            IReadOnlyList<PropertyAttribute> allAttributes,
            Type rawType,
            string label,
            FieldInfo fieldInfo,
            bool inHorizontalLayout,
            IMakeRenderer makeRenderer,
            IDOTweenPlayRecorder doTweenPlayRecorder,
            VisualElement originalField,
            object parent)
        {
            // PropertyField result = new PropertyField(FieldWithInfo.SerializedProperty)
            // {
            //     style =
            //     {
            //         flexGrow = 1,
            //     },
            //     name = FieldWithInfo.SerializedProperty.propertyPath,
            // };
            // result.Bind(FieldWithInfo.SerializedProperty.serializedObject);
            // return (result, false);


            // About letting SaintsPropertyDrawer fallback:
            // SaintsPropertyDrawer relays on PropertyField to fallback. Directly hi-jacking the drawer with SaintsPropertyDrawer
            // the workflow will still get into the PropertyField flow, then SaintsField will fail to decide when the
            // fallback should stop.

            Type useDrawerType = null;
            Attribute useAttribute = null;
            bool isArray = property.propertyType == SerializedPropertyType.Generic
                           && property.isArray;

            // bool useFallbackSaintsRow = false;
            // Debug.Log($"rendering {property.propertyPath}/{property.propertyType}/isArray={isArray}");
            if(!isArray)
            {
                ISaintsAttribute saintsAttr = allAttributes
                    .OfType<ISaintsAttribute>()
                    .FirstOrDefault();

                useAttribute = saintsAttr as Attribute;
                if (saintsAttr != null)
                {
                    useDrawerType = SaintsPropertyDrawer.GetFirstSaintsDrawerType(saintsAttr.GetType());
                }
                else
                {
                    (Attribute attrOrNull, Type drawerType) =
                        SaintsPropertyDrawer.GetFallbackDrawerType(fieldInfo,
                            property);
                    // Debug.Log($"{FieldWithInfo.SerializedProperty.propertyPath}: {drawerType}");
                    useAttribute = attrOrNull;
                    useDrawerType = drawerType;

                    // if (useDrawerType == null &&
                    //     property.propertyType == SerializedPropertyType.Generic)
                    // {
                    //     // useFallbackSaintsRow = true;
                    //     // useDrawerType = typeof(SaintsRowAttributeDrawer);
                    // }
                }
            }

            // List<(ISaintsAttribute Attribute, SaintsPropertyDrawer Drawer)> appendSaintsAttributeDrawer = null;
            //
            // if (!isArray && InHorizentalLayout)
            // {
            //     appendSaintsAttributeDrawer = new List<(ISaintsAttribute Attribute, SaintsPropertyDrawer Drawer)>();
            //     NoLabelAttribute noLabelAttribute = new NoLabelAttribute();
            //     RichLabelAttributeDrawer noLabelDrawer = (RichLabelAttributeDrawer)
            //         SaintsPropertyDrawer.MakePropertyDrawer(typeof(RichLabelAttributeDrawer),
            //             FieldWithInfo.FieldInfo, useAttribute, FieldWithInfo.SerializedProperty.displayName);
            //
            //     appendSaintsAttributeDrawer.Add((noLabelAttribute, noLabelDrawer));
            //
            //     // // ReSharper disable once RedundantArgumentDefaultValue
            //     // AboveRichLabelAttribute aboveRichLabelAttribute = new AboveRichLabelAttribute("<label />");
            //     // FullWidthRichLabelAttributeDrawer aboveRichLabelDrawer = (FullWidthRichLabelAttributeDrawer)
            //     //     SaintsPropertyDrawer.MakePropertyDrawer(typeof(FullWidthRichLabelAttributeDrawer),
            //     //         FieldWithInfo.FieldInfo, aboveRichLabelAttribute, FieldWithInfo.SerializedProperty.displayName);
            //     //
            //     // appendSaintsAttributeDrawer.Add((aboveRichLabelAttribute, aboveRichLabelDrawer));
            // }



            // if (!isArray && useDrawerType == null && inHorizontalLayout)
            // {
            //     useFallbackSaintsRow = true;
            //     useDrawerType = typeof(SaintsPropertyDrawer);
            // }

            // Debug.Log($"rendering {property.propertyPath}/useAttribute={useAttribute}/useDrawerType={useDrawerType}");

            // fallback to SaintsRow cuz: not custom drawer (attr drawer or type drawer), is generic, and is not array
            // if (useDrawerType != null && property.propertyType == SerializedPropertyType.Generic
            //                           && !property.isArray)
            // {
            //     SaintsRowAttributeDrawer saintsRowDrawer = (SaintsRowAttributeDrawer)SaintsPropertyDrawer.MakePropertyDrawer(typeof(SaintsRowAttributeDrawer), fieldInfo, useAttribute, label);
            //     saintsRowDrawer.InHorizontalLayout = inHorizontalLayout;
            //     return saintsRowDrawer.CreatePropertyGUI(property);
            // }

            if (useDrawerType == null)
            {
                VisualElement r = CreateOrUpdateFieldRawFallback(
                    property,
                    allAttributes,
                    rawType,
                    label,
                    fieldInfo,
                    inHorizontalLayout,
                    makeRenderer,
                    doTweenPlayRecorder,
                    originalField,
                    parent
                );
                // we don't need decorator here
                return r;
            }

            // Nah... This didn't handle for mis-ordered case
            // // Above situation will handle all including SaintsRow for general class/struct/interface.
            // // At this point we only need to let Unity handle it
            // PropertyField result = new PropertyField(FieldWithInfo.SerializedProperty)
            // {
            //     style =
            //     {
            //         flexGrow = 1,
            //     },
            //     name = FieldWithInfo.SerializedProperty.propertyPath,
            // };
            // result.Bind(FieldWithInfo.SerializedProperty.serializedObject);
            // return (result, false);
            // Debug.Log($"use {useDrawerType} for {property.propertyPath}");
            PropertyDrawer propertyDrawer = SaintsPropertyDrawer.MakePropertyDrawer(useDrawerType, fieldInfo, useAttribute, label);
            // Debug.Log(saintsPropertyDrawer);
            if (propertyDrawer is SaintsPropertyDrawer saintsPropertyDrawer)
            {
                // saintsPropertyDrawer.AppendSaintsAttributeDrawer = appendSaintsAttributeDrawer;
                saintsPropertyDrawer.InHorizontalLayout = inHorizontalLayout;
            }


            MethodInfo uiToolkitMethod = useDrawerType.GetMethod("CreatePropertyGUI");

            // bool isSaintsDrawer = useDrawerType.IsSubclassOf(typeof(SaintsPropertyDrawer)) || useDrawerType == typeof(SaintsPropertyDrawer);

            bool useImGui = uiToolkitMethod == null ||
                            uiToolkitMethod.DeclaringType == typeof(PropertyDrawer);  // null: old Unity || did not override

            // Debug.Log($"{useDrawerType}/{uiToolkitMethod.DeclaringType}/{FieldWithInfo.SerializedProperty.propertyPath}");

            if (!useImGui)
            {
                VisualElement r = propertyDrawer.CreatePropertyGUI(property);
                if (r != null)
                {
                    PropertyDrawerElementDirtyFix(property, propertyDrawer, r);
                    return UIToolkitCache.MergeWithDec(r, allAttributes);
                }
            }

            // SaintsPropertyDrawer won't have pure IMGUI one. Let Unity handle it.
            // We don't need to handle decorators either
            PropertyField result = new PropertyField(property)
            {
                style =
                {
                    flexGrow = 1,
                },
                name = property.propertyPath,
            };
            result.Bind(property.serializedObject);
            return result;

            // // this is the SaintsPropertyDrawer way, but some IMGUI has height issue with IMGUIContainer (e.g. Wwise.EventDrawer)
            // // so we just ignore anything and let unity handle it
            // SerializedProperty property = FieldWithInfo.SerializedProperty;
            // MethodInfo imGuiGetPropertyHeightMethod = useDrawerType.GetMethod("GetPropertyHeight");
            // MethodInfo imGuiOnGUIMethodInfo = useDrawerType.GetMethod("OnGUI");
            // Debug.Assert(imGuiGetPropertyHeightMethod != null);
            // Debug.Assert(imGuiOnGUIMethodInfo != null);
            //
            // IMGUILabelHelper imguiLabelHelper = new IMGUILabelHelper(property.displayName);
            //
            // IMGUIContainer imGuiContainer = new IMGUIContainer(() =>
            // {
            //     property.serializedObject.Update();
            //
            //     GUIContent label = imguiLabelHelper.NoLabel
            //         ? GUIContent.none
            //         : new GUIContent(imguiLabelHelper.RichLabel);
            //
            //     using (new ImGuiFoldoutStyleRichTextScoop())
            //     using (new ImGuiLabelStyleRichTextScoop())
            //     {
            //         float height =
            //             (float)imGuiGetPropertyHeightMethod.Invoke(propertyDrawer, new object[] { property, label });
            //         Rect rect = EditorGUILayout.GetControlRect(true, height, GUILayout.ExpandWidth(true));
            //         imGuiOnGUIMethodInfo.Invoke(propertyDrawer, new object[] { rect, property, label });
            //     }
            // })
            // {
            //     style =
            //     {
            //         flexGrow = 1,
            //         flexShrink = 0,
            //     },
            //     userData = imguiLabelHelper,
            // };
            // imGuiContainer.AddToClassList(IMGUILabelHelper.ClassName);
            //
            // return (imGuiContainer, false);
        }

        public static void PropertyDrawerElementDirtyFix(SerializedProperty property, PropertyDrawer propertyDrawer, VisualElement element)
        {
            // ReSharper disable once InvertIf
            if (propertyDrawer is UnityEventDrawer)  // I have zero idea why...
            {
                ListView lv = element.Q<ListView>();
                // ReSharper disable once InvertIf
                if(lv != null)
                {
                    SerializedProperty propertyRelative = property.FindPropertyRelative("m_PersistentCalls.m_Calls");
                    // lv.bindingPath = propertyRelative.propertyPath;
                    if(propertyRelative != null)
                    {
                        lv.BindProperty(propertyRelative);
                    }
                }
                // Debug.Log(lv.itemsSource);
                // return ele;
            }
        }

        // Basically the same idea from PropertyField
        // Note: do NOT pass SerializedPropertyType.Generic type: process it externally.
        public static VisualElement CreateOrUpdateFieldRawFallback(
          SerializedProperty property,
          IReadOnlyList<PropertyAttribute> allAttributes,
          Type rawType,
          string label,
          FieldInfo fieldInfo,
          bool inHorizontalLayout,
          IMakeRenderer makeRenderer,
          IDOTweenPlayRecorder doTweenPlayRecorder,
          VisualElement originalField,
          object parent)
        {
            SerializedPropertyType propertyType = property.propertyType;
            switch (propertyType)
            {
                case SerializedPropertyType.Generic:
                case SerializedPropertyType.ManagedReference:
                {
                    if (property.isArray)
                    {
                        ListView listView = originalField as ListView;
                        bool listViewNotExist = listView == null;
                        if (listViewNotExist)
                        {
                            // Debug.Log($"listView {property.propertyPath}");
                            listView = new ListView
                            {
                                showBorder = true,
                                selectionType = SelectionType.Multiple,
                                showAddRemoveFooter = true,
                                showBoundCollectionSize = true,
                                showFoldoutHeader = true,
                                virtualizationMethod = CollectionVirtualizationMethod.DynamicHeight,
                                showAlternatingRowBackgrounds = AlternatingRowBackground.None,
                                reorderable = true,
                                reorderMode = ListViewReorderMode.Animated,

                                makeItem = () => new VisualElement(),
                                bindItem = (element, index) =>
                                {
                                    SerializedProperty itemProp = property.GetArrayElementAtIndex(index);
                                    element.Clear();

                                    VisualElement result = CreateOrUpdateFieldProperty(
                                        itemProp,
                                        allAttributes,
                                        ReflectUtils.GetElementType(rawType),
                                        $"Element {index}",
                                        fieldInfo, inHorizontalLayout, makeRenderer, doTweenPlayRecorder, null, parent);
                                    // Debug.Log($"done rendering {index}/{itemProp.propertyPath}/{result == null}/{property.arraySize}");
                                    if (result != null)
                                    {
                                        element.Add(result);
                                    }
                                },
                                // onAdd = thisListView =>
                                // {
                                //     property.arraySize += 1;
                                //     property.serializedObject.ApplyModifiedProperties();
                                // },
                            };
                            Toggle listViewToggle = listView.Q<Toggle>();
                            if (listViewToggle != null && listViewToggle.style.marginLeft != -12)
                            {
                                listViewToggle.style.marginLeft = -12;
                            }
                            listView.AddToClassList(SaintsPropertyDrawer.ClassAllowDisable);

                            // This won't work as itemsSourceSizeChanged is an internal event
                            // listView.itemsSourceSizeChanged += () =>
                            // {
                            //     using (SerializedPropertyChangeEvent pooled = SerializedPropertyChangeEvent.GetPooled(property))
                            //     {
                            //         pooled.target = listView;
                            //         listView.SendEvent(pooled);
                            //     }
                            // };

                            int curSize = property.arraySize;
                            listView.schedule.Execute(() =>
                            {
                                int newSize;
                                try
                                {
                                    newSize = property.arraySize;
                                }
                                catch (ObjectDisposedException)
                                {
                                    return;
                                }
                                catch (NullReferenceException)
                                {
                                    return;
                                }

                                if (newSize == curSize)
                                {
                                    return;
                                }

                                curSize = newSize;
                                // ReSharper disable once ConvertToUsingDeclaration
                                using (SerializedPropertyChangeEvent pooled = SerializedPropertyChangeEvent.GetPooled(property))
                                {
                                    pooled.target = listView;
                                    listView.SendEvent(pooled);
                                }
                            }).Every(100);
                        }

                        SerializedProperty serializedProperty = property.Copy();
                        // string str = PropertyField.listViewNamePrefix + property.propertyPath;
                        string str = "saints-field--list-view--" + property.propertyPath;
                        listView.headerTitle = label;
                        listView.userData = serializedProperty;
                        listView.bindingPath = property.propertyPath;
                        listView.viewDataKey = str;
                        listView.name = str;

                        // this is internal too...
                        // listView.SetProperty((PropertyName) PropertyField.listViewBoundFieldProperty, (object) this);
                        // Toggle toggle = listView.Q<Toggle>((string) null, Foldout.toggleUssClassName);
                        // if (toggle != null)
                        //     toggle.m_Clickable.acceptClicksIfDisabled = true;

                        listView.BindProperty(property);

                        return listViewNotExist ? listView : null;

                    }
                    if (originalField != null &&
                        originalField.ClassListContains(SaintsRowAttributeDrawer.SaintsRowClass))
                    {
                        return null;
                    }
                    return SaintsRowAttributeDrawer.CreateElement(property, label, fieldInfo, inHorizontalLayout, null, makeRenderer, doTweenPlayRecorder, parent);
                }
                    // throw new ArgumentOutOfRangeException(nameof(propertyType), propertyType, "Should Not Put it here");
                case SerializedPropertyType.Integer:
                {
                    if (property.type == "long")
                    {
                        if (originalField is LongField longField)
                        {
                            longField.SetValueWithoutNotify(property.longValue);
                            return null;
                        }

                        longField = new LongField(label)
                        {
                            value = property.longValue,
                            style =
                            {
                                flexGrow = 1,
                                flexShrink = 1,
                            },
                        };
                        longField.AddToClassList(SaintsPropertyDrawer.ClassAllowDisable);
                        longField.BindProperty(property);
                        longField.AddToClassList(LongField.alignedFieldUssClassName);
                        return longField;
                    }

                    if (property.type == "ulong")
                    {
#if UNITY_2022_3_OR_NEWER
                        if (originalField is UnsignedLongField unsignedLongField)
                        {
                            unsignedLongField.SetValueWithoutNotify(property.ulongValue);
                            return null;
                        }

                        unsignedLongField = new UnsignedLongField(label)
                        {
                            value = property.ulongValue,
                            style =
                            {
                                flexGrow = 1,
                                flexShrink = 1,
                            },
                        };
                        unsignedLongField.AddToClassList(SaintsPropertyDrawer.ClassAllowDisable);
                        unsignedLongField.BindProperty(property);
                        unsignedLongField.AddToClassList(UnsignedLongField.alignedFieldUssClassName);
                        return unsignedLongField;
#else
                        if (originalField is LongField unsignedLongField)
                        {
                            unsignedLongField.SetValueWithoutNotify(property.longValue);
                            return null;
                        }

                        unsignedLongField = new LongField(label)
                        {
                            value = property.longValue,
                            style =
                            {
                                flexGrow = 1,
                                flexShrink = 1,
                            },
                        };
                        unsignedLongField.AddToClassList(SaintsPropertyDrawer.ClassAllowDisable);
                        unsignedLongField.BindProperty(property);
                        unsignedLongField.AddToClassList(LongField.alignedFieldUssClassName);
                        return unsignedLongField;
#endif
                    }

                    if (property.type == "int")
                    {
                        if (originalField is IntegerField integerField)
                        {
                            integerField.SetValueWithoutNotify(property.intValue);
                            return null;
                        }

                        integerField = new IntegerField(label)
                        {
                            value = property.intValue,
                            style =
                            {
                                flexGrow = 1,
                                flexShrink = 1,
                            },
                        };
                        integerField.AddToClassList(SaintsPropertyDrawer.ClassAllowDisable);
                        integerField.BindProperty(property);
                        integerField.AddToClassList(IntegerField.alignedFieldUssClassName);
                        return integerField;
                    }

                    if (property.type == "uint")
                    {
#if UNITY_2022_3_OR_NEWER
                        if (originalField is UnsignedIntegerField unsignedIntegerField)
                        {
                            unsignedIntegerField.SetValueWithoutNotify(property.uintValue);
                            return null;
                        }

                        unsignedIntegerField = new UnsignedIntegerField(label)
                        {
                            value = property.uintValue,
                            style =
                            {
                                flexGrow = 1,
                                flexShrink = 1,
                            },
                        };
                        unsignedIntegerField.BindProperty(property);
                        unsignedIntegerField.AddToClassList(UnsignedIntegerField.alignedFieldUssClassName);
                        unsignedIntegerField.AddToClassList(SaintsPropertyDrawer.ClassAllowDisable);
                        return unsignedIntegerField;
#else
                        if (originalField is IntegerField unsignedIntegerField)
                        {
                            unsignedIntegerField.SetValueWithoutNotify(property.intValue);
                            return null;
                        }

                        unsignedIntegerField = new IntegerField(label)
                        {
                            value = property.intValue,
                            style =
                            {
                                flexGrow = 1,
                                flexShrink = 1,
                            },
                        };
                        unsignedIntegerField.BindProperty(property);
                        unsignedIntegerField.AddToClassList(IntegerField.alignedFieldUssClassName);
                        unsignedIntegerField.AddToClassList(SaintsPropertyDrawer.ClassAllowDisable);
                        return unsignedIntegerField;
#endif
                    }

                    if (property.type == "sbyte")
                    {
                        if (originalField is IntegerField integerField)
                        {
                            integerField.SetValueWithoutNotify((sbyte)property.intValue);
                            return null;
                        }

                        IntegerField element = new IntegerField(label)
                        {
                            value = (sbyte)property.intValue,
                            style =
                            {
                                flexGrow = 1,
                                flexShrink = 1,
                            },
                        };
                        element.BindProperty(property);
                        element.AddToClassList(IntegerField.alignedFieldUssClassName);
                        element.AddToClassList(SaintsPropertyDrawer.ClassAllowDisable);

                        return element;
                    }
                    if (property.type == "byte")
                    {
                        if (originalField is IntegerField oldIntegerField)
                        {
                            oldIntegerField.SetValueWithoutNotify((byte)property.intValue);
                            return null;
                        }

                        IntegerField element = new IntegerField(label)
                        {
                            value = (byte)property.intValue,
                            style =
                            {
                                flexGrow = 1,
                                flexShrink = 1,
                            },
                        };
                        element.BindProperty(property);
                        element.AddToClassList(IntegerField.alignedFieldUssClassName);
                        element.AddToClassList(SaintsPropertyDrawer.ClassAllowDisable);
                        return element;
                    }

                    if (property.type == "short")
                    {
                        if (originalField is IntegerField oldIntegerField)
                        {
                            oldIntegerField.SetValueWithoutNotify((short)property.intValue);
                            return null;
                        }

                        IntegerField element = new IntegerField(label)
                        {
                            value = (short)property.intValue,
                            style =
                            {
                                flexGrow = 1,
                                flexShrink = 1,
                            },
                        };
                        element.BindProperty(property);
                        element.AddToClassList(IntegerField.alignedFieldUssClassName);
                        element.AddToClassList(SaintsPropertyDrawer.ClassAllowDisable);
                        return element;
                    }
                    if (property.type == "ushort")
                    {
                        if (originalField is IntegerField oldIntegerField)
                        {
                            oldIntegerField.SetValueWithoutNotify((ushort)property.intValue);
                            return null;
                        }

                        IntegerField element = new IntegerField(label)
                        {
                            value = (ushort)property.intValue,
                            style =
                            {
                                flexGrow = 1,
                                flexShrink = 1,
                            },
                        };
                        element.BindProperty(property);
                        element.AddToClassList(IntegerField.alignedFieldUssClassName);
                        element.AddToClassList(SaintsPropertyDrawer.ClassAllowDisable);
                        return element;
                    }
                    throw new ArgumentOutOfRangeException(nameof(property.type), property.type, null);
                }
                case SerializedPropertyType.Boolean:
                {
                    if(originalField is Toggle toggle)
                    {
                        toggle.SetValueWithoutNotify(property.boolValue);
                        return null;
                    }

                    toggle = new Toggle(label)
                    {
                        value = property.boolValue,
                        style =
                        {
                            flexGrow = 1,
                            flexShrink = 1,
                        },
                    };
                    toggle.BindProperty(property);
                    toggle.AddToClassList(Toggle.alignedFieldUssClassName);
                    toggle.AddToClassList(SaintsPropertyDrawer.ClassAllowDisable);
                    return toggle;
                }
                case SerializedPropertyType.Float:
                {
                    if (originalField is DoubleField doubleField)
                    {
                        doubleField.SetValueWithoutNotify(property.doubleValue);
                        return null;
                    }

                    doubleField = new DoubleField(label)
                    {
                        value = property.doubleValue,
                        style =
                        {
                            flexGrow = 1,
                            flexShrink = 1,
                        },
                    };
                    doubleField.BindProperty(property);
                    doubleField.AddToClassList(DoubleField.alignedFieldUssClassName);
                    doubleField.AddToClassList(SaintsPropertyDrawer.ClassAllowDisable);
                    return doubleField;
                }
                case SerializedPropertyType.String:
                {
                    if (originalField is TextField textField)
                    {
                      textField.SetValueWithoutNotify(property.stringValue);
                      return null;
                    }

                    textField = new TextField(label)
                    {
                        value = property.stringValue,
                        style =
                        {
                            flexGrow = 1,
                            flexShrink = 1,
                        },
                    };
                    textField.BindProperty(property);
                    textField.AddToClassList(TextField.alignedFieldUssClassName);
                    textField.AddToClassList(SaintsPropertyDrawer.ClassAllowDisable);
                    return textField;
                }
                case SerializedPropertyType.Color:
                {
                    if (originalField is ColorField colorField)
                    {
                        colorField.SetValueWithoutNotify(property.colorValue);
                        return null;
                    }

                    colorField = new ColorField(label)
                    {
                        value = property.colorValue,
                        style =
                        {
                            flexGrow = 1,
                            flexShrink = 1,
                        },
                    };
                    colorField.AddToClassList(ColorField.alignedFieldUssClassName);
                    colorField.BindProperty(property);
                    colorField.AddToClassList(SaintsPropertyDrawer.ClassAllowDisable);
                    return colorField;
                }
                case SerializedPropertyType.ObjectReference:
                {
                    if (originalField is ObjectField objectField)
                    {
                        objectField.SetValueWithoutNotify(property.objectReferenceValue);
                        // ReSharper disable once InvertIf
                        if (objectField.objectType != rawType)
                        {
                            objectField.objectType = rawType;
                            objectField.BindProperty(property);
                        }

                        return null;
                    }

                    objectField = new ObjectField(label)
                    {
                        objectType = rawType,
                        value = property.objectReferenceValue,
                        style =
                        {
                            flexGrow = 1,
                            flexShrink = 1,
                        },
                    };
                    objectField.BindProperty(property);
                    objectField.AddToClassList(ObjectField.alignedFieldUssClassName);
                    objectField.AddToClassList(SaintsPropertyDrawer.ClassAllowDisable);
                    return objectField;
                }
                case SerializedPropertyType.LayerMask:
                {
                    if(originalField is LayerMaskField layerMaskField)
                    {
                        layerMaskField.SetValueWithoutNotify(property.intValue);
                        return null;
                    }

                    layerMaskField = new LayerMaskField(label)
                    {
                        value = property.intValue,
                        style =
                        {
                            flexGrow = 1,
                            flexShrink = 1,
                        },
                    };
                    layerMaskField.BindProperty(property);
                    layerMaskField.AddToClassList(LayerMaskField.alignedFieldUssClassName);
                    layerMaskField.AddToClassList(SaintsPropertyDrawer.ClassAllowDisable);
                    return layerMaskField;
                }
                case SerializedPropertyType.Enum:
                {
                    bool hasFlags = rawType.GetCustomAttributes(typeof(FlagsAttribute), true).Length > 0;

                    Enum enumValue = (Enum)Enum.ToObject(rawType, property.intValue);

                    if (hasFlags)
                    {
                        if (originalField is EnumFlagsField enumFlagsField)
                        {
                            enumFlagsField.SetValueWithoutNotify(enumValue);
                            return null;
                        }

                        enumFlagsField = new EnumFlagsField(label, enumValue)
                        {
                            style =
                            {
                                flexGrow = 1,
                                flexShrink = 1,
                            },
                        };
                        enumFlagsField.BindProperty(property);
                        enumFlagsField.AddToClassList(EnumFlagsField.alignedFieldUssClassName);
                        enumFlagsField.AddToClassList(SaintsPropertyDrawer.ClassAllowDisable);
                        return enumFlagsField;
                    }

                    List<object> enumRawValues = Enum.GetValues(rawType)
                        .Cast<object>()
                        .ToList();

                    List<string> enumDisplayNames = enumRawValues
                        .Select(each =>
                        {
                            (bool found, string richName) = ReflectUtils.GetRichLabelFromEnum(rawType, each);
                            return found ? richName : Enum.GetName(rawType, each);
                        })
                        .ToList();

                    // Debug.Log($"property.enumValueIndex={property.enumValueIndex}");
                    int propertyFieldIndex = property.enumValueIndex < 0 || property.enumValueIndex >= enumDisplayNames.Count
                        ? -1
                        : property.enumValueIndex;

                    if (originalField is PopupField<string> popupField)
                    {
                        popupField.index = propertyFieldIndex;
                        return null;
                    }
                    //
                    // Dictionary<object, string> enumObjectToFancyName = enumRawValues
                    //     .ToDictionary(each => each, each =>
                    //     {
                    //         (bool found, string richName) = ReflectUtils.GetRichLabelFromEnum(rawType, each);
                    //         return found ? richName : Enum.GetName(rawType, each);
                    //     });

                    popupField = new PopupField<string>(label)
                    {
                        style =
                        {
                            flexGrow = 1,
                            flexShrink = 1,
                        },
                        choices = enumDisplayNames,
                        index = propertyFieldIndex,
                    };

                    popupField.BindProperty(property);

                    // popupField.RegisterValueChangedCallback(e =>
                    // {
                    //     string newValue = e.newValue;
                    //     // Debug.Log(newValue);
                    //     // int index = enumFancyNames.IndexOf(newValue);
                    //     // if (index == -1)
                    //     // {
                    //     //     return;
                    //     // }
                    //     // Debug.Log(index);
                    //     if (newValue == null)
                    //     {
                    //         return;
                    //     }
                    //
                    //     property.intValue = (int)newValue;
                    //     property.serializedObject.ApplyModifiedProperties();
                    // });
                    popupField.AddToClassList(PopupField<string>.alignedFieldUssClassName);
                    popupField.AddToClassList(SaintsPropertyDrawer.ClassAllowDisable);

                    return popupField;
                }
                case SerializedPropertyType.Vector2:
                {
                    if (originalField is Vector2Field vector2Field)
                    {
                        vector2Field.SetValueWithoutNotify(property.vector2Value);
                        return null;
                    }

                    vector2Field = new Vector2Field(label)
                    {
                        value = property.vector2Value,
                        style =
                        {
                            flexGrow = 1,
                            flexShrink = 1,
                        },
                    };
                    vector2Field.BindProperty(property);
                    vector2Field.AddToClassList(Vector2Field.alignedFieldUssClassName);
                    vector2Field.AddToClassList(SaintsPropertyDrawer.ClassAllowDisable);
                    return vector2Field;
                }
                case SerializedPropertyType.Vector3:
                {
                    if(originalField is Vector3Field vector3Field)
                    {
                        vector3Field.SetValueWithoutNotify(property.vector3Value);
                        return null;
                    }

                    vector3Field = new Vector3Field(label)
                    {
                        value = property.vector3Value,
                        style =
                        {
                            flexGrow = 1,
                            flexShrink = 1,
                        },
                    };
                    vector3Field.BindProperty(property);
                    vector3Field.AddToClassList(Vector3Field.alignedFieldUssClassName);
                    vector3Field.AddToClassList(SaintsPropertyDrawer.ClassAllowDisable);
                    return vector3Field;
                }
                case SerializedPropertyType.Vector4:
                {
                    if (originalField is Vector4Field vector4Field)
                    {
                        vector4Field.SetValueWithoutNotify(property.vector4Value);
                        return null;
                    }

                    vector4Field = new Vector4Field(label)
                    {
                        value = property.vector4Value,
                        style =
                        {
                            flexGrow = 1,
                            flexShrink = 1,
                        },
                    };
                    vector4Field.BindProperty(property);
                    vector4Field.AddToClassList(Vector4Field.alignedFieldUssClassName);
                    vector4Field.AddToClassList(SaintsPropertyDrawer.ClassAllowDisable);
                    return vector4Field;
                }
                case SerializedPropertyType.Rect:
                {
                    if (originalField is RectField rectField)
                    {
                        rectField.SetValueWithoutNotify(property.rectValue);
                        return null;
                    }

                    rectField = new RectField(label)
                    {
                        value = property.rectValue,
                        style =
                        {
                            flexGrow = 1,
                            flexShrink = 1,
                        },
                    };
                    rectField.BindProperty(property);
                    rectField.AddToClassList(RectField.alignedFieldUssClassName);
                    rectField.AddToClassList(SaintsPropertyDrawer.ClassAllowDisable);
                    return rectField;
                }
                case SerializedPropertyType.ArraySize:
                {
                    if (originalField is IntegerField integerField)
                    {
                        integerField.SetValueWithoutNotify(property.intValue);
                        return null;
                    }

                    integerField = new IntegerField(label)
                    {
                        value = property.intValue,
                        isDelayed = true,
                        style =
                        {
                            flexGrow = 1,
                            flexShrink = 1,
                        },
                    };
                    integerField.BindProperty(property);
                    integerField.AddToClassList(IntegerField.alignedFieldUssClassName);
                    integerField.AddToClassList(SaintsPropertyDrawer.ClassAllowDisable);
                    return integerField;
                }
                case SerializedPropertyType.Character:
                {
                    if (originalField is TextField textField)
                    {
                        textField.SetValueWithoutNotify(property.stringValue);
                        return null;
                    }

                    textField = new TextField(label)
                    {
                        value = property.stringValue,
                        maxLength = 1,
                        style =
                        {
                            flexGrow = 1,
                            flexShrink = 1,
                        },
                    };
                    textField.BindProperty(property);
                    textField.AddToClassList(TextField.alignedFieldUssClassName);
                    textField.AddToClassList(SaintsPropertyDrawer.ClassAllowDisable);
                    return textField;
                }
                case SerializedPropertyType.AnimationCurve:
                {
                    if (originalField is CurveField curveField)
                    {
                        curveField.SetValueWithoutNotify(property.animationCurveValue);
                        return null;
                    }

                    curveField = new CurveField(label)
                    {
                        value = property.animationCurveValue,
                        style =
                        {
                            flexGrow = 1,
                            flexShrink = 1,
                        },
                    };
                    curveField.BindProperty(property);
                    curveField.AddToClassList(CurveField.alignedFieldUssClassName);
                    curveField.AddToClassList(SaintsPropertyDrawer.ClassAllowDisable);
                    return curveField;
                }
                case SerializedPropertyType.Bounds:
                {
                    if (originalField is BoundsField boundsField)
                    {
                        boundsField.SetValueWithoutNotify(property.boundsValue);
                        return null;
                    }

                    boundsField = new BoundsField(label)
                    {
                        value = property.boundsValue,
                        style =
                        {
                            flexGrow = 1,
                            flexShrink = 1,
                        },
                    };
                    boundsField.BindProperty(property);
                    boundsField.AddToClassList(BoundsField.alignedFieldUssClassName);
                    boundsField.AddToClassList(SaintsPropertyDrawer.ClassAllowDisable);
                    return boundsField;
                }
                case SerializedPropertyType.Gradient:
                {
                    // ReSharper disable once JoinDeclarationAndInitializer
                    Gradient gradient;
#if UNITY_2022_1_OR_NEWER
                    gradient = property.gradientValue;
#else
                    PropertyInfo propertyInfo = property.GetType().GetProperty("gradientValue", BindingFlags.NonPublic | BindingFlags.Instance);
                    if (propertyInfo == null)
                    {
                        throw new InvalidOperationException("Property 'gradientValue' not found.");
                    }
                    gradient = (Gradient)propertyInfo.GetValue(property);
#endif

                    if (originalField is GradientField gradientField)
                    {

                        gradientField.SetValueWithoutNotify(gradient);
                        return null;
                    }

                    gradientField = new GradientField(label)
                    {
                        value = gradient,
                        style =
                        {
                            flexGrow = 1,
                            flexShrink = 1,
                        },
                    };
                    gradientField.BindProperty(property);
                    gradientField.AddToClassList(GradientField.alignedFieldUssClassName);
                    gradientField.AddToClassList(SaintsPropertyDrawer.ClassAllowDisable);
                    return gradientField;
                }
                case SerializedPropertyType.Quaternion:
                case SerializedPropertyType.ExposedReference:
                    return null;
                case SerializedPropertyType.FixedBufferSize:
                {
                    if (originalField is IntegerField integerField)
                    {
                        integerField.SetValueWithoutNotify(property.intValue);
                        return null;
                    }

                    integerField = new IntegerField(label)
                    {
                        value = property.intValue,
                        style =
                        {
                            flexGrow = 1,
                            flexShrink = 1,
                        },
                    };
                    integerField.BindProperty(property);
                    integerField.AddToClassList(IntegerField.alignedFieldUssClassName);
                    integerField.AddToClassList(SaintsPropertyDrawer.ClassAllowDisable);
                    return integerField;
                }
                case SerializedPropertyType.Vector2Int:
                {
                    if (originalField is Vector2IntField vector2IntField)
                    {
                        vector2IntField.SetValueWithoutNotify(property.vector2IntValue);
                        return null;
                    }

                    vector2IntField = new Vector2IntField(label)
                    {
                        value = property.vector2IntValue,
                        style =
                        {
                            flexGrow = 1,
                            flexShrink = 1,
                        },
                    };
                    vector2IntField.BindProperty(property);
                    vector2IntField.AddToClassList(Vector2IntField.alignedFieldUssClassName);
                    vector2IntField.AddToClassList(SaintsPropertyDrawer.ClassAllowDisable);
                    return vector2IntField;
                }
                case SerializedPropertyType.Vector3Int:
                {
                    if (originalField is Vector3IntField vector3IntField)
                    {
                        vector3IntField.SetValueWithoutNotify(property.vector3IntValue);
                        return null;
                    }

                    vector3IntField = new Vector3IntField(label)
                    {
                        value = property.vector3IntValue,
                        style =
                        {
                            flexGrow = 1,
                            flexShrink = 1,
                        },
                    };
                    vector3IntField.BindProperty(property);
                    vector3IntField.AddToClassList(Vector3IntField.alignedFieldUssClassName);
                    vector3IntField.AddToClassList(SaintsPropertyDrawer.ClassAllowDisable);
                    return vector3IntField;
                }
                case SerializedPropertyType.RectInt:
                {
                    if (originalField is RectIntField rectIntField)
                    {
                        rectIntField.SetValueWithoutNotify(property.rectIntValue);
                        return null;
                    }

                    rectIntField = new RectIntField(label)
                    {
                        value = property.rectIntValue,
                        style =
                        {
                            flexGrow = 1,
                            flexShrink = 1,
                        },
                    };
                    rectIntField.BindProperty(property);
                    rectIntField.AddToClassList(RectIntField.alignedFieldUssClassName);
                    rectIntField.AddToClassList(SaintsPropertyDrawer.ClassAllowDisable);
                    return rectIntField;
                }
                case SerializedPropertyType.BoundsInt:
                {
                    if (originalField is BoundsIntField boundsIntField)
                    {
                        boundsIntField.SetValueWithoutNotify(property.boundsIntValue);
                        return null;
                    }

                    boundsIntField = new BoundsIntField(label)
                    {
                        value = property.boundsIntValue,
                        style =
                        {
                            flexGrow = 1,
                            flexShrink = 1,
                        },
                    };
                    boundsIntField.BindProperty(property);
                    boundsIntField.AddToClassList(BoundsIntField.alignedFieldUssClassName);
                    boundsIntField.AddToClassList(SaintsPropertyDrawer.ClassAllowDisable);
                    return boundsIntField;
                }
                case SerializedPropertyType.Hash128:
                {
                    if (originalField is Hash128Field hash128Field)
                    {
                        hash128Field.SetValueWithoutNotify(property.hash128Value);
                        return null;
                    }

                    hash128Field = new Hash128Field(label)
                    {
                        value = property.hash128Value,
                        style =
                        {
                            flexGrow = 1,
                            flexShrink = 1,
                        },
                    };
                    hash128Field.BindProperty(property);
                    hash128Field.AddToClassList(Hash128Field.alignedFieldUssClassName);
                    hash128Field.AddToClassList(SaintsPropertyDrawer.ClassAllowDisable);
                    return hash128Field;
                }
                default:
                    return null;
            }
        }
#endif

        public static void AddContextualMenuManipulator(VisualElement ele, SerializedProperty property, Action onValueChangedCallback)
        {
            ele.AddManipulator(new ContextualMenuManipulator(evt =>
            {
                evt.menu.AppendAction("Copy Property Path", _ => EditorGUIUtility.systemCopyBuffer = property.propertyPath);

                bool spearator = false;
                if (ClipboardHelper.CanCopySerializedProperty(property.propertyType))
                {
                    spearator = true;
                    evt.menu.AppendSeparator();
                    evt.menu.AppendAction("Copy", _ => ClipboardHelper.DoCopySerializedProperty(property));
                }

                (bool hasReflectionPaste, bool hasValuePaste) = ClipboardHelper.CanPasteSerializedProperty(property.propertyType);

                // ReSharper disable once InvertIf
                if (hasReflectionPaste)
                {
                    if (!spearator)
                    {
                        evt.menu.AppendSeparator();
                    }

                    evt.menu.AppendAction("Paste", _ =>
                    {
                        ClipboardHelper.DoPasteSerializedProperty(property);
                        property.serializedObject.ApplyModifiedProperties();
                        onValueChangedCallback.Invoke();
                    }, hasValuePaste? DropdownMenuAction.Status.Normal: DropdownMenuAction.Status.Disabled);
                }
            }));
        }


        public class DropdownButtonField : BaseField<string>
        {
            public readonly Button ButtonElement;
            public readonly Label ButtonLabelElement;
            // private readonly MethodInfo AlignLabel;

            public DropdownButtonField(string label, Button visualInput, Label buttonLabel) : base(label, visualInput)
            {
                ButtonElement = visualInput;
                ButtonLabelElement = buttonLabel;

                // AlignLabel = typeof(BaseField<string>).GetMethod("AlignLabel", BindingFlags.NonPublic | BindingFlags.Instance);
            }
        }
    }
#endif
}
