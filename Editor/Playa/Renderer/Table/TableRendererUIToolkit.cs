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

        protected override (VisualElement target, bool needUpdate) CreateSerializedUIToolkit()
        {
            TableAttribute tableAttribute = FieldWithInfo.PlayaAttributes.OfType<TableAttribute>().FirstOrDefault();
            Debug.Assert(tableAttribute != null, FieldWithInfo.SerializedProperty.propertyPath);

            VisualElement result = new VisualElement
            {
                name = NameTableContainer(FieldWithInfo.SerializedProperty),
            };

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
        private TableContentElement _tableContentElement;

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

            TableContentElement tableContentElement = _tableContentElement = new TableContentElement(FieldWithInfo, _attribute);
            foldout.Add(tableContentElement);

            ListViewPagerElement pager = new ListViewPagerElement
            {
                style = { display = _attribute.NumberOfItemsPerPage > 0 ? DisplayStyle.Flex : DisplayStyle.None },
            };
            pager.NumberOfItemsPerPageField.SetValueWithoutNotify(Mathf.Max(0, _attribute.NumberOfItemsPerPage));
            pager.NumberOfItemsTotalField.SetValueWithoutNotify(arrayProp.arraySize);
            tableContentElement.PageChanged += (pageIndex, pageCount) =>
            {
                pager.PagePreButton.SetEnabled(pageIndex > 0);
                pager.PageNextButton.SetEnabled(pageIndex < pageCount - 1);
                pager.PageField.SetValueWithoutNotify(pageIndex + 1);
                pager.PageLabel.text = $" / {pageCount}";
            };
            pager.PagePreButton.clicked += () => tableContentElement.SetPage(tableContentElement.CurPageIndex - 1, pager.NumberOfItemsPerPageField.value);
            pager.PageNextButton.clicked += () => tableContentElement.SetPage(tableContentElement.CurPageIndex + 1, pager.NumberOfItemsPerPageField.value);
            pager.PageField.RegisterValueChangedCallback(evt => tableContentElement.SetPage(evt.newValue - 1, pager.NumberOfItemsPerPageField.value));
            pager.NumberOfItemsPerPageField.RegisterValueChangedCallback(evt =>
            {
                int perPage = Mathf.Max(0, evt.newValue);
                pager.NumberOfItemsPerPageField.SetValueWithoutNotify(perPage);
                tableContentElement.SetPage(tableContentElement.CurPageIndex, perPage);
            });
            tableContentElement.SetPage(0, pager.NumberOfItemsPerPageField.value);

            foldout.MenuButton.clicked += () =>
            {
                GenericDropdownMenu genericDropdownMenu = new GenericDropdownMenu();
                bool curPaging = pager.style.display != DisplayStyle.None;
                genericDropdownMenu.AddItem("Paging", curPaging, () =>
                {
                    pager.style.display = curPaging ? DisplayStyle.None : DisplayStyle.Flex;
                    pager.NumberOfItemsPerPageField.value = curPaging ? 0 : _attribute.NumberOfItemsPerPage > 0
                        ? _attribute.NumberOfItemsPerPage : Mathf.Max(5, arrayProp.arraySize / 2);
                });

                genericDropdownMenu.AddSeparator("");

                bool curSearchAll = tableContentElement.SearchableAll;
                bool curSearchCols = tableContentElement.SearchableCols;
                genericDropdownMenu.AddItem("Search Table", curSearchAll, () => tableContentElement.SetSearchableAll(!curSearchAll));
                genericDropdownMenu.AddItem("Search Columns", curSearchCols, () => tableContentElement.SetSearchableCols(!curSearchCols));
                bool curSearch = curSearchAll || curSearchCols;
                if (curSearch && tableContentElement.HasExtraSearch)
                {
                    genericDropdownMenu.AddItem("Default Search", tableContentElement.DefaultSearch, () =>
                    {
                        tableContentElement.DefaultSearch = !tableContentElement.DefaultSearch;
                        tableContentElement.RefreshSearchingStatus();
                    });
                }
                else
                {
                    genericDropdownMenu.AddDisabledItem("Default Search", tableContentElement.DefaultSearch);
                }
                if (curSearch)
                {
                    genericDropdownMenu.AddItem("Object Search", tableContentElement.ObjectNestedSearch, () =>
                    {
                        tableContentElement.ObjectNestedSearch = !tableContentElement.ObjectNestedSearch;
                        tableContentElement.RefreshSearchingStatus();
                    });
                }
                else
                {
                    genericDropdownMenu.AddDisabledItem("Object Search", tableContentElement.ObjectNestedSearch);
                }
                if (tableContentElement.HasExtraSearch)
                {
                    if (curSearch)
                    {
                        genericDropdownMenu.AddItem("Extra Search", tableContentElement.ExtraSearch, () =>
                        {
                            tableContentElement.ExtraSearch = !tableContentElement.ExtraSearch;
                            tableContentElement.RefreshSearchingStatus();
                        });
                    }
                    else
                    {
                        genericDropdownMenu.AddDisabledItem("Extra Search", tableContentElement.ExtraSearch);
                    }
                }
                genericDropdownMenu.AddSeparator("");
                if (tableContentElement.HasListView())
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

            if (defaultCollapse)
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

            foldout.ArraySizeField.RegisterValueChangedCallback(evt => Resize(evt.newValue));
            pager.NumberOfItemsTotalField.RegisterValueChangedCallback(evt => Resize(evt.newValue));

            VisualElement footer = new VisualElement
            {
                style =
                {
                    flexDirection = FlexDirection.Row,
                    justifyContent = Justify.FlexEnd,
                },
            };
            foldout.Add(footer);

            footer.Add(pager);

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
            footer.Add(listViewFooterButtons);

            listViewFooterButtons.AddButton.clicked += () =>
            {
                int oldValue = arrayProp.arraySize;
                Resize(oldValue + 1);
                tableContentElement.SetPage(int.MaxValue, pager.NumberOfItemsPerPageField.value);
            };

            if (_attribute.HideAddButton)
            {
                listViewFooterButtons.AddButton.style.display = DisplayStyle.None;
            }

            listViewFooterButtons.RemoveButton.clicked += () =>
            {
                DeleteArrayElement(arrayProp, tableContentElement.SelectedIndices());
                foldout.ArraySizeField.SetValueWithoutNotify(arrayProp.arraySize);
                pager.NumberOfItemsTotalField.SetValueWithoutNotify(arrayProp.arraySize);
                _preArraySize = arrayProp.arraySize;
                tableContentElement.Refresh();
            };

            if (_attribute.HideRemoveButton)
            {
                listViewFooterButtons.RemoveButton.style.display = DisplayStyle.None;
            }

            if (_attribute.HideAddButton && _attribute.HideRemoveButton)
            {
                foldout.ArraySizeField.SetEnabled(false);
                pager.NumberOfItemsTotalField.SetEnabled(false);
                listViewFooterButtons.ButtonsContainer.style.display = DisplayStyle.None;
            }

            root.TrackPropertyValue(arrayProp, _ =>
            {
                // ReSharper disable once InvertIf
                if (_preArraySize != arrayProp.arraySize)
                {
                    _preArraySize = arrayProp.arraySize;
                    foldout.ArraySizeField.SetValueWithoutNotify(arrayProp.arraySize);
                    pager.NumberOfItemsTotalField.SetValueWithoutNotify(arrayProp.arraySize);
                }
            });
            return;

            void Resize(int newValue)
            {
                int oldValue = arrayProp.arraySize;
                int changedValue = ChangeArraySize(newValue, arrayProp);
                foldout.ArraySizeField.SetValueWithoutNotify(changedValue);
                pager.NumberOfItemsTotalField.SetValueWithoutNotify(changedValue);
                if (changedValue == oldValue)
                {
                    return;
                }

                _preArraySize = changedValue;
                tableContentElement.Refresh();
            }
        }

        public override void OnDestroyUIToolkit()
        {
            _tableContentElement?.StopSearch();
        }
    }
}
#endif
