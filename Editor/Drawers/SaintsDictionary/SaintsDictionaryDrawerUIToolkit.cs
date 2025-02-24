#if UNITY_2022_2_OR_NEWER
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using SaintsField.Editor.Utils;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace SaintsField.Editor.Drawers.SaintsDictionary
{
    public partial class SaintsDictionaryDrawer
    {
        // public override VisualElement CreatePropertyGUI(SerializedProperty property)
        // {
        //     return new TextElement()
        //     {
        //         text = "Hi"
        //     };
        // }

        private static string NameFoldout(SerializedProperty property) =>
            $"{property.propertyPath}__SaintsDictionary_Foldout";
        private static string NameTotalCount(SerializedProperty property) => $"{property.propertyPath}__SaintsDictionary_TotalCount";

        private static string NameListView(SerializedProperty property) => $"{property.propertyPath}__SaintsDictionary_ListView";
        private static string NameAddButton(SerializedProperty property) => $"{property.propertyPath}__SaintsDictionary_AddButton";
        private static string NameRemoveButton(SerializedProperty property) => $"{property.propertyPath}__SaintsDictionary_RemoveButton";

        protected override bool UseCreateFieldUIToolKit => true;

        protected override VisualElement CreateFieldUIToolKit(SerializedProperty property, ISaintsAttribute saintsAttribute,
            IReadOnlyList<PropertyAttribute> allAttributes, VisualElement container, FieldInfo info, object parent)
        {
            VisualElement root = new VisualElement
            {
                style =
                {
                    position = Position.Relative,
                },
            };
            VisualElement foldout = new Foldout
            {
                text = property.displayName,
                value = property.isExpanded,
                name = NameFoldout(property),
            };

            VisualElement content = foldout.Q<VisualElement>(className: "unity-foldout__content");
            // Debug.Log(content);
            if (content != null)
            {
                content.style.marginLeft = 0;
            }

            root.Add(foldout);

            // (string propKeysName, string propValuesName) = GetKeysValuesPropName(info.FieldType);

            // SerializedProperty keysProp = property.FindPropertyRelative(propKeysName);
            // SerializedProperty valuesProp = property.FindPropertyRelative(propValuesName);

            IntegerField totalCount = new IntegerField
            {
                // value = keysProp.arraySize,
                label = "",
                isDelayed = true,
                style =
                {
                    minWidth = 50,
                    position = Position.Absolute,
                    top = 1,
                    right = 1,
                },
                name = NameTotalCount(property),
            };
            root.Add(totalCount);

            MultiColumnListView multiColumnListView = new MultiColumnListView
            {
                // showBoundCollectionSize = true,
                virtualizationMethod = CollectionVirtualizationMethod.DynamicHeight,
                // itemsSource = MakeSource(keysProp),
                reorderable = true,
                reorderMode = ListViewReorderMode.Animated,
                showBorder = true,

                name = NameListView(property),

                // this has some issue because we bind order with renderer. Sort is not possible
// #if UNITY_6000_0_OR_NEWER
//                 sortingMode = ColumnSortingMode.Default,
// #else
//                 sortingEnabled = true,
// #endif
            };

            foldout.Add(multiColumnListView);

            // footer: add/remove
            VisualElement footerButtons = new VisualElement
            {
                style =
                {
                    flexDirection = FlexDirection.Row,
                    justifyContent = Justify.FlexEnd,
                    marginRight = 5,
                },
            };

            footerButtons.Add(new Button
            {
                text = "+",
                name = NameAddButton(property),
            });
            footerButtons.Add(new Button
            {
                text = "-",
                name = NameRemoveButton(property),
            });

            foldout.Add(footerButtons);

            // root.Add(multiColumnListView);
            return root;
        }

        protected override void OnAwakeUIToolkit(SerializedProperty property, ISaintsAttribute saintsAttribute, int index,
            IReadOnlyList<PropertyAttribute> allAttributes, VisualElement container, Action<object> onValueChangedCallback, FieldInfo info, object parent)
        {
            Foldout foldout = container.Q<Foldout>(name: NameFoldout(property));
            foldout.RegisterValueChangedCallback(newValue => property.isExpanded = newValue.newValue);

            int arrayIndex = SerializedUtils.PropertyPathIndex(property.propertyPath);

            Type rawType = arrayIndex == -1 ? info.FieldType : info.FieldType.GetElementType();
            // Debug.Log(info.FieldType);
            (string propKeysName, string propValuesName) = GetKeysValuesPropName(rawType);

            Debug.Log(propKeysName);

            SerializedProperty keysProp = property.FindPropertyRelative(propKeysName) ?? SerializedUtils.FindPropertyByAutoPropertyName(property, propKeysName);
            SerializedProperty valuesProp = property.FindPropertyRelative(propValuesName) ?? SerializedUtils.FindPropertyByAutoPropertyName(property, propValuesName);

            FieldInfo keysField = rawType.GetField(propKeysName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance  | BindingFlags.FlattenHierarchy);
            FieldInfo valuesField = rawType.GetField(propValuesName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance  | BindingFlags.FlattenHierarchy);

            (string curFieldError, int _, object curFieldVaue) = Util.GetValue(property, info, parent);
            Debug.Log(curFieldVaue);

            IEnumerable<object> allKeyList = keysField.GetValue(curFieldVaue) as IEnumerable<object>;
            Debug.Log(allKeyList);

            // foreach (object key in allKeyList)
            // {
            //     Debug.Log(key);
            // }

            IntegerField integerField = container.Q<IntegerField>(name: NameTotalCount(property));
            integerField.SetValueWithoutNotify(keysProp.arraySize);
            integerField.TrackPropertyValue(keysProp, _ =>
            {
                integerField.SetValueWithoutNotify(keysProp.arraySize);
            });
            integerField.RegisterValueChangedCallback(evt =>
            {
                int newSize = Mathf.Max(evt.newValue, 0);
                if (newSize >= keysProp.arraySize)
                {
                    if (IncreaseArraySize(newSize, keysProp, valuesProp))
                    {
                        property.serializedObject.ApplyModifiedProperties();
                    }
                }
                else
                {
                    IReadOnlyList<int> deleteIndexes = Enumerable.Range(newSize, keysProp.arraySize - newSize)
                        .Reverse()
                        .ToList();
                    DecreaseArraySize(deleteIndexes, keysProp, valuesProp);
                    property.serializedObject.ApplyModifiedProperties();
                }
            });

            // List<int> itemIndexToPropertyIndex = Enumerable.Range(0, keysProp.arraySize).ToList();

            MultiColumnListView multiColumnListView = container.Q<MultiColumnListView>(name: NameListView(property));

            multiColumnListView.columns.Add(new Column
            {
                name = "Keys",
                // title = "Keys",
                stretchable = true,
                makeHeader = () =>
                {
                    VisualElement header = new VisualElement();
                    header.Add(new Label("Keys"));
                    header.Add(new TextField
                    {
                        style =
                        {
                            marginRight = 3,
                        },
                    });
                    return header;
                },
                makeCell = () =>
                {
                    PropertyField propField = new PropertyField
                    {
                        label = "",
                    };
                    propField.Bind(property.serializedObject);
                    return propField;
                },
                bindCell = (element, elementIndex) =>
                {
                    PropertyField propertyField = (PropertyField)element;
                    SerializedProperty elementProp = keysProp.GetArrayElementAtIndex(elementIndex);

                    propertyField.TrackPropertyValue(elementProp, _ =>
                    {
                        IEnumerable<object> allKeyList = keysField.GetValue(parent) as IEnumerable<object>;

                        // integerField.SetValueWithoutNotify(keysProp.arraySize);
                    });

                    propertyField.BindProperty(elementProp);
                },
            });

            multiColumnListView.columns.Add(new Column
            {
                name = "Values",
                // title = "Values",
                stretchable = true,
                makeHeader = () =>
                {
                    VisualElement header = new VisualElement();
                    header.Add(new Label("Values"));
                    header.Add(new TextField
                    {
                        style =
                        {
                            marginRight = 3,
                        },
                    });
                    return header;
                },
                makeCell = () =>
                {
                    PropertyField propField = new PropertyField
                    {
                        label = "",
                    };
                    propField.Bind(property.serializedObject);
                    return propField;
                },
                bindCell = (element, elementIndex) =>
                {
                    PropertyField propertyField = (PropertyField)element;
                    SerializedProperty elementProp = valuesProp.GetArrayElementAtIndex(elementIndex);
                    propertyField.BindProperty(elementProp);
                },
            });

            Button addButton = container.Q<Button>(name: NameAddButton(property));
            addButton.clicked += () =>
            {
                IncreaseArraySize(keysProp.arraySize + 1, keysProp, valuesProp);
                property.serializedObject.ApplyModifiedProperties();
                multiColumnListView.itemsSource = MakeSource(keysProp);
                multiColumnListView.Rebuild();
            };
            Button deleteButton = container.Q<Button>(name: NameRemoveButton(property));
            deleteButton.clicked += () =>
            {
                // Debug.Log("Clicked");
                List<int> selected = multiColumnListView.selectedIndices.OrderByDescending(each => each).ToList();
                if (selected.Count == 0)
                {
                    int curSize = keysProp.arraySize;
                    if (curSize == 0)
                    {
                        return;
                    }
                    selected.Add(curSize);
                }

                // Debug.Log($"delete key at {string.Join(",", selected)}");

                DecreaseArraySize(selected, keysProp, valuesProp);
                property.serializedObject.ApplyModifiedProperties();
                multiColumnListView.itemsSource = MakeSource(keysProp);
                multiColumnListView.Rebuild();
            };

            int curSize = keysProp.arraySize;
            multiColumnListView.TrackPropertyValue(keysProp, _ =>
            {
                if (curSize != keysProp.arraySize)
                {
                    curSize = keysProp.arraySize;
                    multiColumnListView.itemsSource = MakeSource(keysProp);
                    multiColumnListView.Rebuild();
                }
            });

            multiColumnListView.itemIndexChanged += (first, second) =>
            {
//                 int fromPropIndex = itemIndexToPropertyIndex[first];
//                 int toPropIndex = itemIndexToPropertyIndex[second];
// #if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_LIST_DRAWER_SETTINGS
//                 Debug.Log($"drag {fromPropIndex}({first}) -> {toPropIndex}({second})");
// #endif
//
//                 property.MoveArrayElement(fromPropIndex, toPropIndex);
//                 property.serializedObject.ApplyModifiedProperties();
            };

            multiColumnListView.itemsSource = MakeSource(keysProp);
            multiColumnListView.Rebuild();
        }

        private static List<int> MakeSource(SerializedProperty property)
        {
            return Enumerable.Range(0, property.arraySize).ToList();
        }
    }
}
#elif UNITY_2021_3_OR_NEWER
using System.Collections.Generic;
using System.Reflection;
using SaintsField.Editor.Utils;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace SaintsField.Editor.Drawers.SaintsDictionary
{
    public partial class SaintsDictionaryDrawer
    {
        protected override VisualElement CreateFieldUIToolKit(SerializedProperty property, ISaintsAttribute saintsAttribute,
            IReadOnlyList<PropertyAttribute> allAttributes,
            VisualElement container, FieldInfo info, object parent)
        {
            int propertyIndex = SerializedUtils.PropertyPathIndex(property.propertyPath);
            if (propertyIndex != 0)
            {
                return new VisualElement();
            }

            // Action<object> onValueChangedCallback = null;
            // onValueChangedCallback = value =>
            // {
            //     object newFetchParent = SerializedUtils.GetFieldInfoAndDirectParent(property).parent;
            //     if (newFetchParent == null)
            //     {
            //         Debug.LogWarning($"{property.propertyPath} parent disposed unexpectedly.");
            //         return;
            //     }
            //
            //     foreach (SaintsPropertyInfo saintsPropertyInfo in saintsPropertyDrawers)
            //     {
            //         saintsPropertyInfo.Drawer.OnValueChanged(
            //             property, saintsPropertyInfo.Attribute, saintsPropertyInfo.Index, containerElement,
            //             info, newFetchParent,
            //             onValueChangedCallback,
            //             value);
            //     }
            // };

            IMGUILabelHelper imguiLabelHelper = new IMGUILabelHelper(property.displayName);

            IMGUIContainer imGuiContainer = new IMGUIContainer(() =>
            {
                GUIContent label = imguiLabelHelper.NoLabel
                    ? GUIContent.none
                    : new GUIContent(imguiLabelHelper.RichLabel);

                property.serializedObject.Update();

                using(new ImGuiFoldoutStyleRichTextScoop())
                using(new ImGuiLabelStyleRichTextScoop())
                // using(EditorGUI.ChangeCheckScope changed = new EditorGUI.ChangeCheckScope())
                {
                    float height =
                        GetFieldHeight(property, label, saintsAttribute, info, true, parent);
                    Rect rect = EditorGUILayout.GetControlRect(true, height, GUILayout.ExpandWidth(true));

                    DrawField(rect, property, label, saintsAttribute, allAttributes, new OnGUIPayload(), info, parent);
                    // ReSharper disable once InvertIf
                    // if (changed.changed)
                    // {
                    //     object newFetchParent = SerializedUtils.GetFieldInfoAndDirectParent(property).parent;
                    //     if (newFetchParent == null)
                    //     {
                    //         Debug.LogWarning($"{property.propertyPath} parent disposed unexpectedly.");
                    //         return;
                    //     }
                    //
                    //     (string error, int _, object value) = Util.GetValue(property, info, newFetchParent);
                    //     if (error == "")
                    //     {
                    //         onValueChangedCallback(value);
                    //     }
                    // }
                }

                property.serializedObject.ApplyModifiedProperties();
            })
            {
                style =
                {
                    flexGrow = 1,
                    flexShrink = 0,
                },
                userData = imguiLabelHelper,
            };
            imGuiContainer.AddToClassList(IMGUILabelHelper.ClassName);

            return imGuiContainer;
        }
    }
}
#endif
