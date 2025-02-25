#if UNITY_2022_2_OR_NEWER
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using SaintsField.Editor.Linq;
using SaintsField.Editor.Utils;
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

            VisualElement root = new VisualElement();

            ArraySizeAttribute arraySizeAttribute = allAttributes.OfType<ArraySizeAttribute>().FirstOrDefault();

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
                        root.Add(BuildContent(arraySizeAttribute, root, property, info));
                    });
                    return root;
                }
            }

            BuildContent(arraySizeAttribute, root, property, info);
            return root;
        }

        private VisualElement BuildContent(ArraySizeAttribute arraySizeAttribute, VisualElement root, SerializedProperty property, FieldInfo info)
        {
            int min = 0;
            int max = int.MaxValue;
            if (arraySizeAttribute != null)
            {
                min = arraySizeAttribute.Min;
                max = arraySizeAttribute.Max;
                // Debug.Log($"{min} ~ {max}");
            }

            bool itemIsObject = property.propertyType == SerializedPropertyType.ObjectReference;

            int propertyIndex = SerializedUtils.PropertyPathIndex(property.propertyPath);
            if (propertyIndex != 0)
            {
                return new VisualElement();
            }
            (string error, SerializedProperty arrayProp) = SerializedUtils.GetArrayProperty(property);

            _preArraySize = arrayProp.arraySize;

            if (error != "")
            {
                return new HelpBox(error, HelpBoxMessageType.Error);
            }

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
            toolbar.Add(addButton);
            ToolbarButton removeButton = new ToolbarButton(() =>
            {
                DeleteArrayElement(arrayProp, multiColumnListView.selectedIndices);
            })
            {
                text = "-",
                name = NameRemoveButton(property),
            };
            toolbar.Add(removeButton);
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
                    // container.Q<IntegerField>(name: NameArraySize(property)).SetValueWithoutNotify(arrayProp.arraySize);

                    if (arraySizeAttribute != null)
                    {
                        // Debug.Log($"{arrayProp.arraySize}: {min} ~ {max}");
                        addButton.SetEnabled(arrayProp.arraySize < max);
                        removeButton.SetEnabled(arrayProp.arraySize > min);
                    }
                }
            });

            if (arraySizeAttribute != null)
            {
                addButton.SetEnabled(arrayProp.arraySize < max);
                removeButton.SetEnabled(arrayProp.arraySize > min);

                if (arraySizeAttribute.Min == arraySizeAttribute.Max)
                {
                    arraySizeField.SetEnabled(false);
                }
            }

            return root;
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
#elif UNITY_2021_3_OR_NEWER
using System.Collections.Generic;
using System.Reflection;
using SaintsField.Editor.Utils;
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
