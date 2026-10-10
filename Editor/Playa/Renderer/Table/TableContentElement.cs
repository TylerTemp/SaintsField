#if UNITY_2021_3_OR_NEWER
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using SaintsField.Editor.Drawers.SaintsRowDrawer;
using SaintsField.Editor.Playa.Renderer.BaseRenderer;
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
    public class TableContentElement: VisualElement
    {
        private readonly SaintsFieldWithInfo _fieldWithInfo;

        private readonly VisualElement _emptyNotice;
        private readonly VisualElement _tableContentContainer;

        private readonly HashSet<string> _valueTableHeaders;
        private readonly bool _headerIsHide;
        private readonly Type _elementType;

        private int _preArraySize;
        private MultiColumnListView _multiColumnListView;

        public TableContentElement(SaintsFieldWithInfo fieldWithInfo, TableAttribute settings)
        {
            _fieldWithInfo = fieldWithInfo;
            InitializeSearch(settings);
            // Empty
            // _emptyNotice = new HelpBox("Table is empty", HelpBoxMessageType.None);
            _emptyNotice = new VisualElement();
            _emptyNotice.AddToClassList("unity-collection-view--with-border");
            _emptyNotice.AddToClassList("unity-list-view__scroll-view--with-footer");
            _emptyNotice.Add(new Label("Table is empty")
            {
                style =
                {
                    height = 22,
                    paddingLeft = 6,
                    paddingRight = 2,
                    unityTextAlign = TextAnchor.MiddleLeft,
                    whiteSpace = WhiteSpace.NoWrap,
                },
            });
            Add(_emptyNotice);

            // Filled
            _tableContentContainer = new VisualElement();
            Add(_tableContentContainer);

            this.TrackPropertyValue(fieldWithInfo.SerializedProperty, _ => OnArrayPropertyChanged());

            _elementType =
                ReflectUtils.GetElementType(fieldWithInfo.FieldInfo?.FieldType ??
                                            fieldWithInfo.PropertyInfo.PropertyType);

            #region Headers

            MemberInfo info;
            if (fieldWithInfo.FieldInfo != null)
            {
                info = fieldWithInfo.FieldInfo;
            }
            else
            {
                info = fieldWithInfo.PropertyInfo;
            }
            TableHeadersAttribute tableHeadersAttribute = ReflectCache.GetCustomAttributes<TableHeadersAttribute>(info)
                .FirstOrDefault();
            _valueTableHeaders = new HashSet<string>();
            _headerIsHide = true;
            if (tableHeadersAttribute != null)
            {
                _headerIsHide = tableHeadersAttribute.IsHide;
                foreach (TableHeadersAttribute.Header header in tableHeadersAttribute.Headers)
                {
                    // string rawName = header.Name;
                    List<string> rawNames = new List<string>();
                    if (header.IsCallback)
                    {
                        (string error, object value) = Util.GetOfNoParams<object>(fieldWithInfo.Targets[0], header.Name, null);
                        if (error != "")
                        {
#if SAINTSFIELD_DEBUG
                            Debug.LogError(error);
#endif
                            continue;
                        }

                        if (RuntimeUtil.IsNull(value))
                        {
                        }
                        // ReSharper disable once ConvertIfStatementToSwitchStatement
                        else if (value is string s)
                        {
                            rawNames.Add(s);
                        }
                        else if (value is IEnumerable<string> si)
                        {
                            rawNames.AddRange(si.Where(each => each != null));
                        }
                        else if (value is IEnumerable<object> oi)
                        {
                            rawNames.AddRange(oi.Where(each => !RuntimeUtil.IsNull(each))
                                .Select(each => each.ToString()));
                        }
                    }
                    else
                    {
                        rawNames.Add(header.Name);
                    }

                    _valueTableHeaders.UnionWith(rawNames.SelectMany(each => new[]{each, ObjectNames.NicifyVariableName(each)}));
                }
            }
            #endregion

            OnArrayPropertyChanged();
        }

        public IEnumerable<int> SelectedIndices()
        {
            if (_preArraySize == 0)
            {
                return Array.Empty<int>();
            }

            return _multiColumnListView.selectedIndices
                .Where(each => each >= 0 && each < _asyncSearchItems.ItemIndexToPropertyIndex.Count)
                .Select(each => _asyncSearchItems.ItemIndexToPropertyIndex[each]).ToArray();
        }

        private void OnArrayPropertyChanged()
        {
            bool columnFoldoutRefreshScheduled = false;

            SerializedProperty arrayProp = _fieldWithInfo.SerializedProperty;
            int newArraySize = arrayProp.arraySize;
            if (newArraySize == 0)
            {
                _preArraySize = newArraySize;
                _emptyNotice.style.display = DisplayStyle.Flex;
                _tableContentContainer.style.display = DisplayStyle.None;
                _tableContentContainer.Clear();
                _multiColumnListView = null;
                RefreshSearchingStatus();
                return;
            }

            if (_preArraySize == 0)
            {
                _preArraySize = newArraySize;
                _emptyNotice.style.display = DisplayStyle.None;
                _tableContentContainer.style.display = DisplayStyle.Flex;
                _tableContentContainer.Clear();

                MultiColumnListView multiColumnListView = _multiColumnListView = new MultiColumnListView
                {
                    showBoundCollectionSize = false,
                    virtualizationMethod = CollectionVirtualizationMethod.DynamicHeight,
                    itemsSource = new List<int>(_asyncSearchItems.ItemIndexToPropertyIndex),
                    selectionType = SelectionType.Multiple,
                    reorderable = true,
                    reorderMode = ListViewReorderMode.Animated,
                    showBorder = true,

                    viewDataKey = SerializedUtils.GetUniqueId(arrayProp),

                    // this has some issue because we bind order with renderer. Sort is not possible
// #if UNITY_6000_0_OR_NEWER
//                 sortingMode = ColumnSortingMode.Default,
// #else
//                 sortingEnabled = true,
// #endif
                };
                multiColumnListView.selectedIndicesChanged += _ => ClearNestedSelectionAndFocusOutsideSelection(multiColumnListView);


#if UNITY_6000_0_OR_NEWER
                multiColumnListView.columns.propertyChanged += (_, _) => ScheduleColumnFoldoutRefreshDisplay();
#endif

                SerializedProperty firstProp = arrayProp.GetArrayElementAtIndex(0);
                bool itemIsObject = firstProp.propertyType == SerializedPropertyType.ObjectReference;

                if (itemIsObject)
                {
                    Object obj0 = Enumerable.Range(0, arrayProp.arraySize).Select(arrayProp.GetArrayElementAtIndex)
                        .Select(each => each.objectReferenceValue)
                        .FirstOrDefault(each => each);

                    if (!obj0)
                    {
                        // PropertyField nullProp = new PropertyField(child0);
                        // nullProp.Bind(child0.serializedObject);
                        // return nullProp;
                        ObjectField arrayItemProp = new ObjectField("")
                        {
                            objectType = ReflectUtils.GetElementType(_fieldWithInfo.FieldInfo?.FieldType??_fieldWithInfo.PropertyInfo.PropertyType),
                        };

                        _tableContentContainer.Add(arrayItemProp);

                        arrayItemProp.RegisterValueChangedCallback(evt =>
                        {
                            Object newValue = evt.newValue;
                            _tableContentContainer.Clear();
                            _preArraySize = 0;
                            _fieldWithInfo.SerializedProperty.GetArrayElementAtIndex(0).objectReferenceValue = newValue;
                            _fieldWithInfo.SerializedProperty.serializedObject.ApplyModifiedProperties();
                            OnArrayPropertyChanged();
                        });
                        return;
                    }

                    Dictionary<string, List<string>> columnToMemberIds = new Dictionary<string, List<string>>();
                    Dictionary<string, string> memberIdToPropertyName = new Dictionary<string, string>();
                    Dictionary<string, bool> columnToDefaultHide = new Dictionary<string, bool>();

                    using(SerializedObject serializedObject = new SerializedObject(obj0))
                    {
                        Dictionary<string, SerializedProperty> serializedPropertyDict = SerializedUtils
                            .GetAllField(serializedObject)
                            .Where(each => each != null)
                            .ToDictionary(each => each.name, each => each.Copy());
                        IEnumerable<SaintsFieldInfoName> saintsFieldWithInfos = SaintsEditor
                            .HelperGetSaintsFieldWithInfo(_fieldWithInfo.SerializedProperty.serializedObject, serializedPropertyDict, null, null, -1, new []{obj0})
                            .Where(TableRenderer.SaintsFieldInfoShouldDraw)
                            .Select(each => new SaintsFieldInfoName(each, AbsRenderer.GetFriendlyName(each)));

                        foreach (SaintsFieldInfoName saintsFieldInfoName in saintsFieldWithInfos)
                        {
                            string columnName = saintsFieldInfoName.FriendlyName;
                            foreach (IPlayaAttribute playaAttribute in saintsFieldInfoName.SaintsFieldWithInfo.PlayaAttributes)
                            {
                                // ReSharper disable once InvertIf
                                if (playaAttribute is TableColumnAttribute tc)
                                {
                                    columnName = tc.Title;
                                    break;
                                }
                            }

                            bool headerHide = HeaderDefaultHide(columnName, _valueTableHeaders, _headerIsHide);
                            if (headerHide)
                            {
                                columnToDefaultHide[columnName] = true;
                            }
                            else
                            {
                                // ReSharper disable once LoopCanBeConvertedToQuery
                                foreach (IPlayaAttribute playaAttribute in saintsFieldInfoName.SaintsFieldWithInfo
                                             .PlayaAttributes)
                                {
                                    // ReSharper disable once InvertIf
                                    if (playaAttribute is TableHideAttribute)
                                    {
                                        columnToDefaultHide[columnName] = true;
                                        break;
                                    }
                                }
                            }

                            if(!columnToMemberIds.TryGetValue(columnName, out List<string> list))
                            {
                                columnToMemberIds[columnName] = list = new List<string>();
                            }
                            // Debug.Log($"{columnName}: {saintsFieldWithInfo}");
                            list.Add(saintsFieldInfoName.SaintsFieldWithInfo.MemberId);
                            if (saintsFieldInfoName.SaintsFieldWithInfo.SerializedProperty != null)
                            {
                                memberIdToPropertyName[saintsFieldInfoName.SaintsFieldWithInfo.MemberId] = saintsFieldInfoName.SaintsFieldWithInfo.SerializedProperty.name;
                            }
                        }
                    }

                    // ReSharper disable once UseDeconstruction
                    foreach (KeyValuePair<string, List<string>> columnKv in columnToMemberIds)
                    {
                        string columnName = columnKv.Key;
                        List<string> memberIds = columnKv.Value;

                        string id = string.Join(";", memberIds);
                        RegisterSearchColumn(id, memberIds.Where(memberIdToPropertyName.ContainsKey).Select(each => memberIdToPropertyName[each]));

                        bool thisIsVisible = true;
                        if (columnToDefaultHide.TryGetValue(columnName, out bool hide))
                        {
                            thisIsVisible = !hide;
                        }

                        Column curColumn = new Column
                        {
                            name = id,
                            title = columnName,
                            stretchable = true,
                            visible = thisIsVisible,
                            makeHeader = () => MakeSearchHeader(id, columnName),
                        };
                        ApplySessionColumnWidth(curColumn);
                        multiColumnListView.columns.Add(curColumn);

                        curColumn.makeCell = MakeTableCellFoldableElement;

                        curColumn.bindCell = (element, index) =>
                        {
                            TableCellFoldableElement cellElement = (TableCellFoldableElement)element;
                            int propertyIndex = _asyncSearchItems.ItemIndexToPropertyIndex[index];
                            string viewKey = $"{SerializedUtils.GetUniqueId(arrayProp)}:{propertyIndex}";
                            cellElement.SetViewKey(viewKey);
                            RowData currentRowData = new RowData(multiColumnListView, index);
                            cellElement.userData = currentRowData;
                            cellElement.ToggleDisplay(false);

                            ScheduleColumnFoldoutRefreshDisplay();
                            SerializedProperty targetProp = arrayProp.GetArrayElementAtIndex(propertyIndex);
                            targetProp.isExpanded = true;

                            Object targetPropValue = targetProp.objectReferenceValue;

                            if (RuntimeUtil.IsNull(targetPropValue))
                            {
                                ObjectField arrayItemProp = new ObjectField("")
                                {
                                    objectType = _elementType,
                                };

                                element.Clear();
                                element.Add(arrayItemProp);

                                arrayItemProp.RegisterValueChangedCallback(evt =>
                                {
                                    targetProp.objectReferenceValue = evt.newValue;
                                    targetProp.serializedObject.ApplyModifiedProperties();
                                    multiColumnListView.Rebuild();
                                });

                                cellElement.BindButton(() => "Null");
                                return;
                            }

                            SerializedObject targetSerializedObject = new SerializedObject(targetPropValue);

                            Dictionary<string, SerializedProperty> targetPropertyDict = SerializedUtils
                                .GetAllField(targetSerializedObject)
                                .Where(each => each != null)
                                .ToDictionary(each => each.name, each => each.Copy());

                            List<SaintsFieldWithInfo> allSaintsFieldWithInfos =
                                new List<SaintsFieldWithInfo>(memberIds.Count);

                            int serCount = 0;
                            foreach (SaintsFieldWithInfo saintsFieldWithInfo in SaintsEditor
                                         .HelperGetSaintsFieldWithInfo(_fieldWithInfo.SerializedProperty.serializedObject, targetPropertyDict, null, null, -1, new[]{targetPropValue})
                                         .Where(saintsFieldWithInfo => memberIds.Contains(saintsFieldWithInfo.MemberId)))
                            {
                                allSaintsFieldWithInfos.Add(saintsFieldWithInfo);
                                if (saintsFieldWithInfo.SerializedProperty != null)
                                {
                                    serCount += 1;
                                }
                            }

                            cellElement.Clear();

                            bool saintsRowInline = memberIds.Count == 1;
                            bool noLabel = serCount <= 1;

                            List<AbsRenderer> allRenderers = new List<AbsRenderer>();

                            using(new SaintsRowAttributeDrawer.ForceInlineScoop(saintsRowInline? 1: 0))
                            {
                                // ReSharper disable once ForeachCanBePartlyConvertedToQueryUsingAnotherGetEnumerator
                                foreach (SaintsFieldWithInfo saintsFieldWithInfo in allSaintsFieldWithInfos)
                                {
                                    foreach (IReadOnlyList<SaintsFieldWithRenderer> renderers in SaintsEditor.HelperMakeRenderer(arrayProp.serializedObject, saintsFieldWithInfo))
                                    {
                                        // Debug.Log(renderer);
                                        // ReSharper disable once InvertIf
                                        foreach (SaintsFieldWithRenderer rendererInfo in renderers)
                                        {
                                            AbsRenderer renderer = rendererInfo.Renderer;
                                            if (renderer == null)
                                            {
                                                continue;
                                            }

                                            allRenderers.Add(renderer);
                                            renderer.NoLabel = noLabel;
                                            renderer.InDirectHorizontalLayout = renderer.InAnyHorizontalLayout = true;
                                            VisualElement fieldElement = renderer.CreateVisualElement(element);

                                            if (fieldElement != null)
                                            {
                                                element.Add(fieldElement);
                                            }
                                        }
                                    }
                                }
                            }

                            cellElement.BindButton(() =>
                            {
                                return string.Join(", ", allRenderers
                                    .Select(each => each.GetField("", "field", ""))
                                    .Where(each => !string.IsNullOrEmpty(each)));
                            });
                        };
#if UNITY_6000_0_OR_NEWER
                        curColumn.propertyChanged += (_, args) =>
                        {
                            ScheduleColumnFoldoutRefreshDisplay();
                            SaveSessionColumnWidth(curColumn, args);
                        };
#endif
                    }
                }
                else  // item is general
                {
                    // Debug.Log($"rendering generic {firstProp.propertyPath}");
                    Dictionary<string, SerializedProperty> firstSerializedPropertyDict = SerializedUtils.GetPropertyChildren(firstProp)
                        .Where(each => each != null)
                        .ToDictionary(each => each.name);

                    MemberInfo info = (MemberInfo)_fieldWithInfo.FieldInfo ?? _fieldWithInfo.PropertyInfo;

                    (PropertyAttribute[] _, object parentRefreshed) = SerializedUtils.GetAttributesAndDirectParent<PropertyAttribute>(firstProp);

                    (string error, int index, object value) firstPropValue = Util.GetValue(firstProp, info, parentRefreshed);

                    IEnumerable<SaintsFieldWithInfo> firstSaintsFieldWithInfos = SaintsEditor
                        .HelperGetSaintsFieldWithInfo(_fieldWithInfo.SerializedProperty.serializedObject, firstSerializedPropertyDict, null, null, -1, new[]{firstPropValue.value})
                        .Where(TableRenderer.SaintsFieldInfoShouldDraw);

                    Dictionary<string, List<string>> columnToMemberIds = new Dictionary<string, List<string>>();
                    Dictionary<string, string> memberIdToPropertyName = new Dictionary<string, string>();
                    Dictionary<string, bool> columnToDefaultHide = new Dictionary<string, bool>();

                    foreach (SaintsFieldWithInfo saintsFieldWithInfo in firstSaintsFieldWithInfos)
                    {
                        string columnName = AbsRenderer.GetFriendlyName(saintsFieldWithInfo);
                        foreach (IPlayaAttribute playaAttribute in saintsFieldWithInfo.PlayaAttributes)
                        {
                            // ReSharper disable once InvertIf
                            if (playaAttribute is TableColumnAttribute tc)
                            {
                                columnName = tc.Title;
                                break;
                            }
                        }

                        bool headerHide = HeaderDefaultHide(columnName, _valueTableHeaders, _headerIsHide);
                        if (headerHide)
                        {
                            columnToDefaultHide[columnName] = true;
                        }
                        else
                        {
                            // ReSharper disable once LoopCanBeConvertedToQuery
                            foreach (IPlayaAttribute playaAttribute in saintsFieldWithInfo.PlayaAttributes)
                            {
                                // ReSharper disable once InvertIf
                                if (playaAttribute is TableHideAttribute)
                                {
                                    columnToDefaultHide[columnName] = true;
                                    break;
                                }
                            }
                        }

                        if(!columnToMemberIds.TryGetValue(columnName, out List<string> list))
                        {
                            columnToMemberIds[columnName] = list = new List<string>();
                        }
                        // Debug.Log($"{columnName}: {saintsFieldWithInfo}");
                        list.Add(saintsFieldWithInfo.MemberId);
                        if (saintsFieldWithInfo.SerializedProperty != null)
                        {
                            memberIdToPropertyName[saintsFieldWithInfo.MemberId] = saintsFieldWithInfo.SerializedProperty.name;
                        }
                    }

                    // ReSharper disable once UseDeconstruction
                    foreach (KeyValuePair<string, List<string>> columnKv in columnToMemberIds)
                    {
                        string columnName = columnKv.Key;
                        List<string> memberIds = columnKv.Value;

                        string id = string.Join(";", memberIds);
                        RegisterSearchColumn(id, memberIds.Where(memberIdToPropertyName.ContainsKey).Select(each => memberIdToPropertyName[each]));

                        bool thisIsVisible = true;
                        if (columnToDefaultHide.TryGetValue(columnName, out bool hide))
                        {
                            thisIsVisible = !hide;
                        }

                        Column curColumn = new Column
                        {
                            name = id,
                            title = columnName,
                            stretchable = true,
                            visible = thisIsVisible,
                            makeHeader = () => MakeSearchHeader(id, columnName),
                        };
                        ApplySessionColumnWidth(curColumn);
                        multiColumnListView.columns.Add(curColumn);

                        curColumn.makeCell = MakeTableCellFoldableElement;
                        curColumn.bindCell = (element, index) =>
                        {
                            TableCellFoldableElement cellElement = (TableCellFoldableElement)element;
                            RowData currentRowData = new RowData(multiColumnListView, index);
                            int propertyIndex = _asyncSearchItems.ItemIndexToPropertyIndex[index];
                            cellElement.SetViewKey($"{SerializedUtils.GetUniqueId(arrayProp)}:{propertyIndex}");
                            cellElement.userData = currentRowData;
                            cellElement.ToggleDisplay(false);

                            // UIToolkitUtils.OnAttachToPanelOnce(cellElement, _ =>
                            // {
                            //     cellElement.RegisterValueChangedCallback(expand =>
                            //     {
                            //         Debug.Log($"{index}/{expand}");
                            //     });
                            // });

                            ScheduleColumnFoldoutRefreshDisplay();
                            // Debug.Log($"id={id}/index={index}");
                            SerializedProperty targetProp = arrayProp.GetArrayElementAtIndex(propertyIndex);
                            targetProp.isExpanded = true;

                            (PropertyAttribute[] _, object thisParentRefreshed) = SerializedUtils.GetAttributesAndDirectParent<PropertyAttribute>(targetProp);

                            (string error, int index, object value) targetPropValue = Util.GetValue(targetProp, info, thisParentRefreshed);
                            if (targetPropValue.error != "")
                            {
                                cellElement.Clear();
                                cellElement.Add(new HelpBox(targetPropValue.error, HelpBoxMessageType.Error));
                                return;
                            }
                            Dictionary<string, SerializedProperty> targetSerializedPropertyDict = SerializedUtils.GetPropertyChildren(targetProp)
                                .Where(each => each != null)
                                .ToDictionary(each => each.name);

                            List<SaintsFieldWithInfo>  allSaintsFieldWithInfos = new List<SaintsFieldWithInfo>(memberIds.Count);

                            // Debug.Log($"looking {targetPropValue.value}({targetPropValue.error})");

                            int serCount = 0;
                            foreach (SaintsFieldWithInfo saintsFieldWithInfo in SaintsEditor
                                         .HelperGetSaintsFieldWithInfo(_fieldWithInfo.SerializedProperty.serializedObject, targetSerializedPropertyDict, null, null, -1, new[]{targetPropValue.value})
                                         .Where(saintsFieldWithInfo => memberIds.Contains(saintsFieldWithInfo.MemberId)))
                            {
                                // Debug.Log($"get {saintsFieldWithInfo}");
                                allSaintsFieldWithInfos.Add(saintsFieldWithInfo);
                                if (saintsFieldWithInfo.SerializedProperty != null)
                                {
                                    serCount += 1;
                                }
                            }

                            cellElement.Clear();

                            bool saintsRowInline = memberIds.Count == 1;
                            bool noLabel = serCount <= 1;

                            List<AbsRenderer> allRenderers = new List<AbsRenderer>();

                            using(new SaintsRowAttributeDrawer.ForceInlineScoop(saintsRowInline? 1: 0))
                            {
                                // ReSharper disable once ForeachCanBePartlyConvertedToQueryUsingAnotherGetEnumerator
                                foreach (SaintsFieldWithInfo saintsFieldWithInfo in allSaintsFieldWithInfos)
                                {
                                    foreach (IReadOnlyList<SaintsFieldWithRenderer> renderers in SaintsEditor.HelperMakeRenderer(arrayProp.serializedObject, saintsFieldWithInfo))
                                    {
                                        foreach (SaintsFieldWithRenderer rendererInfo in renderers)
                                        {
                                            AbsRenderer renderer = rendererInfo.Renderer;
                                            if (renderer == null)
                                            {
                                                continue;
                                            }

                                            allRenderers.Add(renderer);
                                            renderer.NoLabel = noLabel;
                                            renderer.InDirectHorizontalLayout = renderer.InAnyHorizontalLayout = true;
                                            VisualElement fieldElement = renderer.CreateVisualElement(element);
                                            if (fieldElement != null)
                                            {
                                                cellElement.Add(fieldElement);
                                            }
                                        }
                                        // if(renderer != null)
                                        // {
                                        //     renderer.NoLabel = noLabel;
                                        //     renderer.InDirectHorizontalLayout = renderer.InAnyHorizontalLayout = true;
                                        //     VisualElement fieldElement = renderer.CreateVisualElement();
                                        //     if (fieldElement != null)
                                        //     {
                                        //         element.Add(fieldElement);
                                        //     }
                                        // }
                                    }
                                }
                            }

                            cellElement.BindButton(() =>
                            {
                                return string.Join(", ", allRenderers
                                    .Select(each => each.GetField("", "field", ""))
                                    .Where(each => !string.IsNullOrEmpty(each)));
                            });
                        };
#if UNITY_6000_0_OR_NEWER
                        curColumn.propertyChanged += (_, args) =>
                        {
                            ScheduleColumnFoldoutRefreshDisplay();
                            SaveSessionColumnWidth(curColumn, args);
                        };
#endif
                    }
                }

                multiColumnListView.itemIndexChanged += (first, second) =>
                {
// #if SAINTSFIELD_DEBUG
//                 Debug.Log($"drag {first}({first}) -> {second}({second}) for {arrayProp.propertyPath}({arrayProp.arraySize})");
// #endif

                    int fromPropIndex = _asyncSearchItems.ItemIndexToPropertyIndex[first];
                    int toPropIndex = _asyncSearchItems.ItemIndexToPropertyIndex[second];
                    arrayProp.MoveArrayElement(fromPropIndex, toPropIndex);
                    arrayProp.serializedObject.ApplyModifiedProperties();
                    multiColumnListView.itemsSource = new List<int>(_asyncSearchItems.ItemIndexToPropertyIndex);
                    multiColumnListView.Rebuild();
                    RefreshSearchingStatus();
                };
                multiColumnListView.RegisterCallback<KeyDownEvent>(evt =>
                {
                // ReSharper disable once MergeIntoLogicalPattern
                bool ctrl = evt.modifiers == EventModifiers.Control ||
                            evt.modifiers == EventModifiers.Command;

                bool copyCommand = ctrl && evt.keyCode == KeyCode.C;
                if (copyCommand)
                {
                    SerializedProperty selected = SelectedIndices()
                        .Select(arrayProp.GetArrayElementAtIndex)
                        // .Select(each => SerializedUtils.PropertyPathIndex(each.propertyPath))
                        .FirstOrDefault();
                    // Debug.Log(string.Join(", ", selected));
                    if (selected == null)
                    {
                        return;
                    }

                    if (ClipboardHelper.CanCopySerializedProperty(selected.propertyType))
                    {
                        ClipboardHelper.DoCopySerializedProperty(selected);
                    }
                }

                bool pasteCommand = ctrl && evt.keyCode == KeyCode.V;
                if (pasteCommand)
                {
                    SerializedProperty selected = SelectedIndices()
                        .Select(arrayProp.GetArrayElementAtIndex)
                        // .Select(each => SerializedUtils.PropertyPathIndex(each.propertyPath))
                        .FirstOrDefault();
                    // Debug.Log(string.Join(", ", selected));
                    if (selected == null)
                    {
                        return;
                    }

                    (bool pasteHasReflection, bool pasteHasValue) = ClipboardHelper.CanPasteSerializedProperty(selected.propertyType);
                    // Debug.Log($"{pasteHasReflection}, {pasteHasValue}");
                    if (pasteHasReflection && pasteHasValue)
                    {
                        ClipboardHelper.DoPasteSerializedProperty(selected);
                        selected.serializedObject.ApplyModifiedProperties();
                        RefreshSearchingStatus();
                    }
                }
                });

                _tableContentContainer.Add(multiColumnListView);
                ScheduleColumnFoldoutRefreshDisplay();
                RefreshSearchingStatus();

                return;

                void ScheduleColumnFoldoutRefreshDisplay()
                {
                    if (columnFoldoutRefreshScheduled)
                    {
                        return;
                    }

                    columnFoldoutRefreshScheduled = true;
                    multiColumnListView.schedule.Execute(() =>
                    {
                        columnFoldoutRefreshScheduled = false;
                        RefreshColumnFoldoutsDisplay(multiColumnListView);
                    });
                }

                void ApplySessionColumnWidth(Column column)
                {
                    float percent = TableRenderer.GetSessionColumnWidth(arrayProp, column.name);
                    if (float.IsNaN(percent))
                    {
                        return;
                    }

                    column.stretchable = false;
                    column.width = Length.Percent(percent);
                }

#if UNITY_6000_0_OR_NEWER
                void SaveSessionColumnWidth(Column column, BindablePropertyChangedEventArgs args)
                {
                    if (args.propertyName != nameof(Column.width))
                    {
                        return;
                    }

                    multiColumnListView.schedule.Execute(() =>
                    {
                        float tableWidth = multiColumnListView.resolvedStyle.width;
                        if (float.IsNaN(tableWidth) || tableWidth <= 0f)
                        {
                            return;
                        }

                        float percent = column.width.unit == LengthUnit.Percent
                            ? column.width.value
                            : column.width.value / tableWidth * 100f;
                        TableRenderer.SaveSessionColumnWidth(arrayProp, column.name, percent);
                    });
                }
#endif

                TableCellFoldableElement MakeTableCellFoldableElement()
                {
                    TableCellFoldableElement itemContainer = new TableCellFoldableElement();
                    itemContainer.RegisterValueChangedCallback(expand =>
                    {
                        if (!(itemContainer.userData is RowData currentRowData))
                        {
                            return;
                        }
                        foreach (TableCellFoldableElement rowCell in multiColumnListView.Query<TableCellFoldableElement>().Build())
                        {
                            if (rowCell.userData is RowData checkRowData &&
                                ReferenceEquals(checkRowData.Owner, currentRowData.Owner) &&
                                checkRowData.RowIndex == currentRowData.RowIndex)
                            {
                                rowCell.SetValueWithoutNotify(expand.newValue);
                            }
                        }
                    });

                    itemContainer.RegisterCallback<AttachToPanelEvent>(_ =>
                    {
                        UIToolkitUtils.LoopCheckOutOfScoopFoldout(itemContainer.contentContainer);
                        ScheduleColumnFoldoutRefreshDisplay();
                    });
                    itemContainer.RegisterCallback<GeometryChangedEvent>(_ => ScheduleColumnFoldoutRefreshDisplay());

                    return itemContainer;
                }
            }

            if (_preArraySize != newArraySize)
            {
                _preArraySize = newArraySize;
                // MultiColumnListView multiColumnListView = container.Q<MultiColumnListView>();
                RefreshSearchingStatus();
                List<int> source = new List<int>(_asyncSearchItems.ItemIndexToPropertyIndex);
                // Debug.Log($"Refresh set source to {string.Join(", ", source.Select(each => $"{each.propertyPath}/{each.propertyType}"))}");
                _multiColumnListView.itemsSource = source;
                _multiColumnListView.Rebuild();
                _multiColumnListView.schedule.Execute(() => RefreshColumnFoldoutsDisplay(_multiColumnListView));
                // _multiColumnListView.Rebuild();
                // _multiColumnListView.schedule.Execute(() =>
                // {
                //     var source = MakeSource(arrayProp);
                //     Debug.Log(
                //         $"Refresh set source to {string.Join(", ", source.Select(each => $"{each.propertyPath}/{each.propertyType}"))}");
                //     _multiColumnListView.itemsSource = source;
                // }).StartingIn(500);
            }
            RefreshSearchingStatus();
        }

        private static void RefreshColumnFoldoutsDisplay(MultiColumnListView multiColumnListView)
        {
            Dictionary<int, TableCellFoldableElement> rowToFirstCell = new Dictionary<int, TableCellFoldableElement>();
            Dictionary<int, float> rowToFirstX = new Dictionary<int, float>();

            foreach (TableCellFoldableElement cell in multiColumnListView.Query<TableCellFoldableElement>().Build())
            {
                cell.ToggleDisplay(false);

                if (cell.userData is null)
                {
                    continue;
                }

                RowData rowData = (RowData)cell.userData;

                int rowIndex = rowData.RowIndex;
                Rect worldBound = cell.worldBound;
                if (cell.panel == null || cell.resolvedStyle.display == DisplayStyle.None || worldBound.width <= 0)
                {
                    continue;
                }

                if (rowToFirstX.TryGetValue(rowIndex, out float firstX) && !(worldBound.x < firstX))
                {
                    continue;
                }

                rowToFirstX[rowIndex] = worldBound.x;
                rowToFirstCell[rowIndex] = cell;
            }

            foreach (TableCellFoldableElement firstCell in rowToFirstCell.Values)
            {
                firstCell.ToggleDisplay(true);
            }
        }

        private static void ClearNestedSelectionAndFocusOutsideSelection(MultiColumnListView multiColumnListView)
        {
            foreach (ListView nestedListView in multiColumnListView.Query<ListView>().Build())
            {
                if (!TryGetRowIndex(multiColumnListView, nestedListView, out int nestedRowIndex))
                {
                    continue;
                }

                if (multiColumnListView.selectedIndices.Contains(nestedRowIndex))
                {
                    continue;
                }

                nestedListView.ClearSelection();
            }

            foreach (MultiColumnListView nestedMultiColumnListView in multiColumnListView.Query<MultiColumnListView>().Build()
                         .Where(each => !ReferenceEquals(each, multiColumnListView)))
            {
                if (!TryGetRowIndex(multiColumnListView, nestedMultiColumnListView, out int nestedRowIndex))
                {
                    continue;
                }

                if (multiColumnListView.selectedIndices.Contains(nestedRowIndex))
                {
                    continue;
                }

                nestedMultiColumnListView.ClearSelection();
            }

            Focusable focused = multiColumnListView.panel?.focusController?.focusedElement;
            if (!(focused is VisualElement focusedElement) || !multiColumnListView.Contains(focusedElement))
            {
                return;
            }

            if (!TryGetRowIndex(multiColumnListView, focusedElement, out int focusedRowIndex))
            {
                return;
            }

            if (multiColumnListView.selectedIndices.Contains(focusedRowIndex))
            {
                return;
            }

            focusedElement.Blur();
            multiColumnListView.Focus();
        }

        private static bool TryGetRowIndex(VisualElement owner, VisualElement element, out int rowIndex)
        {
            for (VisualElement current = element; current != null; current = current.parent)
            {
                if (current.userData is RowData rowData && ReferenceEquals(rowData.Owner, owner))
                {
                    rowIndex = rowData.RowIndex;
                    return true;
                }
            }

            rowIndex = -1;
            return false;
        }

        private readonly struct RowData
        {
            public readonly VisualElement Owner;
            public readonly int RowIndex;

            public RowData(VisualElement owner, int rowIndex)
            {
                Owner = owner;
                RowIndex = rowIndex;
            }
        }

        private readonly struct SaintsFieldInfoName
        {
            public readonly SaintsFieldWithInfo SaintsFieldWithInfo;
            public readonly string FriendlyName;

            public SaintsFieldInfoName(SaintsFieldWithInfo saintsFieldWithInfo, string friendlyName)
            {
                SaintsFieldWithInfo = saintsFieldWithInfo;
                FriendlyName = friendlyName;
            }
        }

        private static bool HeaderDefaultHide(string value, ICollection<string> valueTableHeaders, bool headerIsHide)
        {
            bool inHeader = valueTableHeaders.Contains(value);
            if (headerIsHide)
            {
                return inHeader;
            }
            return !inHeader;
        }

        public bool HasListView() => _multiColumnListView != null;

        public void Refresh() => OnArrayPropertyChanged();

        public void CollapseAll()
        {
            ToggleFoldableAll(false);
        }

        public void ExpandAll()
        {
            ToggleFoldableAll(true);
        }

        private void ToggleFoldableAll(bool expand)
        {
            foreach (TableCellFoldableElement cell in _multiColumnListView.Query<TableCellFoldableElement>().Build())
            {
                cell.value = expand;
            }
        }

        #region Search

        public bool SearchableAll { get; private set; }
        public bool SearchableCols { get; private set; }
        public bool DefaultSearch { get; set; } = true;
        public bool ObjectNestedSearch { get; set; } = true;
        public bool ExtraSearch { get; set; }
        public bool HasExtraSearch => _customSearch != null;
        public int CurPageIndex => _asyncSearchItems.CurPageIndex;
        public event Action<int, int> PageChanged;

        private ToolbarSearchField _searchField;
        private readonly Dictionary<string, SearchColumn> _searchColumns = new Dictionary<string, SearchColumn>();
        private Func<int, IReadOnlyList<ListSearchToken>, bool> _customSearch;
        private AsyncSearchItems _asyncSearchItems;
        private int _numberOfItemsPerPage;

        private class SearchColumn
        {
            public string[] PropertyNames;
            public string SearchText = "";
        }

        // Keep the search state and paging model aligned with SerializedListElement.
        private class AsyncSearchItems
        {
            public bool Started;
            public bool Finished;
            public IEnumerator<IReadOnlyList<int>> SourceGenerator;
            public List<int> FullSources;
            public double DebounceSearchTime;
            public List<int> CachedFullSources;
            public List<int> ItemIndexToPropertyIndex;
            public int CurPageIndex;
        }

        private struct PagingInfo
        {
            public List<int> IndexesCurPage;
            public int CurPageIndex;
            public int PageCount;
        }

        private void InitializeSearch(TableAttribute settings)
        {
            SerializedProperty property = _fieldWithInfo.SerializedProperty;
            SearchableAll = settings.SearchableAll;
            SearchableCols = settings.SearchableCols;
            _numberOfItemsPerPage = Mathf.Max(0, settings.NumberOfItemsPerPage);
            _customSearch = CreateExtraSearch(property,
                ReflectUtils.GetElementType(_fieldWithInfo.FieldInfo?.FieldType ?? _fieldWithInfo.PropertyInfo.PropertyType),
                _fieldWithInfo.Targets[0], RuntimeUtil.ParseCallback(settings.ExtraSearch).content);
            ExtraSearch = HasExtraSearch;
            List<int> fullList = Enumerable.Range(0, property.arraySize).ToList();
            _asyncSearchItems = new AsyncSearchItems
            {
                Started = true,
                Finished = true,
                FullSources = fullList,
                CachedFullSources = new List<int>(fullList),
                ItemIndexToPropertyIndex = new List<int>(fullList),
                DebounceSearchTime = double.MaxValue,
            };

            _searchField = MakeSearchField("", SearchableAll, _ =>
            {
                _asyncSearchItems.CurPageIndex = 0;
                RefreshSearchingStatus(true);
            });
            Add(_searchField);
            RegisterCallback<AttachToPanelEvent>(_ => RefreshSearchingStatus());
            RegisterCallback<DetachFromPanelEvent>(_ => StopSearch());
            schedule.Execute(AdvanceSearch).Every(1);
        }

        private ToolbarSearchField MakeSearchField(string text, bool searchable, Action<string> changed)
        {
            ToolbarSearchField searchField = new ToolbarSearchField
            {
                style =
                {
                    display = searchable ? DisplayStyle.Flex : DisplayStyle.None,
                    marginRight = 3,
                    width = StyleKeyword.Auto,
                    minWidth = 0,
                    flexGrow = 1,
                    flexShrink = 1,
                },
            };
            searchField.SetValueWithoutNotify(text);
            searchField.RegisterValueChangedCallback(evt => changed(evt.newValue));
            searchField.RegisterCallback<KeyDownEvent>(evt =>
            {
                if (evt.keyCode == KeyCode.Return && !_asyncSearchItems.Started)
                {
                    _asyncSearchItems.DebounceSearchTime = 0;
                }
            }, TrickleDown.TrickleDown);

            TextField searchTextField = searchField.Q<TextField>();
            searchTextField.style.position = Position.Relative;
            Image loadingImage = new Image
            {
                name = "saints-table-search-loading",
                image = Util.LoadResource<Texture2D>("refresh.png"),
                pickingMode = PickingMode.Ignore,
                tintColor = EColor.Gray.GetColor(),
                style =
                {
                    position = Position.Absolute,
                    right = 0,
                    top = 1,
                    width = 12,
                    height = 12,
                    visibility = Visibility.Hidden,
                },
            };
            searchTextField.Add(loadingImage);
            UIToolkitUtils.SetKeepRotate(loadingImage);
            loadingImage.schedule.Execute(() => UIToolkitUtils.TriggerRotate(loadingImage));
            return searchField;
        }

        private void RegisterSearchColumn(string id, IEnumerable<string> propertyNames)
        {
            if (!_searchColumns.TryGetValue(id, out SearchColumn column))
            {
                _searchColumns[id] = column = new SearchColumn();
            }
            column.PropertyNames = propertyNames.ToArray();
        }

        private VisualElement MakeSearchHeader(string id, string columnName)
        {
            VisualElement headerContainer = new VisualElement();
            headerContainer.Add(new Label(columnName)
            {
                style = { marginLeft = 3, marginRight = 3 },
            });
            SearchColumn column = _searchColumns[id];
            ToolbarSearchField searchField = MakeSearchField(column.SearchText, SearchableCols, text =>
            {
                column.SearchText = text;
                _asyncSearchItems.CurPageIndex = 0;
                RefreshSearchingStatus(true);
            });
            searchField.SetEnabled(column.PropertyNames.Length > 0);
            headerContainer.Add(searchField);
            return headerContainer;
        }

        public void SetSearchableAll(bool searchable) => SetSearchable(searchable, SearchableCols);

        public void SetSearchableCols(bool searchable) => SetSearchable(SearchableAll, searchable);

        private void SetSearchable(bool searchableAll, bool searchableCols)
        {
            SearchableAll = searchableAll;
            SearchableCols = searchableCols;
            foreach (SearchColumn column in _searchColumns.Values)
            {
                if (!searchableCols)
                {
                    column.SearchText = "";
                }
            }
            foreach (ToolbarSearchField field in this.Query<ToolbarSearchField>().Build())
            {
                // Nested collections have their own search settings.
                if (!ReferenceEquals(field, _searchField) && field.GetFirstAncestorOfType<TableCellFoldableElement>() != null)
                {
                    continue;
                }
                bool searchable = ReferenceEquals(field, _searchField) ? searchableAll : searchableCols;
                field.style.display = searchable ? DisplayStyle.Flex : DisplayStyle.None;
                if (!searchable)
                {
                    field.SetValueWithoutNotify("");
                }
            }
            _asyncSearchItems.CurPageIndex = 0;
            RefreshSearchingStatus();
        }

        public void SetPage(int pageIndex, int numberOfItemsPerPage)
        {
            _numberOfItemsPerPage = Mathf.Max(0, numberOfItemsPerPage);
            LocalUpdatePage(pageIndex);
        }

        public void RefreshSearchingStatus(bool debounce = false)
        {
            StopSearch();
            SerializedProperty property = _fieldWithInfo.SerializedProperty;
            if (!SerializedUtils.IsOk(property))
            {
                return;
            }
            bool hasSearch = (SearchableAll && !string.IsNullOrWhiteSpace(_searchField.value)) ||
                (SearchableCols && _searchColumns.Values.Any(each => !string.IsNullOrWhiteSpace(each.SearchText)));
            if (!hasSearch)
            {
                List<int> resultIndexes = Enumerable.Range(0, property.arraySize).ToList();
                _asyncSearchItems.FullSources = resultIndexes;
                _asyncSearchItems.CachedFullSources = new List<int>(resultIndexes);
            }
            else
            {
                _asyncSearchItems.DebounceSearchTime = debounce ? EditorApplication.timeSinceStartup + 0.6f : 0;
                _asyncSearchItems.Started = false;
                _asyncSearchItems.Finished = false;
                _asyncSearchItems.FullSources.Clear();
                _asyncSearchItems.CachedFullSources.RemoveAll(each => each >= property.arraySize);
                _asyncSearchItems.SourceGenerator = SearchCallbackWithCustom(property).GetEnumerator();
            }
            LocalUpdatePage(_asyncSearchItems.CurPageIndex);
        }

        private IEnumerable<IReadOnlyList<int>> SearchCallbackWithCustom(SerializedProperty arrayProperty)
        {
            const int batchLimit = 10;
            List<int> batch = new List<int>();
            IReadOnlyList<ListSearchToken> tokens = SerializedUtils.ParseSearch(_searchField.value).ToArray();
            bool globalSearch = SearchableAll && !string.IsNullOrWhiteSpace(_searchField.value);
            (string[] PropertyNames, ListSearchToken[] Tokens)[] columns = _searchColumns.Values.Where(each => SearchableCols && !string.IsNullOrWhiteSpace(each.SearchText))
                .Select(each => (each.PropertyNames, Tokens: SerializedUtils.ParseSearch(each.SearchText).ToArray())).ToArray();
            for (int index = 0; index < arrayProperty.arraySize; index++)
            {
                SerializedProperty item = arrayProperty.GetArrayElementAtIndex(index);
                bool matches = !globalSearch || (ExtraSearch && _customSearch != null && _customSearch(index, tokens)) ||
                    (DefaultSearch && tokens.All(token =>
                        SerializedUtils.SearchProp(item, token.Token, ObjectNestedSearch, new HashSet<object>())));
                if (matches && columns.Length > 0)
                {
                    SerializedObject referencedObject = item.propertyType == SerializedPropertyType.ObjectReference && item.objectReferenceValue
                        ? new SerializedObject(item.objectReferenceValue)
                        : null;
                    try
                    {
                        foreach ((string[] PropertyNames, ListSearchToken[] Tokens) column in columns)
                        {
                            bool columnMatches = DefaultSearch && column.Tokens.All(token => column.PropertyNames.Any(propName =>
                            {
                                SerializedProperty member = item.propertyType == SerializedPropertyType.ObjectReference
                                    ? referencedObject?.FindProperty(propName)
                                    : item.FindPropertyRelative(propName);
                                return member != null && SerializedUtils.SearchProp(member, token.Token, ObjectNestedSearch, new HashSet<object>());
                            }));
                            if (!columnMatches && !(ExtraSearch && _customSearch != null && _customSearch(index, column.Tokens)))
                            {
                                matches = false;
                                break;
                            }
                        }
                    }
                    finally
                    {
                        referencedObject?.Dispose();
                    }
                }
                if (matches)
                {
                    batch.Add(index);
                }
                // Yield even when no rows match, so a large scan never runs in one editor update.
                if ((index + 1) % batchLimit == 0)
                {
                    yield return batch.ToArray();
                    batch.Clear();
                }
            }
            if (batch.Count > 0)
            {
                yield return batch;
            }
        }

        private void AdvanceSearch()
        {
            if (!SerializedUtils.IsOk(_fieldWithInfo.SerializedProperty))
            {
                StopSearch();
                return;
            }
            if (!_asyncSearchItems.Started && EditorApplication.timeSinceStartup > _asyncSearchItems.DebounceSearchTime)
            {
                _asyncSearchItems.Started = true;
                LocalUpdatePage(_asyncSearchItems.CurPageIndex);
            }
            if (_asyncSearchItems.Started && !_asyncSearchItems.Finished)
            {
                if (_asyncSearchItems.SourceGenerator.MoveNext())
                {
                    // ReSharper disable once AssignNullToNotNullAttribute
                    _asyncSearchItems.FullSources.AddRange(_asyncSearchItems.SourceGenerator.Current);
                }
                else
                {
                    _asyncSearchItems.Finished = true;
                    _asyncSearchItems.SourceGenerator.Dispose();
                    _asyncSearchItems.SourceGenerator = null;
                }
                _asyncSearchItems.CachedFullSources = new List<int>(_asyncSearchItems.FullSources);
                LocalUpdatePage(_asyncSearchItems.CurPageIndex);
            }
            foreach (Image loadingImage in this.Query<Image>(name: "saints-table-search-loading").Build())
            {
                loadingImage.style.visibility = _asyncSearchItems.Started && !_asyncSearchItems.Finished
                    ? Visibility.Visible : Visibility.Hidden;
            }
        }

        private void LocalUpdatePage(int newPageIndex)
        {
            IReadOnlyList<int> resultIndexes = _asyncSearchItems.Started
                ? _asyncSearchItems.FullSources : _asyncSearchItems.CachedFullSources;
            PagingInfo pagingInfo = GetPagingInfo(newPageIndex, resultIndexes, _numberOfItemsPerPage);
            _asyncSearchItems.ItemIndexToPropertyIndex = new List<int>(pagingInfo.IndexesCurPage);
            _asyncSearchItems.CurPageIndex = pagingInfo.CurPageIndex;
            if (_multiColumnListView != null && !_multiColumnListView.itemsSource.Cast<int>().SequenceEqual(pagingInfo.IndexesCurPage))
            {
                _multiColumnListView.ClearSelection();
                _multiColumnListView.itemsSource = pagingInfo.IndexesCurPage;
                _multiColumnListView.Rebuild();
            }
            PageChanged?.Invoke(pagingInfo.CurPageIndex, pagingInfo.PageCount);
        }

        private static PagingInfo GetPagingInfo(int newPageIndex, IReadOnlyList<int> fullIndexResults, int numberOfItemsPerPage)
        {
            int pageCount = numberOfItemsPerPage <= 0 ? 1 : Mathf.Max(1, Mathf.CeilToInt((float)fullIndexResults.Count / numberOfItemsPerPage));
            int curPageIndex = numberOfItemsPerPage <= 0 ? 0 : Mathf.Clamp(newPageIndex, 0, pageCount - 1);
            return new PagingInfo
            {
                IndexesCurPage = fullIndexResults.Skip(numberOfItemsPerPage <= 0 ? 0 : curPageIndex * numberOfItemsPerPage)
                    .Take(numberOfItemsPerPage <= 0 ? int.MaxValue : numberOfItemsPerPage).ToList(),
                CurPageIndex = curPageIndex,
                PageCount = pageCount,
            };
        }

        public void StopSearch()
        {
            _asyncSearchItems.SourceGenerator?.Dispose();
            _asyncSearchItems.SourceGenerator = null;
            _asyncSearchItems.Started = true;
            _asyncSearchItems.Finished = true;
        }

        private static Func<int, IReadOnlyList<ListSearchToken>, bool> CreateExtraSearch(
            SerializedProperty property, Type elementType, object callbackOwner, string methodName)
        {
            if (callbackOwner == null || string.IsNullOrEmpty(methodName))
            {
                return null;
            }
            foreach (Type type in ReflectUtils.GetSelfAndBaseTypesFromType(callbackOwner.GetType()))
            {
                foreach (MethodInfo method in type.GetMethods(ReflectUtils.FindTargetBindAttr))
                {
                    if (method.Name != methodName || method.ReturnType != typeof(bool))
                    {
                        continue;
                    }
                    ParameterInfo[] parameters = method.GetParameters();
                    if (parameters.Length < 2 || parameters.Length > 3 ||
                        !typeof(IEnumerable<ListSearchToken>).IsAssignableFrom(parameters[parameters.Length - 1].ParameterType))
                    {
                        continue;
                    }
                    bool valueAndIndex = parameters.Length == 3 && elementType.IsAssignableFrom(parameters[0].ParameterType) &&
                        parameters[1].ParameterType == typeof(int);
                    bool valueOnly = parameters.Length == 2 && elementType.IsAssignableFrom(parameters[0].ParameterType);
                    bool indexOnly = parameters.Length == 2 && !valueOnly && parameters[0].ParameterType == typeof(int);
                    if (!valueAndIndex && !valueOnly && !indexOnly)
                    {
                        continue;
                    }
                    return (index, tokens) =>
                    {
                        if (indexOnly)
                        {
                            return (bool)method.Invoke(callbackOwner, new object[] { index, tokens });
                        }
                        SerializedProperty item = property.GetArrayElementAtIndex(index);
                        (SerializedUtils.FieldOrProp member, object owner) = SerializedUtils.GetFieldInfoAndDirectParent(item);
                        MemberInfo memberInfo = member.IsField ? member.FieldInfo : member.PropertyInfo;
                        (string error, int _, object value) = Util.GetValue(item, memberInfo, owner);
                        return error == "" && (bool)method.Invoke(callbackOwner, valueAndIndex
                            ? new[] { value, index, tokens } : new[] { value, tokens });
                    };
                }
            }
            return null;
        }

        #endregion

    }
}
#endif
