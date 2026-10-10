#if UNITY_2021_3_OR_NEWER
using System.Linq;
using SaintsField.Editor.Core;
using SaintsField.Editor.UIToolkitElements;
using SaintsField.Editor.Utils;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

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

        private static string SessionKeyColumnWidth(SerializedProperty property, string columnName) =>
            $"{property.propertyPath}[{columnName}:width]";

        public static float GetSessionColumnWidth(SerializedProperty property, string columnName)
        {
            float percent = SessionState.GetFloat(SessionKeyColumnWidth(property, columnName), float.NaN);
            return !float.IsNaN(percent) && percent > 0f ? percent : float.NaN;
        }

        public static void SaveSessionColumnWidth(SerializedProperty property, string columnName, float percent)
        {
            if (!float.IsNaN(percent) && percent > 0f)
            {
                SessionState.SetFloat(SessionKeyColumnWidth(property, columnName), percent);
            }
        }

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
            FillTableToContainer(result, tableAttribute.DefaultCollapse);

            OnSearchFieldUIToolkit.AddListener(Search);
            result.RegisterCallback<DetachFromPanelEvent>(_ => OnSearchFieldUIToolkit.RemoveListener(Search));
            result.AddToClassList(SaintsPropertyDrawer.ClassLabelFieldUIToolkit);


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

        private void FillTableToContainer(VisualElement root, bool defaultCollapse)
        {
            SerializedProperty arrayProp = FieldWithInfo.SerializedProperty;

            CollectionFoldout foldout = new CollectionFoldout(arrayProp.displayName)
            {
                viewDataKey = NameTableContainer(arrayProp),
            };

            UIToolkitUtils.AddContextualMenuManipulator(foldout, arrayProp, () => {});
            root.Add(foldout);

            foldout.ArraySizeField.value = arrayProp.arraySize;

            VisualElement foldoutContent = foldout.contentContainer;
            foldoutContent.style.marginLeft = 0;

            _preArraySize = arrayProp.arraySize;

            TableContentElement tableContentElement = new TableContentElement(FieldWithInfo);
            foldout.Add(tableContentElement);

            foldout.MenuButton.clicked += () =>
            {
                GenericDropdownMenu genericDropdownMenu = new GenericDropdownMenu();
                if(tableContentElement.HasListView())
                {
                    genericDropdownMenu.AddItem("Collapse All", false, tableContentElement.CollapseAll);
                    genericDropdownMenu.AddItem("Expand All", false, tableContentElement.ExpandAll);
                }
                else
                {
                    genericDropdownMenu.AddDisabledItem("Collapse All", false);
                    genericDropdownMenu.AddDisabledItem("Expand All", false);
                }

                Rect menuBound = foldout.MenuButton.worldBound;
#if !UNITY_6000_3_OR_NEWER
                menuBound.xMin = menuBound.xMax - Mathf.Max(menuBound.width, 120f);
#endif
                genericDropdownMenu.DropDown(menuBound, foldout.MenuButton,
#if UNITY_6000_3_OR_NEWER
                    DropdownMenuSizeMode.Auto
#else
                    true
#endif
                );
            };

            if(defaultCollapse)
            {
                UIToolkitUtils.OnAttachToPanelOnceWithEnsure(foldout, () =>
                {
                    foldout.schedule.Execute(() =>
                    {
                        if (tableContentElement.HasListView())
                        {
                            tableContentElement.CollapseAll();
                        }
                    });
                });
            }

            foldout.ArraySizeField.RegisterValueChangedCallback(evt =>
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

            ListViewFooterButtonsElement listViewFooterButtons = new ListViewFooterButtonsElement
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
            listViewFooterButtons.AddButton.clicked += () =>
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
            if (_attribute.HideAddButton)
            {
                // addButton.style.display = DisplayStyle.None;
                listViewFooterButtons.AddButton.style.display = DisplayStyle.None;
            }
            // toolbar.Add(addButton);

            listViewFooterButtons.RemoveButton.clicked += () =>
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
            if (_attribute.HideRemoveButton)
            {
                // removeButton.style.display = DisplayStyle.None;
                listViewFooterButtons.RemoveButton.style.display = DisplayStyle.None;
            }
            // toolbar.Add(removeButton);

            if (_attribute.HideAddButton && _attribute.HideRemoveButton)
            {
                foldout.ArraySizeField.SetEnabled(false);
                // listViewFooter.style.display = DisplayStyle.None;
                listViewFooterButtons.ButtonsContainer.style.display = DisplayStyle.None;
            }

            // controls.Add(toolbar);

            // root.Add(toolbar);




            root.TrackPropertyValue(arrayProp, _ =>
            {
                // ReSharper disable once InvertIf
                if (_preArraySize != arrayProp.arraySize)
                {
                    _preArraySize = arrayProp.arraySize;
                    foldout.ArraySizeField.SetValueWithoutNotify(arrayProp.arraySize);
                }
            });

            // bool focused = false;
            // multiColumnListView.RegisterCallback<FocusOutEvent>(_ => focused = false);
            // multiColumnListView.RegisterCallback<FocusInEvent>(_ => focused = true);

// #endif

            foldout.Add(listViewFooterButtons);
        }

        public override void OnDestroyUIToolkit()
        {
        }
    }
}
#endif
