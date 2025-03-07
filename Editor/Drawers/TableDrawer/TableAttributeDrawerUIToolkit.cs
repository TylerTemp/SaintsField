#if UNITY_2022_2_OR_NEWER && !SAINTSFIELD_DEBUG_UNITY_BROKEN_FALLBACK
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using SaintsField.Editor.Drawers.ArraySizeDrawer;
using SaintsField.Editor.Linq;
using SaintsField.Editor.Utils;
using SaintsField.Interfaces;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;


namespace SaintsField.Editor.Drawers.TableDrawer
{
    public partial class TableAttributeDrawer
    {
        private static string NameArraySize(SerializedProperty property) => $"{property.propertyPath}__Table_ArraySize";
        private static string NameAddButton(SerializedProperty property) => $"{property.propertyPath}__Table_AddButton";
        private static string NameRemoveButton(SerializedProperty property) => $"{property.propertyPath}__Table_RemoveButton";

        private int _preArraySize;

        protected override VisualElement CreateFieldUIToolKit(SerializedProperty property,
            ISaintsAttribute saintsAttribute,
            IReadOnlyList<PropertyAttribute> allAttributes,
            VisualElement container, FieldInfo info, object parent)
        {
            int propertyIndex = SerializedUtils.PropertyPathIndex(property.propertyPath);
            if (propertyIndex != 0)
            {
                return new VisualElement();
            }
            (string error, SerializedProperty arrayProp) = SerializedUtils.GetArrayProperty(property);

            if (error != "")
            {
                return new HelpBox(error, HelpBoxMessageType.Error);
            }

            TableAttribute tableAttribute = (TableAttribute) saintsAttribute;

            VisualElement root = new VisualElement();

            bool itemIsObject = property.propertyType == SerializedPropertyType.ObjectReference;
            if(itemIsObject)
            {
                Object obj0 = MakeSource(arrayProp).Select(each => each.objectReferenceValue)
                    .FirstOrDefault(each => each != null);
                if (obj0 == null)
                {
                    // PropertyField nullProp = new PropertyField(child0);
                    // nullProp.Bind(child0.serializedObject);
                    // return nullProp;
                    ObjectField arrayItemProp = new ObjectField("")
                    {
                        objectType = ReflectUtils.GetElementType(info.FieldType),
                    };

                    root.Add(arrayItemProp);

                    arrayItemProp.RegisterValueChangedCallback(evt =>
                    {
                        root.Clear();
                        property.objectReferenceValue = evt.newValue;
                        property.serializedObject.ApplyModifiedProperties();
                        root.Add(BuildContent(arrayProp, root, tableAttribute, property, info, parent));
                    });
                    return root;
                }
            }

            BuildContent(arrayProp, root, tableAttribute, property, info, parent);
            return root;
        }

        private VisualElement BuildContent(SerializedProperty arrayProp, VisualElement root, TableAttribute tableAttribute, SerializedProperty property, FieldInfo info, object parent)
        {
            bool itemIsObject = property.propertyType == SerializedPropertyType.ObjectReference;

            int propertyIndex = SerializedUtils.PropertyPathIndex(property.propertyPath);
            if (propertyIndex != 0)
            {
                return new VisualElement();
            }
            // (string error, SerializedProperty arrayProp) = SerializedUtils.GetArrayProperty(property);

            _preArraySize = arrayProp.arraySize;

            // if (error != "")
            // {
            //     return new HelpBox(error, HelpBoxMessageType.Error);
            // }

            // controls
            VisualElement controls = new VisualElement
            {
                style =
                {
                    flexDirection = FlexDirection.Row,
                    justifyContent = Justify.FlexEnd,
                    // marginBottom = 4,
                },
            };

            MultiColumnListView multiColumnListView = new MultiColumnListView
            {
                showBoundCollectionSize = true,
                virtualizationMethod = CollectionVirtualizationMethod.DynamicHeight,
                itemsSource = MakeSource(arrayProp),
                reorderable = true,
                reorderMode = ListViewReorderMode.Animated,
                showBorder = true,

                // this has some issue because we bind order with renderer. Sort is not possible
// #if UNITY_6000_0_OR_NEWER
//                 sortingMode = ColumnSortingMode.Default,
// #else
//                 sortingEnabled = true,
// #endif
            };

            IntegerField arraySizeField = new IntegerField
            {
                value = arrayProp.arraySize,
                isDelayed = true,
                style =
                {
                    minWidth = 50,
                },
                name = NameArraySize(property),
            };
            arraySizeField.RegisterValueChangedCallback(evt =>
            {
                int newValue = evt.newValue;
                int oldValue = arrayProp.arraySize;
                int changedValue = ChangeArraySize(newValue, arrayProp);
                if (changedValue == oldValue)
                {
                    return;
                }

                _preArraySize = newValue;
                multiColumnListView.itemsSource = MakeSource(arrayProp);
                multiColumnListView.Rebuild();
            });
            controls.Add(arraySizeField);

            Toolbar toolbar = new Toolbar();
            ToolbarButton addButton = new ToolbarButton(() =>
            {
                int oldValue = arrayProp.arraySize;
                ChangeArraySize(oldValue + 1, arrayProp);
            })
            {
                text = "+",
                name = NameAddButton(property),
            };
            if (tableAttribute.HideAddButton)
            {
                addButton.style.display = DisplayStyle.None;
            }
            toolbar.Add(addButton);
            ToolbarButton removeButton = new ToolbarButton(() =>
            {
                DeleteArrayElement(arrayProp, multiColumnListView.selectedIndices);
            })
            {
                text = "-",
                name = NameRemoveButton(property),
            };
            if (tableAttribute.HideRemoveButton)
            {
                removeButton.style.display = DisplayStyle.None;
            }
            toolbar.Add(removeButton);

            if (tableAttribute.HideAddButton && tableAttribute.HideRemoveButton)
            {
                arraySizeField.SetEnabled(false);
            }

            controls.Add(toolbar);

            root.Add(controls);

            if (itemIsObject)
            {
                Object obj0 = multiColumnListView.itemsSource.Cast<SerializedProperty>().Select(each => each.objectReferenceValue).FirstOrDefault(each => each != null);
                SerializedObject serializedObject = new SerializedObject(obj0);

                Dictionary<string, List<SerializedPropertyInfo>> columnToProperties = new Dictionary<string, List<SerializedPropertyInfo>>();

                foreach (SerializedProperty serializedProperty in SerializedUtils.GetAllField(serializedObject))
                {
                    (TableColumnAttribute[] tableColumnAttributes, object _) = SerializedUtils.GetAttributesAndDirectParent<TableColumnAttribute>(serializedProperty);
                    string columnName = tableColumnAttributes.Length > 0? tableColumnAttributes[0].Title: serializedProperty.displayName;
                    if(!columnToProperties.TryGetValue(columnName, out List<SerializedPropertyInfo> list))
                    {
                        columnToProperties[columnName] = list = new List<SerializedPropertyInfo>();
                    }
                    list.Add(new SerializedPropertyInfo
                    {
                        Name = serializedProperty.name,
                        PropertyPath = serializedProperty.propertyPath,
                    });
                }

                // ReSharper disable once UseDeconstruction
                foreach (KeyValuePair<string, List<SerializedPropertyInfo>> columnKv in columnToProperties)
                {
                    string columnName = columnKv.Key;
                    List<SerializedPropertyInfo> properties = columnKv.Value;
                    IReadOnlyList<string> propNames = properties.Select(each => each.Name).ToArray();

                    string id = string.Join(";", properties.Select(each => each.PropertyPath));

                    multiColumnListView.columns.Add(new Column
                    {
                        name = id,
                        title = columnName,
                        stretchable = true,
                        // comparison = (a, b) => CompareProp((SerializedProperty) multiColumnListView.itemsSource[a], (SerializedProperty) multiColumnListView.itemsSource[(int)b]),
                    });

                    multiColumnListView.columns[id].makeCell = () =>
                    {
                        VisualElement itemContainer = new VisualElement();
                        // PropertyField propField = new PropertyField();
                        // itemContainer.Add(propField);
                        foreach (int _ in Enumerable.Range(0, properties.Count))
                        {
                            itemContainer.Add(new PropertyField());
                        }
                        return itemContainer;
                    };

                    multiColumnListView.columns[id].bindCell = (element, index) =>
                    {

                        SerializedProperty sp = (SerializedProperty)multiColumnListView.itemsSource[index];
                        Object obj = sp.objectReferenceValue;
                        if (obj == null)
                        {
                            ObjectField arrayItemProp = new ObjectField("")
                            {
                                objectType = ReflectUtils.GetElementType(info.FieldType),
                            };

                            element.Clear();
                            element.Add(arrayItemProp);

                            arrayItemProp.RegisterValueChangedCallback(evt =>
                            {
                                sp.objectReferenceValue = evt.newValue;
                                sp.serializedObject.ApplyModifiedProperties();
                                multiColumnListView.Rebuild();
                            });
                            return;
                        }
                        SerializedObject itemObject = new SerializedObject(obj);

                        foreach ((PropertyField propField, int propIndex) in element.Query<PropertyField>().ToList().WithIndex())
                        {
                            if (propIndex >= propNames.Count)
                            {
                                break;
                            }

                            string propName = propNames[propIndex];
                            SerializedProperty itemProp = itemObject.FindProperty(propName) ?? SerializedUtils.FindPropertyByAutoPropertyName(itemObject, propName);
                            propField.BindProperty(itemProp);
                            propField.Bind(itemObject);
                            propField.label = "";
                            Label firstLabel = propField.Query<Label>().First();
                            if(firstLabel != null)
                            {
                                if (!firstLabel.ClassListContains(ClassNoRichLabelUpdate))
                                {
                                    firstLabel.AddToClassList(ClassNoRichLabelUpdate);
                                }
                                if(itemProp.propertyType != SerializedPropertyType.Generic && firstLabel.style.display != DisplayStyle.None)
                                {
                                    firstLabel.style.display = DisplayStyle.None;
                                }
                            }
                        }
                    };
                }
            }
            else
            {
                SerializedProperty firstProp = arrayProp.GetArrayElementAtIndex(0);
                // Debug.Log($"rendering generic {firstProp.propertyPath}");
                Dictionary<string, List<SerializedPropertyInfo>> columnToProperties = new Dictionary<string, List<SerializedPropertyInfo>>();

                foreach (SerializedProperty serializedProperty in SerializedUtils.GetPropertyChildren(firstProp)
                             .Where(each => each != null))
                {
                    (TableColumnAttribute[] tableColumnAttributes, object _) = SerializedUtils.GetAttributesAndDirectParent<TableColumnAttribute>(serializedProperty);
                    string columnName = tableColumnAttributes.Length > 0? tableColumnAttributes[0].Title: serializedProperty.displayName;
                    if(!columnToProperties.TryGetValue(columnName, out List<SerializedPropertyInfo> list))
                    {
                        columnToProperties[columnName] = list = new List<SerializedPropertyInfo>();
                    }
                    list.Add(new SerializedPropertyInfo
                    {
                        Name = serializedProperty.name,
                        PropertyPath = serializedProperty.propertyPath,
                    });
                }

                // ReSharper disable once UseDeconstruction
                foreach (KeyValuePair<string, List<SerializedPropertyInfo>> columnKv in columnToProperties)
                {
                    string columnName = columnKv.Key;
                    List<SerializedPropertyInfo> properties = columnKv.Value;
                    IReadOnlyList<string> propNames = properties.Select(each => each.Name).ToArray();

                    string id = string.Join(";", properties.Select(each => each.PropertyPath));

                    multiColumnListView.columns.Add(new Column
                    {
                        name = id,
                        title = columnName,
                        stretchable = true,
                    });

                    multiColumnListView.columns[id].makeCell = () =>
                    {
                        VisualElement itemContainer = new VisualElement();
                        // PropertyField propField = new PropertyField();
                        // itemContainer.Add(propField);
                        // ReSharper disable once ForeachCanBePartlyConvertedToQueryUsingAnotherGetEnumerator
                        foreach (int _ in Enumerable.Range(0, properties.Count))
                        {
                            PropertyField propField = new PropertyField();
                            itemContainer.Add(propField);
                        }
                        return itemContainer;
                    };

                    multiColumnListView.columns[id].bindCell = (element, index) =>
                    {
                        SerializedProperty sp = (SerializedProperty)multiColumnListView.itemsSource[index];

                        foreach ((PropertyField propField, int propIndex) in element.Query<PropertyField>().ToList().WithIndex())
                        {
                            if (propIndex >= propNames.Count)
                            {
                                break;
                            }
                            string propName = propNames[propIndex];
                            SerializedProperty itemProp = sp.FindPropertyRelative(propName) ?? SerializedUtils.FindPropertyByAutoPropertyName(sp, propName);
                            propField.BindProperty(itemProp);
                            propField.Bind(itemProp.serializedObject);
                            propField.label = "";
                            Label firstLabel = propField.Query<Label>().First();
                            if(firstLabel != null)
                            {
                                if (!firstLabel.ClassListContains(ClassNoRichLabelUpdate))
                                {
                                    firstLabel.AddToClassList(ClassNoRichLabelUpdate);
                                }
                                if(itemProp.propertyType != SerializedPropertyType.Generic && firstLabel.style.display != DisplayStyle.None)
                                {
                                    firstLabel.style.display = DisplayStyle.None;
                                }
                            }
                        }
                    };
                }
            }
            root.Add(multiColumnListView);

            multiColumnListView.TrackPropertyValue(arrayProp, _ =>
            {
                // ReSharper disable once InvertIf
                if (_preArraySize != arrayProp.arraySize)
                {
                    _preArraySize = arrayProp.arraySize;
                    // MultiColumnListView multiColumnListView = container.Q<MultiColumnListView>();
                    multiColumnListView.itemsSource = MakeSource(arrayProp);
                    multiColumnListView.Rebuild();
                    arraySizeField.SetValueWithoutNotify(arrayProp.arraySize);
                }
            });

            // try to impletment copy/paste and failed
// #if SAINTSFIELD_NEWTONSOFT_JSON
//             bool focused = false;
//             multiColumnListView.RegisterCallback<FocusOutEvent>(_ => focused = false);
//             multiColumnListView.RegisterCallback<FocusInEvent>(_ => focused = true);
//             multiColumnListView.RegisterCallback<KeyDownEvent>(evt =>
//             {
//                 if (!focused)
//                 {
//                     return;
//                 }
//
//                 // ReSharper disable once MergeIntoLogicalPattern
//                 if (evt.keyCode == KeyCode.C && (evt.modifiers == EventModifiers.Control ||
//                                                  evt.modifiers == EventModifiers.Command))
//                 {
//                     SerializedProperty[] selected = multiColumnListView.selectedItems
//                         .Cast<SerializedProperty>()
//                         // .Select(each => SerializedUtils.PropertyPathIndex(each.propertyPath))
//                         .ToArray();
//                     // Debug.Log(string.Join(", ", selected));
//                     if (selected.Length == 0)
//                     {
//                         return;
//                     }
//
//                     List<object> results = new List<object>();
//                     foreach (SerializedProperty serProp in selected)
//                     {
//                         (string error, int index, object value) values = Util.GetValue(serProp, info, parent);
//                         if (values.error == "")
//                         {
//                             results.Add(values.value);
//                         }
//                     }
//
//                     // Debug.Log(string.Join(", ", results));
//
//                     if (results.Count == 0)
//                     {
//                         return;
//                     }
//
//                     bool isUObject = results[0] is Object;
//
//                     if (results.Count == 1)
//                     {
//
//                     }
//                 }
//                 // Debug.Log($"{focused}: {evt.keyCode}/{evt.commandKey}/{string.Join(",", evt.modifiers)}");
//             });
// #endif

            return root;
        }

        private ArraySizeAttribute _arraySizeAttribute;
        private bool _dynamic;
        private int _min = -1;
        private int _max = -1;

        protected override void OnAwakeUIToolkit(SerializedProperty property, ISaintsAttribute saintsAttribute, int index,
            IReadOnlyList<PropertyAttribute> allAttributes, VisualElement container, Action<object> onValueChangedCallback, FieldInfo info, object parent)
        {
            _arraySizeAttribute = allAttributes.OfType<ArraySizeAttribute>().FirstOrDefault();
            if (_arraySizeAttribute == null)
            {
                return;
            }

            (string error, bool dynamic, int min, int max) = ArraySizeAttributeDrawer.GetMinMax(_arraySizeAttribute, property, info, parent);
            if (error != "")
            {
                return;
            }

            _dynamic = dynamic;
            _min = min;
            _max = max;
        }

        protected override void OnUpdateUIToolkit(SerializedProperty property, ISaintsAttribute saintsAttribute, int index,
            VisualElement container, Action<object> onValueChanged, FieldInfo info)
        {
            if (_arraySizeAttribute is null)
            {
                return;
            }

            (string arrayError, SerializedProperty arrayProp) = SerializedUtils.GetArrayProperty(property);
            if (arrayError != "")
            {
                return;
            }

            if (_dynamic)
            {
                object parent = SerializedUtils.GetFieldInfoAndDirectParent(property).parent;
                if (parent == null)
                {
                    return;
                }

                (string error, bool _, int min, int max) = ArraySizeAttributeDrawer.GetMinMax(_arraySizeAttribute, property, info, parent);
                if (error != "")
                {
                    return;
                }

                _min = min;
                _max = max;
            }

            Button addButton = container.Q<Button>(name: NameAddButton(property));
            Button removeButton = container.Q<Button>(name: NameRemoveButton(property));

            // Debug.Log($"{arrayProp.arraySize}: {_max}");

            if(_max >= 0)
            {
                addButton.SetEnabled(arrayProp.arraySize < _max);
            }
            if(_min >= 0)
            {
                removeButton.SetEnabled(arrayProp.arraySize > _min);
            }
        }

        // this does not work because Unity internally update the style using inline
        // protected override void OnAwakeUIToolkit(SerializedProperty property, ISaintsAttribute saintsAttribute, int index,
        //     IReadOnlyList<PropertyAttribute> allAttributes, VisualElement container, Action<object> onValueChangedCallback, FieldInfo info, object parent)
        // {
        //     bool isRenderedBySaintsEditor = UIToolkitUtils.FindParentClass(container, AbsRenderer.ClassSaintsFieldPlayaContainer).Any();
        //     // Debug.Log($"isRenderedBySaintsEditor={isRenderedBySaintsEditor}");
        //     if (isRenderedBySaintsEditor)
        //     {
        //         return;
        //     }
        //
        //     ListView listView = UIToolkitUtils.IterUpWithSelf(container).OfType<ListView>().FirstOrDefault();
        //     if (listView == null)
        //     {
        //         return;
        //     }
        //
        //     VisualElement first = listView.Q<VisualElement>(name: "unity-list-view__reorderable-item");
        //     if (first != null && !first.ClassListContains("saints-field-first-item"))
        //     {
        //         first.AddToClassList("saints-field-first-item");
        //         StyleSheet uss = Util.LoadResource<StyleSheet>("UIToolkit/ListViewSkipRest.uss");
        //         Debug.Log($"uss={uss}");
        //         listView.styleSheets.Add(uss);
        //     }
        // }

        // protected override void OnUpdateUIToolkit(SerializedProperty property, ISaintsAttribute saintsAttribute, int index,
        //     VisualElement container, Action<object> onValueChanged, FieldInfo info)
        // {
        //     int propertyIndex = SerializedUtils.PropertyPathIndex(property.propertyPath);
        //     if (propertyIndex != 0)
        //     {
        //         return;
        //     }
        //
        //     (string error, SerializedProperty arrayProp) = SerializedUtils.GetArrayProperty(property);
        //     if (error != "")
        //     {
        //         return;
        //     }
        //
        //     if (_preArraySize != arrayProp.arraySize)
        //     {
        //         _preArraySize = arrayProp.arraySize;
        //         MultiColumnListView multiColumnListView = container.Q<MultiColumnListView>();
        //         multiColumnListView.itemsSource = MakeSource(arrayProp);
        //         multiColumnListView.Rebuild();
        //         container.Q<IntegerField>(name: NameArraySize(property)).SetValueWithoutNotify(arrayProp.arraySize);
        //     }
        // }

        private static List<SerializedProperty> MakeSource(SerializedProperty arrayProp)
        {
            return Enumerable.Range(0, arrayProp.arraySize)
                .Select(arrayProp.GetArrayElementAtIndex).ToList();
        }
    }
}
#elif UNITY_2021_3_OR_NEWER || SAINTSFIELD_DEBUG_UNITY_BROKEN_FALLBACK
using System.Collections.Generic;
using System.Reflection;
using SaintsField.Editor.Utils;
using SaintsField.Interfaces;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace SaintsField.Editor.Drawers.TableDrawer
{
    public partial class TableAttributeDrawer
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
                        GetFieldHeight(property, label, Screen.width, saintsAttribute, info, true, parent);
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
