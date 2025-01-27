﻿#if UNITY_2022_2_OR_NEWER
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using SaintsField.Editor.Utils;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;


namespace SaintsField.Editor.Drawers.TableDrawer
{
    public partial class TableAttributeDrawer
    {
        private static string NameArraySize(SerializedProperty property) => $"{property.propertyPath}__Table_arraySize";

        private int _preArraySize;

        protected override VisualElement CreateFieldUIToolKit(SerializedProperty property, ISaintsAttribute saintsAttribute,
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
                        root.Add(BuildContent(root, property, saintsAttribute, container, info, parent));
                    });
                    return root;
                }
            }

            BuildContent(root, property, saintsAttribute, container, info, parent);
            return root;
        }

        private VisualElement BuildContent(VisualElement root, SerializedProperty property, ISaintsAttribute saintsAttribute,
            VisualElement container, FieldInfo info, object parent)
        {
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
#if UNITY_6000_0_OR_NEWER
                sortingMode = ColumnSortingMode.Default,
// #else
//                 sortingEnabled = true,
#endif
            };

            IntegerField integerField = new IntegerField
            {
                value = arrayProp.arraySize,
                isDelayed = true,
                style =
                {
                    minWidth = 50,
                },
                name = NameArraySize(property),
            };
            integerField.RegisterValueChangedCallback(evt =>
            {
                int newValue = evt.newValue;
                int oldValue = arrayProp.arraySize;
                if (newValue == oldValue)
                {
                    return;
                }

                arrayProp.arraySize = newValue;
                _preArraySize = newValue;

                if (newValue > oldValue)  // add
                {
                    if (itemIsObject)
                    {
                        foreach (int index in Enumerable.Range(oldValue, newValue - oldValue))
                        {
                            arrayProp.GetArrayElementAtIndex(index).objectReferenceValue = null;
                        }
                    }
                }

                arrayProp.serializedObject.ApplyModifiedProperties();
                multiColumnListView.itemsSource = MakeSource(arrayProp);
                multiColumnListView.Rebuild();
            });
            controls.Add(integerField);

            Toolbar toolbar = new Toolbar();
            ToolbarButton addButton = new ToolbarButton(() =>
            {
                arrayProp.arraySize++;
                if (itemIsObject)
                {
                    arrayProp.GetArrayElementAtIndex(arrayProp.arraySize - 1).objectReferenceValue = null;
                }
                arrayProp.serializedObject.ApplyModifiedProperties();
                integerField.SetValueWithoutNotify(arrayProp.arraySize);
                _preArraySize = arrayProp.arraySize;
                multiColumnListView.itemsSource = MakeSource(arrayProp);
                multiColumnListView.Rebuild();
            })
            {
                text = "+",
            };
            toolbar.Add(addButton);
            ToolbarButton removeButton = new ToolbarButton(() =>
            {
                int selected = multiColumnListView.selectedIndex;
                if (selected == -1)
                {
                    arrayProp.arraySize--;
                }
                else
                {
                    arrayProp.DeleteArrayElementAtIndex(selected);
                }
                arrayProp.serializedObject.ApplyModifiedProperties();
                _preArraySize = arrayProp.arraySize;
                integerField.SetValueWithoutNotify(arrayProp.arraySize);
                multiColumnListView.itemsSource = MakeSource(arrayProp);
                multiColumnListView.Rebuild();
            })
            {
                text = "-",
            };
            toolbar.Add(removeButton);
            controls.Add(toolbar);

            root.Add(controls);

            if (itemIsObject)
            {
                Object obj0 = multiColumnListView.itemsSource.Cast<SerializedProperty>().Select(each => each.objectReferenceValue).FirstOrDefault(each => each != null);
                SerializedObject serializedObject = new SerializedObject(obj0);
                foreach (SerializedProperty serializedProperty in SerializedUtils.GetAllField(serializedObject))
                {
                    string propName = serializedProperty.name;
                    multiColumnListView.columns.Add(new Column { name = serializedProperty.propertyPath, title = serializedProperty.displayName, stretchable = true });
                    multiColumnListView.columns[serializedProperty.propertyPath].makeCell = () =>
                    {
                        VisualElement itemContainer = new VisualElement();
                        PropertyField propField = new PropertyField();
                        itemContainer.Add(propField);
                        return itemContainer;
                    };

                    multiColumnListView.columns[serializedProperty.propertyPath].bindCell = (element, index) =>
                    {
                        PropertyField propField = element.Q<PropertyField>();

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
                        SerializedProperty itemProp = itemObject.FindProperty(propName);
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
                    };
                }
            }
            else
            {
                SerializedProperty firstProp = arrayProp.GetArrayElementAtIndex(0);
                // Debug.Log($"rendering generic {firstProp.propertyPath}");
                foreach (SerializedProperty serializedProperty in SerializedUtils.GetPropertyChildren(firstProp).Where(each => each != null))
                {
                    string propName = serializedProperty.name;
                    // Debug.Log(propName);
                    multiColumnListView.columns.Add(new Column { name = serializedProperty.propertyPath, title = serializedProperty.displayName, stretchable = true });
                    multiColumnListView.columns[serializedProperty.propertyPath].makeCell = () =>
                    {
                        VisualElement itemContainer = new VisualElement();
                        PropertyField propField = new PropertyField();
                        itemContainer.Add(propField);
                        return itemContainer;
                    };

                    multiColumnListView.columns[serializedProperty.propertyPath].bindCell = (element, index) =>
                    {
                        PropertyField propField = element.Q<PropertyField>();

                        SerializedProperty sp = (SerializedProperty)multiColumnListView.itemsSource[index];

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
                    };
                }
                // Debug.Log($"done generic {firstProp.propertyPath}");
            }
            root.Add(multiColumnListView);

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

        protected override void OnUpdateUIToolkit(SerializedProperty property, ISaintsAttribute saintsAttribute, int index,
            VisualElement container, Action<object> onValueChanged, FieldInfo info)
        {
            int propertyIndex = SerializedUtils.PropertyPathIndex(property.propertyPath);
            if (propertyIndex != 0)
            {
                return;
            }

            (string error, SerializedProperty arrayProp) = SerializedUtils.GetArrayProperty(property);
            if (error != "")
            {
                return;
            }

            if (_preArraySize != arrayProp.arraySize)
            {
                _preArraySize = arrayProp.arraySize;
                MultiColumnListView multiColumnListView = container.Q<MultiColumnListView>();
                multiColumnListView.itemsSource = MakeSource(arrayProp);
                multiColumnListView.Rebuild();
                container.Q<IntegerField>(name: NameArraySize(property)).SetValueWithoutNotify(arrayProp.arraySize);
            }
        }

        private static List<SerializedProperty> MakeSource(SerializedProperty arrayProp)
        {
            return Enumerable.Range(0, arrayProp.arraySize)
                .Select(arrayProp.GetArrayElementAtIndex).ToList();
        }
    }
}
#endif
