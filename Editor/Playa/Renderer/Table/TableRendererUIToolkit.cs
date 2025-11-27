#if UNITY_2021_3_OR_NEWER //&& !SAINTSFIELD_UI_TOOLKIT_DISABLE
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using SaintsField.Editor.Drawers.SaintsRowDrawer;
using SaintsField.Editor.Playa.Renderer.BaseRenderer;
using SaintsField.Editor.UIToolkitElements;
using SaintsField.Editor.Utils;
using SaintsField.Playa;
using SaintsField.Utils;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

namespace SaintsField.Editor.Playa.Renderer.Table
{
    public partial class TableRenderer
    {
        private static string NameTableContainer(SerializedProperty property)
        {
            return $"saints-table-container-{SerializedUtils.GetUniqueId(property)}";
        }
        private static string NameAddButton(SerializedProperty property) => $"saints-table-container-{property.propertyPath}__Table_AddButton";
        private static string NameRemoveButton(SerializedProperty property) => $"saints-table-container-{property.propertyPath}__Table_RemoveButton";

        private bool _hasSize;

        protected override (VisualElement target, bool needUpdate) CreateSerializedUIToolkit()
        {
            TableAttribute tableAttribute = FieldWithInfo.PlayaAttributes.OfType<TableAttribute>().FirstOrDefault();
            Debug.Assert(tableAttribute != null, FieldWithInfo.SerializedProperty.propertyPath);

            VisualElement result = new VisualElement
            {
                name = NameTableContainer(FieldWithInfo.SerializedProperty),
            };

            // FillTableToContainer(result);
            FillTableToContainer(result);

            OnSearchFieldUIToolkit.AddListener(Search);
            result.RegisterCallback<DetachFromPanelEvent>(_ => OnSearchFieldUIToolkit.RemoveListener(Search));

            return (result, true);

            void Search(string search)
            {
                DisplayStyle display = Util.UnityDefaultSimpleSearch(FieldWithInfo.SerializedProperty.displayName, search)
                    ? DisplayStyle.Flex
                    : DisplayStyle.None;

                if (result.style.display != display)
                {
                    result.style.display = display;
                }
            }
        }

        private int _preArraySize;

        private void FillTableToContainer(VisualElement root)
        {
            SerializedProperty arrayProp = FieldWithInfo.SerializedProperty;
            TableAttribute tableAttribute = FieldWithInfo.PlayaAttributes.OfType<TableAttribute>().First();

            Foldout foldout = new Foldout
            {
                text = arrayProp.displayName,
                viewDataKey = NameTableContainer(arrayProp),
                style =
                {

                    // marginRight = 54,
                },
            };
            root.Add(foldout);

            IntegerField arraySizeField = new IntegerField
            {
                isDelayed = true,
                value = arrayProp.arraySize,
                style =
                {
                    width = 50,
                    position = Position.Absolute,
                    top = 0,
                    right = 0,
                    // marginLeft = 0,
                    // alignSelf = Align.FlexEnd,
                    // marginTop = -18,
                },
            };
            root.Add(arraySizeField);


            VisualElement foldoutContent = foldout.contentContainer;
            foldoutContent.style.marginLeft = 0;

            // container.Clear();


            // _hasSize = FillTable(FieldWithInfo.SerializedProperty, container, elementType, FieldWithInfo.SerializedProperty);


            // bool itemIsObject = arrayProp.propertyType == SerializedPropertyType.ObjectReference;

            // (string error, SerializedProperty arrayProp) = SerializedUtils.GetArrayProperty(property);

            _preArraySize = arrayProp.arraySize;

            // if (error != "")
            // {
            //     return new HelpBox(error, HelpBoxMessageType.Error);
            // }

            TableContentElement tableContentElement = new TableContentElement(FieldWithInfo);
            foldout.Add(tableContentElement);

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
            });

            // controls.Add(arraySizeField);

            ListViewFooterElement listViewFooter = new ListViewFooterElement
            {
                AddButton =
                {
                    name = NameAddButton(arrayProp),
                },
                RemoveButton =
                {
                    name = NameRemoveButton(arrayProp),
                },
            };
            listViewFooter.AddButton.clicked += () =>
            {
                int oldValue = arrayProp.arraySize;
                ChangeArraySize(oldValue + 1, arrayProp);
            };

            // Toolbar toolbar = new Toolbar();
            // ToolbarButton addButton = new ToolbarButton(() =>
            // {
            //     int oldValue = arrayProp.arraySize;
            //     ChangeArraySize(oldValue + 1, arrayProp);
            // })
            // {
            //     text = "+",
            //     name = NameAddButton(property),
            // };
            if (tableAttribute.HideAddButton)
            {
                // addButton.style.display = DisplayStyle.None;
                listViewFooter.AddButton.style.display = DisplayStyle.None;
            }
            // toolbar.Add(addButton);

            listViewFooter.RemoveButton.clicked += () =>
            {
                DeleteArrayElement(arrayProp, tableContentElement.SelectedIndices());
            };

            // ToolbarButton removeButton = new ToolbarButton(() =>
            // {
            //     DeleteArrayElement(arrayProp, multiColumnListView.selectedIndices);
            // })
            // {
            //     text = "-",
            //     name = NameRemoveButton(property),
            // };
            if (tableAttribute.HideRemoveButton)
            {
                // removeButton.style.display = DisplayStyle.None;
                listViewFooter.RemoveButton.style.display = DisplayStyle.None;
            }
            // toolbar.Add(removeButton);

            if (tableAttribute.HideAddButton && tableAttribute.HideRemoveButton)
            {
                arraySizeField.SetEnabled(false);
            }

            // controls.Add(toolbar);

            // root.Add(toolbar);




            root.TrackPropertyValue(arrayProp, _ =>
            {
                // ReSharper disable once InvertIf
                if (_preArraySize != arrayProp.arraySize)
                {
                    _preArraySize = arrayProp.arraySize;
                    arraySizeField.SetValueWithoutNotify(arrayProp.arraySize);
                }
            });

            // bool focused = false;
            // multiColumnListView.RegisterCallback<FocusOutEvent>(_ => focused = false);
            // multiColumnListView.RegisterCallback<FocusInEvent>(_ => focused = true);

// #endif

            foldout.Add(listViewFooter);
        }

        private static int ChangeArraySize(int newValue, SerializedProperty arrayProp)
        {
            int oldValue = arrayProp.arraySize;
            if (newValue == oldValue)
            {
                return oldValue;
            }

            arrayProp.arraySize = newValue;

            if (newValue > oldValue)  // add
            {
                if (arrayProp.GetArrayElementAtIndex(0).propertyType == SerializedPropertyType.ObjectReference)

                {
                    foreach (int index in Enumerable.Range(oldValue, newValue - oldValue))
                    {
                        arrayProp.GetArrayElementAtIndex(index).objectReferenceValue = null;
                    }
                }
            }

            arrayProp.serializedObject.ApplyModifiedProperties();
            return newValue;
        }

        private static void DeleteArrayElement(SerializedProperty arrayProp, IEnumerable<int> selectedIndices)
        {
            List<int> indexes = selectedIndices.OrderByDescending(each => each).Where(each => each < arrayProp.arraySize).ToList();
            if (indexes.Count == 0)
            {
                indexes.Add(arrayProp.arraySize - 1);
            }

            foreach (int index in indexes)
            {
                arrayProp.DeleteArrayElementAtIndex(index);
            }

            arrayProp.serializedObject.ApplyModifiedProperties();
        }





        // protected override PreCheckResult OnUpdateUIToolKit(VisualElement root)
        // {
        //     PreCheckResult result = base.OnUpdateUIToolKit(root);
        //
        //     int curSize = FieldWithInfo.SerializedProperty.arraySize;
        //     // ReSharper disable once InvertIf
        //     if ((curSize > 0 && !_hasSize)
        //         || (curSize == 0 && _hasSize))
        //     {
        //         VisualElement tableContainer =
        //             root.Q<VisualElement>(name: NameTableContainer(FieldWithInfo.SerializedProperty));
        //         // tableContainer.Clear();
        //
        //         FillTableToContainer(tableContainer);
        //     }
        //
        //     return result;
        // }

        // private static bool FillTable(SerializedProperty arrayProperty, VisualElement result, Type elementType, SerializedProperty property)
        // {
        //     bool hasSize = arrayProperty.arraySize > 0;
        //     SerializedProperty targetProperty = hasSize
        //         ? arrayProperty.GetArrayElementAtIndex(0)
        //         : arrayProperty;
        //
        //     PropertyField propField = new PropertyField(targetProperty)
        //     {
        //         style =
        //         {
        //             flexGrow = 1,
        //         },
        //     };
        //     propField.Bind(arrayProperty.serializedObject);
        //
        //     if (hasSize)
        //     {
        //         Foldout foldout = new Foldout
        //         {
        //             value = arrayProperty.isExpanded,
        //             text = arrayProperty.displayName,
        //             style = { flexGrow = 1 },
        //             viewDataKey = arrayProperty.propertyPath,
        //         };
        //         UIToolkitUtils.AddContextualMenuManipulator(foldout, arrayProperty, () => {});
        //         foldout.Add(propField);
        //         result.Add(foldout);
        //
        //         result.Add(new EmptyPrefabOverrideElement(arrayProperty)
        //         {
        //             style =
        //             {
        //                 position = Position.Absolute,
        //                 top = 0,
        //                 bottom = 0,
        //                 left = 0,
        //                 right = 0,
        //                 height = 18,
        //             },
        //             pickingMode = PickingMode.Ignore,
        //         });
        //
        //         #region Drag
        //         VisualElement foldoutInput = foldout.Q<VisualElement>(classes: "unity-foldout__input");
        //
        //         foldoutInput.RegisterCallback<DragEnterEvent>(_ =>
        //         {
        //             // Debug.Log($"Drag Enter {evt}");
        //             DragAndDrop.visualMode = CanDrop(DragAndDrop.objectReferences, elementType).Any()
        //                 ? DragAndDropVisualMode.Copy
        //                 : DragAndDropVisualMode.Rejected;
        //         });
        //         foldoutInput.RegisterCallback<DragLeaveEvent>(_ =>
        //         {
        //             // Debug.Log($"Drag Leave {evt}");
        //             DragAndDrop.visualMode = DragAndDropVisualMode.None;
        //         });
        //         foldoutInput.RegisterCallback<DragUpdatedEvent>(_ =>
        //         {
        //             // Debug.Log($"Drag Update {evt}");
        //             // DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
        //             DragAndDrop.visualMode = CanDrop(DragAndDrop.objectReferences, elementType).Any()
        //                 ? DragAndDropVisualMode.Copy
        //                 : DragAndDropVisualMode.Rejected;
        //         });
        //         foldoutInput.RegisterCallback<DragPerformEvent>(_ =>
        //         {
        //             // Debug.Log($"Drag Perform {evt}");
        //             if (!DropUIToolkit(elementType, property))
        //             {
        //                 return;
        //             }
        //
        //             property.serializedObject.ApplyModifiedProperties();
        //         });
        //         #endregion
        //     }
        //     else
        //     {
        //         result.Add(propField);
        //     }
        //
        //     return hasSize;
        // }
    }
}
#endif
