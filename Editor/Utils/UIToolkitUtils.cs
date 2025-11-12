using System.Collections;
using SaintsField.Editor.Drawers.EnumFlagsDrawers.FlagsDropdownDrawer;
using SaintsField.Editor.Drawers.LayerDrawer;
using SaintsField.Editor.Drawers.TreeDropdownDrawer;
using SaintsField.Editor.Playa.Renderer.BaseRenderer;
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
using SaintsField.Editor.Drawers.AdvancedDropdownDrawer;
using SaintsField.Editor.Drawers.EnumFlagsDrawers;
using SaintsField.Editor.Drawers.ReferencePicker;
using SaintsField.Editor.Playa;
using SaintsField.Utils;
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

            Label label = TryFindLabel(container);
            if (label == null)
            {
                container.schedule.Execute(() => ChangeLabel(container, chunksOrNull, richTextDrawer, delayTime + 0.3f));
                return;
            }

            SetLabel(label, chunksOrNull, richTextDrawer);
        }

        public static Label TryFindLabel(VisualElement container) => container.Q<Label>(className: "unity-label");

        public static void SetLabel(Label label, IEnumerable<RichTextDrawer.RichTextChunk> chunksOrNull,
            RichTextDrawer richTextDrawer)
        {
            if (label == null)
            {
                return;
            }

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

        public static void OnAttachToPanelOnce(VisualElement rootElement, EventCallback<AttachToPanelEvent> callback)
        {
#if UNITY_2023_2_OR_NEWER || UNITY_6000_0_OR_NEWER
// #if false
            rootElement.RegisterCallbackOnce(callback);
#else
            void WrapCallback(AttachToPanelEvent evt)
            {
                rootElement.UnregisterCallback<AttachToPanelEvent>(WrapCallback);
                callback(evt);
            }
            rootElement.RegisterCallback<AttachToPanelEvent>(WrapCallback);
#endif
        }

        private static VisualTreeAsset _dropdownButtonTree;

        public static TemplateContainer CloneDropdownButtonTree()
        {
            if (_dropdownButtonTree == null)
            {
                _dropdownButtonTree = Util.LoadResource<VisualTreeAsset>("UIToolkit/DropdownButton.uxml");
            }

            return _dropdownButtonTree.CloneTree();
        }

        public static DropdownButtonField MakeDropdownButtonUIToolkit(string label)
        {
            TemplateContainer dropdownElement = CloneDropdownButtonTree();
            Button button = dropdownElement.Q<Button>();
            // Button button = new Button
            // {
            //     style =
            //     {
            //         height = EditorGUIUtility.singleLineHeight,
            //         flexGrow = 1,
            //         flexShrink = 1,
            //
            //         paddingRight = 2,
            //         marginRight = 0,
            //         marginLeft = 0,
            //         alignItems = Align.FlexStart,
            //         unityTextAlign = TextAnchor.MiddleLeft,
            //     },
            //     // name = NameButtonField(property),
            //     // userData = metaInfo.SelectedIndex == -1
            //     //     ? null
            //     //     : metaInfo.DropdownListValue[metaInfo.SelectedIndex].Item2,
            // };

            button.style.flexGrow = 1;

            Label buttonLabel = button.Q<Label>();
            buttonLabel.style.overflow = Overflow.Hidden;
            buttonLabel.style.textOverflow = TextOverflow.Ellipsis;
            buttonLabel.style.marginRight = 0;
            buttonLabel.style.marginLeft = 4;

            // Label buttonLabel = new Label
            // {
            //     style =
            //     {
            //         flexGrow = 1,
            //         flexShrink = 1,
            //         // paddingRight = 20,
            //         // textOverflow = TextOverflow.Ellipsis,
            //         // unityOverflowClipBox = OverflowClipBox.PaddingBox,
            //         overflow = Overflow.Hidden,
            //         marginRight = 15,
            //         unityTextAlign = TextAnchor.MiddleLeft,
            //     },
            // };

            // button.Add(buttonLabel);

            DropdownButtonField dropdownButtonField = new DropdownButtonField(label, button, buttonLabel)
            {
                style =
                {
                    flexGrow = 1,
                    flexShrink = 1,
                },
            };

            // dropdownButtonField.AddToClassList("unity-base-field__aligned");
            dropdownButtonField.AddToClassList(DropdownButtonField.alignedFieldUssClassName);

            // dropdownButtonField.Add(new Image
            // {
            //     image = Util.LoadResource<Texture2D>("classic-dropdown.png"),
            //     scaleMode = ScaleMode.ScaleToFit,
            //     style =
            //     {
            //         maxWidth = 12,
            //         maxHeight = EditorGUIUtility.singleLineHeight,
            //         position = Position.Absolute,
            //         right = 4,
            //     },
            //     pickingMode = PickingMode.Ignore,
            // });

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
            bool mergeDec,
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
            // Debug.Log($"rendering {property.propertyPath}/{property.propertyType}/isArray={isArray}/hor={inHorizontalLayout}");
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
                            property, allAttributes);
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

                // Debug.Log($"{property.propertyPath}: drawer={useDrawerType}; label={label}");
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
                // Debug.Log($"fallback {property.propertyPath}/hor={inHorizontalLayout};prop={string.Join(",", allAttributes)}; label={label}");
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
                if (r == null)
                {
                    return null;
                }

                return mergeDec? UIToolkitCache.MergeWithDec(r, allAttributes): r;
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
            // Debug.Log($"use {useDrawerType} for {property.propertyPath}, label={label}");
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

            // Debug.Log($"{useDrawerType}/{uiToolkitMethod?.DeclaringType}/{property.propertyPath}");

            if (!useImGui)
            {
                // Debug.Log($"CreatePropertyGUI for {property.propertyPath} with {propertyDrawer}");
                VisualElement r = propertyDrawer.CreatePropertyGUI(property);
                if (r != null)
                {
                    r.Bind(property.serializedObject);
                    // PropertyDrawerElementDirtyFix(property, propertyDrawer, r);
                    return mergeDec? UIToolkitCache.MergeWithDec(r, allAttributes): r;
                }
            }

            // SaintsPropertyDrawer won't have pure IMGUI one. Let Unity handle it.
            // We don't need to handle decorators either
            PropertyField result = new PropertyField(property, string.IsNullOrEmpty(label) ? "": label)
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

        // This is now fixed using Bind(serializedObject)
        // public static void PropertyDrawerElementDirtyFix(SerializedProperty property, PropertyDrawer propertyDrawer, VisualElement element)
        // {
        //     // ReSharper disable once InvertIf
        //     if (propertyDrawer is UnityEventDrawer)  // I have zero idea why...
        //     {
        //         ListView lv = element.Q<ListView>();
        //         // ReSharper disable once InvertIf
        //         if(lv != null)
        //         {
        //             SerializedProperty propertyRelative = property.FindPropertyRelative("m_PersistentCalls.m_Calls");
        //             // lv.bindingPath = propertyRelative.propertyPath;
        //             if(propertyRelative != null)
        //             {
        //                 lv.BindProperty(propertyRelative);
        //                 return;
        //             }
        //         }
        //         // Debug.Log(lv.itemsSource);
        //         // return ele;
        //     }
        //
        //     // Debug.Log(element is BindableElement);
        //     if (element is IBindable bindableElement)
        //     {
        //         // https://github.com/TylerTemp/SaintsField/issues/286
        //         // Even TextAreaDrawer has `propertyGui.bindingPath = property.propertyPath;`
        //         // the binding process will still fail, and need to bind again
        //         // I have no idea why... UI Toolkit's editor function is documented very poorly
        //
        //         // Debug.Log(bindableElement.bindingPath);  // this have a value
        //         // Debug.Log(bindableElement.binding);
        //         bindableElement.BindProperty(property);
        //         // Debug.Log(bindableElement.bindingPath);
        //         // Debug.Log(bindableElement.binding);
        //     }
        // }

        private static StyleSheet _unityObjectFieldLabelDisplayNone;

        private static void ApplyBasicStyles(VisualElement field, bool inHorizontalLayout)
        {
            field.AddToClassList(SaintsPropertyDrawer.ClassAllowDisable);
            field.AddToClassList(SaintsPropertyDrawer.ClassLabelFieldUIToolkit);

            if (inHorizontalLayout)
            {
                field.style.flexDirection = FlexDirection.Column;
            }
            else
            {
                field.AddToClassList(BaseField<UnityEngine.Object>.alignedFieldUssClassName);
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
            // Debug.Log($"CreateOrUpdateFieldRawFallback process {property.propertyPath}/{property.propertyType}/{property.isArray}/hor={inHorizontalLayout}");
            switch (propertyType)
            {
                case SerializedPropertyType.Generic:
                case SerializedPropertyType.ManagedReference:
                {
                    // Debug.Log($"generic/managed process {property.propertyPath}/{property.isArray}");
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

                                    // Debug.Log($"draw item {itemProp.propertyPath}/rawType={rawType}/itemType={ReflectUtils.GetElementType(rawType)}");

                                    string defaultName = itemProp.displayName;

                                    VisualElement result = CreateOrUpdateFieldProperty(
                                        itemProp,
                                        allAttributes,
                                        ReflectUtils.GetElementType(rawType),
                                        itemProp.displayName,
                                        fieldInfo, inHorizontalLayout, makeRenderer, doTweenPlayRecorder, null, false, parent);
                                    // Debug.Log($"done rendering {index}/{itemProp.propertyPath}/{result == null}/{property.arraySize}");
                                    if (result != null)
                                    {
                                        element.Add(result);
                                        if(itemProp.propertyType == SerializedPropertyType.Generic || itemProp.propertyType == SerializedPropertyType.ManagedReference)
                                        {
                                            result.schedule.Execute(() =>
                                            {
                                                if (SerializedUtils.IsOk(itemProp) &&
                                                    itemProp.displayName != defaultName)
                                                {
                                                    defaultName = itemProp.displayName;
                                                    ChangeLabel(result,
                                                        RichTextDrawer.ParseRichXml(defaultName, label, property,
                                                            fieldInfo, parent), new RichTextDrawer(), 0);
                                                }
                                            }).Every(150);
                                        }
                                    }
                                },
                                unbindItem = (element, _) =>
                                {
                                    element.Clear();
                                    Unbind(element);
                                    // Debug.Log(element);
                                    // Debug.Log(i);
                                },
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

                        listView.AddToClassList(SaintsPropertyDrawer.ClassLabelFieldUIToolkit);

                        SerializedProperty serializedProperty = property.Copy();
                        // string str = PropertyField.listViewNamePrefix + property.propertyPath;
                        string str = "saints-field--list-view--" + property.propertyPath;
                        listView.headerTitle = string.IsNullOrEmpty(label)
                            ? property.displayName
                            : label;
                        listView.userData = serializedProperty;
                        listView.bindingPath = property.propertyPath;
                        listView.viewDataKey = str;
                        listView.name = str;

                        if (listView.itemsSource?.Count != property.arraySize)
                        {
                            listView.itemsSource = Enumerable.Range(0, property.arraySize)
                                .Select(property.GetArrayElementAtIndex).ToArray();
                        }

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

                    if (propertyType == SerializedPropertyType.ManagedReference && allAttributes.All(each => each is not ReferencePickerAttribute))
                    {
                        ReferencePickerAttribute referencePickerAttribute = new ReferencePickerAttribute();
                        ReferencePickerAttributeDrawer referencePickerAttributeDrawer = (ReferencePickerAttributeDrawer) SaintsPropertyDrawer.MakePropertyDrawer(typeof(ReferencePickerAttributeDrawer), fieldInfo, referencePickerAttribute, label);
                        referencePickerAttributeDrawer.OverridePropertyAttributes = new PropertyAttribute[]
                        {
                            referencePickerAttribute,
                            new SaintsRowAttribute(),
                        };
                        referencePickerAttributeDrawer.InHorizontalLayout = inHorizontalLayout;
                        return referencePickerAttributeDrawer.CreatePropertyGUI(property);
                    }

                    return SaintsRowAttributeDrawer.CreateElement(property, label, fieldInfo, inHorizontalLayout,
                        null, makeRenderer, doTweenPlayRecorder, parent);
                }
                    // throw new ArgumentOutOfRangeException(nameof(propertyType), propertyType, "Should Not Put it here");
                case SerializedPropertyType.Integer:
                {
                    #region long
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
                        longField.BindProperty(property);
                        ApplyBasicStyles(longField, inHorizontalLayout);
                        return longField;
                    }
                    #endregion

                    #region ulong
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
                        unsignedLongField.BindProperty(property);
                        ApplyBasicStyles(unsignedLongField, inHorizontalLayout);
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
                    #endregion

                    #region int
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
                        integerField.BindProperty(property);
                        ApplyBasicStyles(integerField, inHorizontalLayout);
                        return integerField;
                    }
                    #endregion

                    #region uint
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
                        ApplyBasicStyles(unsignedIntegerField, inHorizontalLayout);
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
                    #endregion

                    #region sbyte
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
                        ApplyBasicStyles(element, inHorizontalLayout);
                        return element;
                    }
                    #endregion

                    #region byte
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
                        ApplyBasicStyles(element, inHorizontalLayout);

                        return element;
                    }
                    #endregion

                    #region short
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
                        ApplyBasicStyles(element, inHorizontalLayout);
                        return element;
                    }
                    #endregion

                    #region ushort
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
                        ApplyBasicStyles(element, inHorizontalLayout);
                        return element;
                    }
                    #endregion

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
                    toggle.AddToClassList(SaintsPropertyDrawer.ClassLabelFieldUIToolkit);
                    // Debug.Log(inHorizontalLayout);
                    if (inHorizontalLayout)
                    {
                        // Debug.Log($"inHorizontalLayout{property.propertyPath}");
                        if(!string.IsNullOrEmpty(label))
                        {
                            toggle.style.flexDirection = FlexDirection.RowReverse;
                        }
                        Label toggleLabel = toggle.Q<Label>();
                        VisualElement toggleInput = toggle.Q<VisualElement>(className: Toggle.inputUssClassName);
                        if(toggleLabel != null)
                        {
                            // toggleLabel.style.minWidth = 0;
                            toggleLabel.style.flexGrow = 1;
                        }

                        // Debug.Log(toggleInput);
                        if (toggleInput != null)
                        {
                            toggleInput.style.flexGrow = 0;
                        }
                    }
                    else
                    {
                        toggle.AddToClassList(Toggle.alignedFieldUssClassName);
                    }
                    return toggle;
                }
                case SerializedPropertyType.Float:
                {
                    // Debug.Log(rawType);
                    if(rawType == typeof(double))
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

                        ApplyBasicStyles(doubleField, inHorizontalLayout);
                        return doubleField;
                    }

                    if (originalField is FloatField floatField)
                    {
                        floatField.SetValueWithoutNotify(property.floatValue);
                        return null;
                    }

                    floatField = new FloatField(label)
                    {
                        value = property.floatValue,
                        style =
                        {
                            flexGrow = 1,
                            flexShrink = 1,
                        },
                    };
                    floatField.BindProperty(property);
                    ApplyBasicStyles(floatField, inHorizontalLayout);

                    return floatField;
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
                    ApplyBasicStyles(textField, inHorizontalLayout);
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

                    colorField.BindProperty(property);
                    ApplyBasicStyles(colorField, inHorizontalLayout);
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

                    objectField = new ObjectField(property.displayName)
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
                    ApplyBasicStyles(objectField, inHorizontalLayout);

                    if (string.IsNullOrEmpty(label))  // ObjectField.label has issue in SaintsDictionary. This is a workaround
                    {
                        _unityObjectFieldLabelDisplayNone ??=
                            Util.LoadResource<StyleSheet>("UIToolkit/UnityObjectFieldLabelDisplayNone.uss");
                        objectField.styleSheets.Add(_unityObjectFieldLabelDisplayNone);
                    }

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
                    ApplyBasicStyles(layerMaskField, inHorizontalLayout);
                    return layerMaskField;
                }
                case SerializedPropertyType.Enum:
                {
                    bool hasFlags = rawType.GetCustomAttributes(typeof(FlagsAttribute), true).Length > 0;

                    // Enum enumValue = (Enum)Enum.ToObject(rawType, property.intValue);

                    if (hasFlags)
                    {
                        if (originalField != null)
                        {
                            return null;
                        }


                        FlagsDropdownAttribute flagsDropdownAttribute = new FlagsDropdownAttribute();
                        FlagsDropdownAttributeDrawer flagsDropdownDrawer = (FlagsDropdownAttributeDrawer) SaintsPropertyDrawer.MakePropertyDrawer(typeof(FlagsDropdownAttributeDrawer), fieldInfo, flagsDropdownAttribute, label);
                        flagsDropdownDrawer.OverridePropertyAttributes = new[] { flagsDropdownAttribute };
                        flagsDropdownDrawer.InHorizontalLayout = inHorizontalLayout;
                        return flagsDropdownDrawer.CreatePropertyGUI(property);
                    }

                    if (originalField != null)
                    {
                        return null;
                    }

                    AdvancedDropdownAttribute advancedDropdownAttribute = new AdvancedDropdownAttribute();
                    AdvancedDropdownAttributeDrawer advancedDropdownDrawer = (AdvancedDropdownAttributeDrawer) SaintsPropertyDrawer.MakePropertyDrawer(typeof(AdvancedDropdownAttributeDrawer), fieldInfo, advancedDropdownAttribute, label);
                    advancedDropdownDrawer.OverridePropertyAttributes = new[] { advancedDropdownAttribute };
                    advancedDropdownDrawer.InHorizontalLayout = inHorizontalLayout;
                    return advancedDropdownDrawer.CreatePropertyGUI(property);
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
                    vector2Field.AddToClassList(SaintsPropertyDrawer.ClassAllowDisable);
                    vector2Field.AddToClassList(SaintsPropertyDrawer.ClassLabelFieldUIToolkit);
                    if (inHorizontalLayout)
                    {
                        // element.style.flexDirection = FlexDirection.Column;
                        // element.style.flexWrap = Wrap.Wrap;
                        Label elementLabel = vector2Field.Q<Label>();
                        if (elementLabel != null)
                        {
                            elementLabel.style.minWidth = 0;
                            elementLabel.style.borderRightWidth = 1;
                            elementLabel.style.borderRightColor = EColor.Gray.GetColor();
                        }
                    }
                    else
                    {
                        vector2Field.AddToClassList(Vector2Field.alignedFieldUssClassName);
                    }
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
                    vector3Field.AddToClassList(SaintsPropertyDrawer.ClassAllowDisable);
                    vector3Field.AddToClassList(SaintsPropertyDrawer.ClassLabelFieldUIToolkit);

                    if (inHorizontalLayout)
                    {
                        // element.style.flexDirection = FlexDirection.Column;
                        // element.style.flexWrap = Wrap.Wrap;
                        Label elementLabel = vector3Field.Q<Label>();
                        if (elementLabel != null)
                        {
                            elementLabel.style.minWidth = 0;
                            elementLabel.style.borderRightWidth = 1;
                            elementLabel.style.borderRightColor = EColor.Gray.GetColor();
                        }
                    }
                    else
                    {
                        vector3Field.AddToClassList(Vector3Field.alignedFieldUssClassName);
                    }

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
                    vector4Field.AddToClassList(SaintsPropertyDrawer.ClassAllowDisable);
                    vector4Field.AddToClassList(SaintsPropertyDrawer.ClassLabelFieldUIToolkit);

                    if (inHorizontalLayout)
                    {
                        // element.style.flexDirection = FlexDirection.Column;
                        // element.style.flexWrap = Wrap.Wrap;
                        Label elementLabel = vector4Field.Q<Label>();
                        if (elementLabel != null)
                        {
                            elementLabel.style.minWidth = 0;
                            elementLabel.style.borderRightWidth = 1;
                            elementLabel.style.borderRightColor = EColor.Gray.GetColor();
                        }
                    }
                    else
                    {
                        vector4Field.AddToClassList(Vector4Field.alignedFieldUssClassName);
                    }
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
                    rectField.AddToClassList(SaintsPropertyDrawer.ClassAllowDisable);
                    rectField.AddToClassList(SaintsPropertyDrawer.ClassLabelFieldUIToolkit);

                    if (inHorizontalLayout)
                    {
                        // element.style.flexDirection = FlexDirection.Column;
                        // element.style.flexWrap = Wrap.Wrap;
                        Label elementLabel = rectField.Q<Label>();
                        if (elementLabel != null)
                        {
                            elementLabel.style.minWidth = 0;
                            elementLabel.style.borderRightWidth = 1;
                            elementLabel.style.borderRightColor = EColor.Gray.GetColor();
                        }
                    }
                    else
                    {
                        rectField.AddToClassList(RectField.alignedFieldUssClassName);
                    }
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
                    ApplyBasicStyles(integerField, inHorizontalLayout);
                    return integerField;
                }
                case SerializedPropertyType.Character:
                {
                    if (originalField is TextField textField)
                    {
                        textField.SetValueWithoutNotify(string.IsNullOrEmpty(property.stringValue)? '\0'.ToString(): property.stringValue[..1]);
                        return null;
                    }

                    textField = new TextField(label)
                    {
                        value = string.IsNullOrEmpty(property.stringValue)? '\0'.ToString(): property.stringValue[..1],
                        maxLength = 1,
                        style =
                        {
                            flexGrow = 1,
                            flexShrink = 1,
                        },
                    };
                    textField.BindProperty(property);
                    ApplyBasicStyles(textField, inHorizontalLayout);
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
                    ApplyBasicStyles(curveField, inHorizontalLayout);

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
                    boundsField.AddToClassList(SaintsPropertyDrawer.ClassAllowDisable);
                    boundsField.AddToClassList(SaintsPropertyDrawer.ClassLabelFieldUIToolkit);
                    if (inHorizontalLayout)
                    {
                        // element.style.flexDirection = FlexDirection.Column;
                        // element.style.flexWrap = Wrap.Wrap;
                        // Label elementLabel = element.Q<Label>();
                        // if (elementLabel != null)
                        // {
                        //     elementLabel.style.minWidth = 0;
                        //     elementLabel.style.borderRightWidth = 1;
                        //     elementLabel.style.borderRightColor = EColor.Gray.GetColor();
                        // }
                    }
                    else
                    {
                        boundsField.AddToClassList(BoundsField.alignedFieldUssClassName);
                    }
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
                    ApplyBasicStyles(gradientField, inHorizontalLayout);
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
                    ApplyBasicStyles(integerField, inHorizontalLayout);
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
                    vector2IntField.AddToClassList(SaintsPropertyDrawer.ClassAllowDisable);
                    vector2IntField.AddToClassList(SaintsPropertyDrawer.ClassLabelFieldUIToolkit);

                    if (inHorizontalLayout)
                    {
                        // element.style.flexDirection = FlexDirection.Column;
                        // element.style.flexWrap = Wrap.Wrap;
                        Label elementLabel = vector2IntField.Q<Label>();
                        if (elementLabel != null)
                        {
                            elementLabel.style.minWidth = 0;
                            elementLabel.style.borderRightWidth = 1;
                            elementLabel.style.borderRightColor = EColor.Gray.GetColor();
                        }
                    }
                    else
                    {
                        vector2IntField.AddToClassList(Vector2IntField.alignedFieldUssClassName);
                    }
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
                    vector3IntField.AddToClassList(SaintsPropertyDrawer.ClassAllowDisable);
                    vector3IntField.AddToClassList(SaintsPropertyDrawer.ClassLabelFieldUIToolkit);

                    if (inHorizontalLayout)
                    {
                        // element.style.flexDirection = FlexDirection.Column;
                        // element.style.flexWrap = Wrap.Wrap;
                        Label elementLabel = vector3IntField.Q<Label>();
                        if (elementLabel != null)
                        {
                            elementLabel.style.minWidth = 0;
                            elementLabel.style.borderRightWidth = 1;
                            elementLabel.style.borderRightColor = EColor.Gray.GetColor();
                        }
                    }
                    else
                    {
                        vector3IntField.AddToClassList(Vector3IntField.alignedFieldUssClassName);
                    }
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
                    rectIntField.AddToClassList(SaintsPropertyDrawer.ClassAllowDisable);
                    rectIntField.AddToClassList(SaintsPropertyDrawer.ClassLabelFieldUIToolkit);

                    if (inHorizontalLayout)
                    {
                        // element.style.flexDirection = FlexDirection.Column;
                        // element.style.flexWrap = Wrap.Wrap;
                        Label elementLabel = rectIntField.Q<Label>();
                        if (elementLabel != null)
                        {
                            elementLabel.style.minWidth = 0;
                            elementLabel.style.borderRightWidth = 1;
                            elementLabel.style.borderRightColor = EColor.Gray.GetColor();
                        }
                    }
                    else
                    {
                        rectIntField.AddToClassList(RectIntField.alignedFieldUssClassName);
                    }
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
                    boundsIntField.AddToClassList(SaintsPropertyDrawer.ClassAllowDisable);
                    boundsIntField.AddToClassList(SaintsPropertyDrawer.ClassLabelFieldUIToolkit);

                    if (inHorizontalLayout)
                    {
                        // element.style.flexDirection = FlexDirection.Column;
                        // element.style.flexWrap = Wrap.Wrap;
                        // Label elementLabel = element.Q<Label>();
                        // if (elementLabel != null)
                        // {
                        //     elementLabel.style.minWidth = 0;
                        //     elementLabel.style.borderRightWidth = 1;
                        //     elementLabel.style.borderRightColor = EColor.Gray.GetColor();
                        // }
                    }
                    else
                    {
                        boundsIntField.AddToClassList(BoundsIntField.alignedFieldUssClassName);
                    }
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
                    ApplyBasicStyles(hash128Field, inHorizontalLayout);
                    return hash128Field;
                }
                default:
                    return null;
            }
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

        // private static bool _fallbackUnbindReflectionFailed;
        // private static Type _serializedObjectBindingContextType;
        // private static MethodInfo _serializedObjectBindingContextFindMethod;

        /// <summary>
        /// Remove property track from the element (Unbind)
        /// Thanks to [@Zallist](https://github.com/Zallist) in [#239](https://github.com/TylerTemp/SaintsField/issues/239)
        /// </summary>
        // ReSharper disable once UnusedParameter.Global
        public static void Unbind(VisualElement element)
        {
#if UNITY_2021_3_OR_NEWER
            try
            {
                element.Unbind();
            }
            catch (NullReferenceException)
            {
                // ignore
            }
            catch (ObjectDisposedException)
            {
                // ignore
            }
#else
            // not working atm
            if (_fallbackUnbindReflectionFailed)
            {
#if SAINTSFIELD_DEBUG
                Debug.Log("Unbind skip: already failed");
#endif
                return;
            }

            // get internal binder type
            _serializedObjectBindingContextType ??= Type.GetType("UnityEditor.UIElements.Bindings.SerializedObjectBindingContext, UnityEditor.UIElementsModule", throwOnError: false);

            if (_serializedObjectBindingContextType == null)
            {
#if SAINTSFIELD_DEBUG
                Debug.LogWarning("Unbind skip: failed to find SerializedObjectBindingContext type");
#endif
                _fallbackUnbindReflectionFailed = true;
                return;
            }

            // get the find method
            _serializedObjectBindingContextFindMethod ??= _serializedObjectBindingContextType.GetMethod("FindBindingContext", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);

            if (_serializedObjectBindingContextFindMethod == null)
            {
#if SAINTSFIELD_DEBUG
                Debug.LogWarning("Unbind skip: failed to find FindBindingContext method in SerializedObjectBindingContext");
#endif
                _fallbackUnbindReflectionFailed = true;
                return;
            }

            // get curveField context (if possible)
            object elementContext = _serializedObjectBindingContextFindMethod.Invoke(null, new object[] { element, serializedObject });

            if (elementContext == null)
            {
#if SAINTSFIELD_DEBUG
                Debug.LogWarning($"Unbind skip: failed to find binding context for element {element}");
#endif
                return;
            }

            // get the binding updater (.Add method will always return it)
            MethodInfo bindingUpdaterAddMethod = elementContext.GetType().GetMethod("AddBindingUpdater", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.FlattenHierarchy);

            if (bindingUpdaterAddMethod == null)
            {
#if SAINTSFIELD_DEBUG
                Debug.LogWarning($"Unbind skip: failed to find AddBindingUpdater method in {elementContext.GetType()}");
#endif
                return;
            }

            object bindingUpdater = bindingUpdaterAddMethod.Invoke(elementContext, new object[] { element });

            if (bindingUpdater == null)
            {
#if SAINTSFIELD_DEBUG
                Debug.LogWarning($"Unbind skip: failed to get binding updater for element {element}");
#endif
                return;
            }
            // and call .Unbind() for a blanket removal
            MethodInfo unbindMethod = bindingUpdater.GetType().GetMethod("Unbind", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.FlattenHierarchy);

            if (unbindMethod == null)
            {
#if SAINTSFIELD_DEBUG
                Debug.LogWarning($"Unbind skip: failed to find Unbind method in {bindingUpdater.GetType()}");
#endif
                _fallbackUnbindReflectionFailed = true;
                return;
            }

            unbindMethod.Invoke(bindingUpdater, Array.Empty<object>());
            // Debug.Log("unbindMethod!");
#endif
        }

        #region ContextMenu

        private static bool _fillPropertyContextMenuDelegateLoaded;
        private delegate GenericMenu FillPropertyContextMenuDelegate(SerializedProperty property, SerializedProperty linkedProperty = null, GenericMenu menu = null, VisualElement element = null);
        private static FillPropertyContextMenuDelegate _fillPropertyContextMenu;

//         private static FillPropertyContextMenuDelegate PopulateInternalUnityFunctions()
//         {
//             if (_fillPropertyContextMenuDelegateLoaded)
//             {
//                 return _fillPropertyContextMenu;
//             }
//
//             _fillPropertyContextMenuDelegateLoaded = true;
//             try
//             {
//                 _fillPropertyContextMenu = typeof(EditorGUI)
//                     .GetMethod("FillPropertyContextMenu", BindingFlags.NonPublic | BindingFlags.Static)
//                     ?.CreateDelegate(typeof(FillPropertyContextMenuDelegate)) as FillPropertyContextMenuDelegate;
//             }
//             catch (Exception ex)
//             {
// #if SAINTSFIELD_DEBUG
//                 Debug.LogException(ex);
// #endif
//             }
//
//             return _fillPropertyContextMenu;
//         }
        // [UnityEditor.InitializeOnLoadMethod]
        private static FillPropertyContextMenuDelegate PopulateInternalUnityFunctions()
        {
            if (_fillPropertyContextMenuDelegateLoaded)
            {
                return _fillPropertyContextMenu;
            }

            _fillPropertyContextMenuDelegateLoaded = true;
            MethodInfo method = typeof(EditorGUI)
                .GetMethod("FillPropertyContextMenu", BindingFlags.NonPublic | BindingFlags.Static);
            if (method == null)
            {
                return null;
            }

            try
            {
                _fillPropertyContextMenu = method.CreateDelegate(typeof(FillPropertyContextMenuDelegate)) as FillPropertyContextMenuDelegate;
            }
            catch (Exception ex)
            {
#if SAINTSFIELD_DEBUG
                Debug.LogException(ex);
#endif
            }
            return _fillPropertyContextMenu;
        }

        // Recursively gathers all visible VisualElements under a mouse event
        private static List<VisualElement> GetElementsUnderPointer(IMouseEvent evt, VisualElement root)
        {
            List<VisualElement> elements = new List<VisualElement>();

            Traverse(evt, root, elements);
            return elements;
        }

        private static void Traverse(IMouseEvent evt, VisualElement element, List<VisualElement> elements)
        {
            if (!element.visible || element.resolvedStyle.display == DisplayStyle.None)
            {
                return;
            }

            if (element.worldBound.Contains(evt.mousePosition))
            {
                elements.Add(element);
            }

            foreach (VisualElement child in element.Children())
            {
                Traverse(evt, child, elements);
            }
        }

        // Regex for validating Unity property paths (e.g. "foo.bar[0].baz")
        // Allows nested fields and indexed arrays/lists
        private static readonly System.Text.RegularExpressions.Regex ValidPropertyName =
            new System.Text.RegularExpressions.Regex(@"
(                              # Capture the entire property path
  (?:                          # First segment, doesn't have a period or array stuff
    [a-zA-Z_]                  # First character must be a letter or underscore
    [a-zA-Z0-9_`<>$]*          # Followed by letters, digits, underscore, backtick, angle brackets, or dollar sign
  )
  (?:                          # Non-capturing group for nested parts
    \.                         # A literal dot for nested fields
    [a-zA-Z_]                  # Next field name must start with letter or underscore
    [a-zA-Z0-9_`<>$]*          # Followed by valid characters as above
    |                          # OR
    \[                         # Opening square bracket for array/list index
    (?:\d+)                    # One or more digits
    \]                         # Closing square bracket
  )*                           # Zero or more nested fields or indices
)",
                System.Text.RegularExpressions.RegexOptions.Compiled |
                System.Text.RegularExpressions.RegexOptions.IgnorePatternWhitespace);

        // Internal class for converting Unity's legacy GenericMenu to UI Toolkit's DropdownMenu
        private static class GenericMenuDynamicConverter
        {
            // Reflection targets inside GenericMenu and its nested MenuItem class
            private static readonly PropertyInfo ItemsProperty;
            private static readonly FieldInfo ContentField;
            private static readonly FieldInfo SeparatorField;
            private static readonly FieldInfo FuncField;
            private static readonly FieldInfo Func2Field;
            private static readonly FieldInfo UserDataField;
            private static readonly FieldInfo OnField;

            // Initializes field/property accessors via reflection
            static GenericMenuDynamicConverter()
            {
                Type menuItemType = typeof(GenericMenu).GetNestedType("MenuItem", BindingFlags.NonPublic);
                ItemsProperty = typeof(GenericMenu).GetProperty("menuItems", BindingFlags.Instance | BindingFlags.NonPublic);
                ContentField = menuItemType.GetField("content", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                SeparatorField = menuItemType.GetField("separator", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                FuncField = menuItemType.GetField("func", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                Func2Field = menuItemType.GetField("func2", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                UserDataField = menuItemType.GetField("userData", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                OnField = menuItemType.GetField("on", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            }

            // Caches status callback functions for dropdown items
            private static readonly Dictionary<DropdownMenuAction.Status, Func<DropdownMenuAction, DropdownMenuAction.Status>> StatusCallbacksCache =
                new Dictionary<DropdownMenuAction.Status, Func<DropdownMenuAction, DropdownMenuAction.Status>>
            {
                { DropdownMenuAction.Status.Normal, DropdownMenuAction.AlwaysEnabled },
                { DropdownMenuAction.Status.Disabled, DropdownMenuAction.AlwaysDisabled },
            };

            // Converts a GenericMenu to a DropdownMenu
            public static bool ConvertGenericMenuToDropdownMenu(GenericMenu menu, DropdownMenu dropdownMenu = null)
            {
                DropdownMenu resultDropdownMenu = dropdownMenu ?? new DropdownMenu();
                if (ItemsProperty.GetValue(menu) is not IList menuItems)
                {
#if SAINTSFIELD_DEBUG
                    Debug.LogError("menuItems is null");
#endif
                    return false;
                }

                foreach (object item in menuItems)
                {
                    GUIContent content = (GUIContent)ContentField.GetValue(item);
                    bool isSeparator = (bool)SeparatorField.GetValue(item);
                    GenericMenu.MenuFunction func = FuncField.GetValue(item) as GenericMenu.MenuFunction;
                    GenericMenu.MenuFunction2 func2 = Func2Field.GetValue(item) as GenericMenu.MenuFunction2;
                    object userData = UserDataField.GetValue(item);
                    bool isOn = (bool)OnField.GetValue(item);

                    if (isSeparator)
                    {
                        resultDropdownMenu.AppendSeparator(content.text);
                    }
                    else
                    {
                        Func<DropdownMenuAction, DropdownMenuAction.Status> statusCallback = MenuItemToActionStatusCallback(func, func2, isOn);

                        if (func != null)
                        {
                            resultDropdownMenu.AppendAction(content.text, _ => func(), statusCallback);
                        }
                        else if (func2 != null)
                        {
                            resultDropdownMenu.AppendAction(content.text, action => func2(action.userData),
                                statusCallback, userData);
                        }
                        else
                        {
                            resultDropdownMenu.AppendAction(content.text, null, statusCallback);
                        }
                    }
                }

                return true;
            }

            // Produces the appropriate dropdown menu status callback based on enabled/checked state
            private static Func<DropdownMenuAction, DropdownMenuAction.Status> MenuItemToActionStatusCallback(GenericMenu.MenuFunction func, GenericMenu.MenuFunction2 func2, bool isOn)
            {
                DropdownMenuAction.Status status = DropdownMenuAction.Status.None;

                if (func != null || func2 != null)
                    status |= DropdownMenuAction.Status.Normal;
                else
                    status |= DropdownMenuAction.Status.Disabled;

                if (isOn)
                    status |= DropdownMenuAction.Status.Checked;

                if (!StatusCallbacksCache.TryGetValue(status, out Func<DropdownMenuAction, DropdownMenuAction.Status> callback))
                {
                    StatusCallbacksCache[status] = callback = _ => status;
                }

                return callback;
            }
        }


        /// <summary>
        /// Thanks to [@Zallist](https://github.com/Zallist) in [#254](https://github.com/TylerTemp/SaintsField/issues/254)
        /// </summary>
        public static void AddContextualMenuManipulator(VisualElement ele, SerializedProperty property, Action onValueChangedCallback)
        {
            ele.AddManipulator(new ContextualMenuManipulator(evt =>
            {
                FillPropertyContextMenuDelegate fillPropertyContextMenu = PopulateInternalUnityFunctions();
                if(fillPropertyContextMenu != null)
                {
                    try
                    {
                        PopulateContextMenuUsingFillPropertyContextMenu(evt, ele, property);
                        return;
                    }
                    catch (Exception ex)
                    {
#if SAINTSFIELD_DEBUG
                        Debug.LogException(ex);
#endif
                    }
                }

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

        // Populate context menu using unity's own fill function
        private static void PopulateContextMenuUsingFillPropertyContextMenu(ContextualMenuPopulateEvent evt, VisualElement root, SerializedProperty property)
        {
            List<VisualElement> elements = GetElementsUnderPointer(evt, root);

            for (int revIndex = elements.Count - 1; revIndex >= 0; revIndex--)
            {
                VisualElement underElement = elements[revIndex];

                // Check for direct SerializedProperty reference in userData
                if (underElement.userData is SerializedProperty mySerializedProperty)
                {
                    // Debug.Log("Populate1");
                    Populate(mySerializedProperty, underElement, evt, root);
                    return;
                }

                // Traverse up the element tree to find SerializedProperty in userData
                if (underElement.FindAncestorUserData() is SerializedProperty ancestorSerializedProperty)
                {
                    // Debug.Log("Populate2");
                    Populate(ancestorSerializedProperty, underElement, evt, root);
                    return;
                }

                // Resolve property via binding path if available
                if (underElement is IBindable bindableElement && !string.IsNullOrEmpty(bindableElement.bindingPath))
                {
                    SerializedProperty relativeProperty = property.serializedObject.FindProperty(bindableElement.bindingPath);

                    if (relativeProperty != null)
                    {
                        // Debug.Log("Populate3");
                        Populate(relativeProperty, underElement, evt, root);
                        return;
                    }
                }

                // Guess property by parsing name for valid property path substrings
                if (!string.IsNullOrEmpty(underElement.name) && underElement.name.Contains(property.propertyPath))
                {
                    // ReSharper disable once ReplaceSubstringWithRangeIndexer
                    string elementName = underElement.name.Substring(underElement.name.IndexOf(property.propertyPath, StringComparison.Ordinal));

                    System.Text.RegularExpressions.Match propertyNameMatch = ValidPropertyName.Match(elementName);

                    if (propertyNameMatch.Success)
                    {
                        string propertyPath = propertyNameMatch.Groups[1].Value;
                        SerializedProperty relativeProperty = property.serializedObject.FindProperty(propertyPath);

                        if (relativeProperty != null)
                        {
                            // Debug.Log("Populate4");
                            Populate(relativeProperty, underElement, evt, root);
                            return;
                        }
                    }
                }
            }

            // Fallback: show menu for the original property
            // Debug.Log("Populate Fallback");
            Populate(property, root, evt, root);
        }

        private static void Populate(SerializedProperty property, VisualElement underElement, ContextualMenuPopulateEvent evt, VisualElement root)
        {
            // var guiEnabled = GUI.enabled;
            // Event currentEvent = Event.current;
            GenericMenu menu = null;

            FillPropertyContextMenuDelegate fillPropertyContextMenu = PopulateInternalUnityFunctions();
            if(fillPropertyContextMenu != null)
            {
                using (new EventCurrentScoop(evt.imguiEvent))
                using (new GUIEnabledScoop(underElement.enabledInHierarchy))
                {
                    try
                    {
                        // Temporarily set up GUI context to match the event
                        // Event.current = evt.imguiEvent;
                        // GUI.enabled = element.enabledInHierarchy;

                        menu = PopulateInternalUnityFunctions()?.Invoke(property, null, null, root);
                    }
                    catch (Exception e)
                    {
#if SAINTSFIELD_DEBUG
                        Debug.LogError(e);
#endif
                    }
                    // finally
                    // {
                    //     // GUI.enabled = guiEnabled;
                    //     // Event.current = currentEvent;
                    // }
                }
            }

            if(menu != null)
            {
                // DropdownMenu uiToolkitMenu = null;
                bool converted = false;
                try
                {
                    // Convert legacy menu to new UI Toolkit dropdown menu
                    converted = GenericMenuDynamicConverter.ConvertGenericMenuToDropdownMenu(menu, evt.menu);
                }
                catch (Exception ex)
                {
#if SAINTSFIELD_DEBUG
                    Debug.LogException(ex);
#endif
                    evt.StopImmediatePropagation();

                    // Fallback to native IMGUI context menu
                    menu.ShowAsContext();
                }

                // ReSharper disable once InvertIf
                if (!converted)
                {
                    evt.StopImmediatePropagation();
                    menu.ShowAsContext();
                }

                // Debug.Log(uiToolkitMenu);
                // evt.menu = uiToolkitMenu;
            }
        }



        #endregion

        public static void MakePlaceholderRight(VisualElement target, string label)
        {
            target?.Add(new Label(label)
            {
                pickingMode = PickingMode.Ignore,
                style =
                {
                    color = Color.gray,
                },
            });

            // VisualElement textElement = target.Q<VisualElement>(className: "unity-text-element");
            // if (textElement != null)
            // {
            //     textElement.style.
            // }
        }

        public static DropdownButtonField ReferenceDropdownButtonField(string label, SerializedProperty property, VisualElement worldBoundElement, Func<IEnumerable<Type>> getTypes)
        {
            DropdownButtonField dropdownBtn = MakeDropdownButtonUIToolkit(label);

            dropdownBtn.ButtonLabelElement.text = GetReferencePropertyLabel(property);

            dropdownBtn.ButtonElement.clicked += () =>
            {
                (Rect dropBound, float maxHeight) = SaintsAdvancedDropdownUIToolkit.GetProperPos(worldBoundElement.worldBound);
                // dropBound.height = SaintsPropertyDrawer.SingleLineHeight;

                object managedReferenceValue = property.managedReferenceValue;
                AdvancedDropdownList<Type> dropdownList = new AdvancedDropdownList<Type>
                {
                    {"[Null]", null},
                };
                Dictionary<string, List<Type>> nameSpaceToTypes = new Dictionary<string, List<Type>>();
                foreach (Type type in getTypes())
                {
                    string typeNamespace = type.Namespace;
                    if (string.IsNullOrEmpty(typeNamespace))
                    {
                        typeNamespace = "";
                    }
                    if (!nameSpaceToTypes.TryGetValue(typeNamespace, out List<Type> list))
                    {
                        nameSpaceToTypes[typeNamespace] = list = new List<Type>();
                    }
                    list.Add(type);
                    // string displayName = FormatTypeName(type);
                    // dropdownList.Add(new AdvancedDropdownList<Type>(displayName, type));
                }

                IOrderedEnumerable<string> nameSpaceToTypesSorted = nameSpaceToTypes.Keys.OrderBy(each => each);
                foreach (string @namespace in nameSpaceToTypesSorted)
                {
                    List<Type> types = nameSpaceToTypes[@namespace];
                    // types.Sort((a, b) => string.Compare(a.Name, b.Name, StringComparison.Ordinal));
                    AdvancedDropdownList<Type> namespaceTypes = new AdvancedDropdownList<Type>(@namespace == ""? "[No Namespace]": @namespace);
                    foreach (Type eachType in types)
                    {
                        namespaceTypes.Add(eachType.Name, eachType);
                    }
                    dropdownList.Add(namespaceTypes);
                }

                AdvancedDropdownMetaInfo metaInfo = new AdvancedDropdownMetaInfo
                {
                    Error = "",
                    CurDisplay = managedReferenceValue == null
                        ? "-"
                        : managedReferenceValue.GetType().Name,
                    CurValues = managedReferenceValue == null? Array.Empty<object>(): new []{managedReferenceValue.GetType()},
                    DropdownListValue = dropdownList,
                    SelectStacks = Array.Empty<AdvancedDropdownAttributeDrawer.SelectStack>(),
                };

                UnityEditor.PopupWindow.Show(dropBound, new SaintsTreeDropdownUIToolkit(
                    metaInfo,
                    worldBoundElement.worldBound.width,
                    maxHeight,
                    false,
                    (curItem, _) =>
                    {
                        object instance = curItem == null
                            ? null
                            : ReferencePickerAttributeDrawer.CopyObj(managedReferenceValue, Activator.CreateInstance((Type)curItem));

                        // Debug.Log($"set ref to {instance} for {property.propertyPath}");
                        property.managedReferenceValue = instance;
                        property.serializedObject.ApplyModifiedProperties();
                        return null;
                    }
                ));
            };

            return dropdownBtn;
        }

        public static string GetReferencePropertyLabel(SerializedProperty property)
        {
            if (!SerializedUtils.IsOk(property))
            {
                return "";
            }

            object v = property.managedReferenceValue;
            // ReSharper disable once ConvertIfStatementToReturnStatement
            if (v == null)
            {
                return "-";
            }

            Type type = v.GetType();
            return $"{type.Name} <color=#{ColorUtility.ToHtmlStringRGB(Color.gray)}>{type.Namespace}</color>";
        }

        public static void UIToolkitValueEditAfterProcess<T>(
            BaseField<T> element,
            Action<object> setterOrNull,
            bool labelGrayColor,
            bool inHorizontalLayout)
        {
            if (labelGrayColor)
            {
                element.labelElement.style.color = AbsRenderer.ReColor;
            }
            if (inHorizontalLayout)
            {
                element.style.flexDirection = FlexDirection.Column;
            }
            else
            {
                element.AddToClassList(BaseField<T>.alignedFieldUssClassName);
            }
            if (setterOrNull == null)
            {
                element.SetEnabled(false);
                element.AddToClassList(AbsRenderer.ClassSaintsFieldEditingDisabled);
            }
            else
            {
                element.AddToClassList(SaintsPropertyDrawer.ClassAllowDisable);
                // element.RegisterValueChangedCallback(evt =>
                // {
                //     object newValue = transformer(evt.newValue);
                //     beforeSet?.Invoke(newValue);
                //     setterOrNull.Invoke(newValue);
                // });
            }
        }

        public static void CheckOutOfScoopFoldout(VisualElement root, HashSet<Toggle> processedToggles)
        {
            // foreach (VisualElement actualFieldContainer in root.Query<VisualElement>(className: AbsRenderer.ClassSaintsFieldPlayaContainer).ToList())
            {
                // Debug.Log(actualFieldContainer);
                List<Foldout> foldouts = root.Query<Foldout>().ToList();
                // Debug.Log($"foldouts {foldouts.Count}");
                if (foldouts.Count == 0)
                {
                    return;
                }

                // Debug.Log(root.worldBound.x);

                foreach (Foldout foldout in foldouts)
                {
                    // this class name is not consistent in different UI Toolkit versions. So just remove it...
                    // Toggle toggle = actualFieldContainer.Q<Toggle>(className: "unity-foldout__toggle--inspector");
                    Toggle toggle = foldout.Q<Toggle>();
                    if (toggle == null)
                    {
                        continue;
                    }

                    if (!processedToggles.Add(toggle))  // already processed
                    {
                        continue;
                    }

                    if(toggle.style.marginLeft != 0)
                    {
                        if (toggle.userData is VisualElement moverTarget)
                        {
                            // Debug.Log($"moving {toggle} for {moverTarget}");
                            if (moverTarget.style.paddingLeft != 12)
                            {
                                moverTarget.style.paddingLeft = 12;
                            }
                        }
                        else
                        {
                            // Debug.Log($"moving {toggle} marginLeft");
                            toggle.style.marginLeft = 0;
                        }
                    }

                    // Yeah... I no longer need this...
                    // if (double.IsNaN(toggle.resolvedStyle.width))
                    // {
                    //     continue;
                    // }
                    //
                    //
                    // float distance = toggle.worldBound.x - root.worldBound.x;
                    // if(distance < 0)
                    // {
                    //     // Debug.Log($"process {toggle.worldBound.x} - {root.worldBound.x}: {distance}");
                    //     float marginLeft = -distance + 4;
                    //     // VisualElement saintsParent = UIToolkitUtils.FindParentClass(foldout, SaintsPropertyDrawer.ClassLabelFieldUIToolkit)
                    //     //     .FirstOrDefault();
                    //     // if(saintsParent == null)
                    //     // {
                    //     //     Debug.Log(foldout);
                    //     //     foldout.style.marginLeft = marginLeft;
                    //     // }
                    //     // else
                    //     // {
                    //     //     float ml = saintsParent.resolvedStyle.marginLeft;
                    //     //     float useValue = double.IsNaN(ml) ? marginLeft : marginLeft + ml;
                    //     //     saintsParent.style.marginLeft = useValue;
                    //     // }
                    //     VisualElement propertyParent = UIToolkitUtils.IterUpWithSelf(foldout).Skip(1).FirstOrDefault(each => each is PropertyField);
                    //     if (propertyParent != null)
                    //     {
                    //         propertyParent.style.marginLeft = marginLeft;
                    //     }
                    // }
                }
            }
        }

        public static void LoopCheckOutOfScoopFoldout(VisualElement root)
        {
            HashSet<Toggle> processedToggles = new HashSet<Toggle>();

            CheckOutOfScoopFoldout(root, processedToggles);

            root.schedule
                .Execute(() => CheckOutOfScoopFoldout(root, processedToggles))
                .Every(150);
        }

        public static void SetHelpBox(HelpBox helpBox, string content)
        {
            if (helpBox == null)
            {
                return;
            }

            if (string.IsNullOrEmpty(content))
            {
                if (helpBox.style.display != DisplayStyle.None)
                {
                    helpBox.style.display = DisplayStyle.None;
                }

                return;
            }

            if (helpBox.text != content)
            {
                helpBox.text = content;
            }
            if (helpBox.style.display != DisplayStyle.Flex)
            {
                helpBox.style.display = DisplayStyle.Flex;
            }

        }
    }
#endif
}
