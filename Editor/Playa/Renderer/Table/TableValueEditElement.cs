#if UNITY_2021_3_OR_NEWER
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using SaintsField.Editor.Playa.Renderer.BaseRenderer;
using SaintsField.Editor.UIToolkitElements;
using SaintsField.Editor.Utils;
using SaintsField.Playa;
using SaintsField.Utils;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace SaintsField.Editor.Playa.Renderer.Table
{
    public class TableValueEditElement : VisualElement
    {
        private class ColumnData
        {
            public string Title;
            public readonly List<SaintsFieldWithInfo> Members = new List<SaintsFieldWithInfo>();
            public string Search = "";
            public bool Hidden;
        }

        private class CellData
        {
            public int Index;
            public readonly List<AbsRenderer> Renderers = new List<AbsRenderer>();
            public readonly List<Action> Updates = new List<Action>();
        }

        // ButtonRenderer uses the parent setter to write back methods invoked on boxed structs.
        private class RowTarget
        {
            private readonly TableValueEditElement _table;
            private readonly int _index;

            public RowTarget(TableValueEditElement table, int index)
            {
                _table = table;
                _index = index;
            }

            public object Value
            {
                get => _table._values[_index];
                set
                {
                    _table._beforeSet?.Invoke(_table._rawValue);
                    _table.SetItem(_index, value);
                }
            }
        }

        private readonly CollectionFoldout _foldout;
        private readonly MultiColumnListView _listView;
        private readonly ListViewPagerElement _pager;
        private readonly ListViewFooterButtonsElement _buttons;
        private readonly Label _emptyNotice;
        private readonly ToolbarSearchField _searchField;
        private readonly TableAttribute _settings;
        private readonly IReadOnlyList<Attribute> _attributes;
        private readonly IRichTextTagProvider _richTextTagProvider;
        private readonly string _viewKey;
        private readonly Type _elementType;
        private readonly Type _creationType;
        private readonly List<ColumnData> _columns = new List<ColumnData>();
        private readonly List<int> _filteredIndices = new List<int>();
        private List<int> _pageIndices = new List<int>();
        private object _rawValue;
        private object[] _values;
        private Action<object> _beforeSet;
        private Action<object> _setter;
        private IReadOnlyList<object> _targets;
        private Type _rowType;
        private int _page;
        private bool _searchableAll;
        private bool _searchableCols;
        private bool _objectSearch = true;
        private bool _defaultSearch = true;
        private bool _extraSearch;
        private readonly Func<int, IReadOnlyList<ListSearchToken>, bool> _customSearch;
        private IEnumerator<int> _search;
        private double _searchTime;

        public TableValueEditElement(string label, Type valueType, object rawValue, object[] values,
            Action<object> beforeSet, Action<object> setter, bool labelGrayColor,
            IReadOnlyList<Attribute> attributes, IReadOnlyList<object> targets,
            IRichTextTagProvider richTextTagProvider, string viewKey)
        {
            _settings = attributes.OfType<TableAttribute>().First();
            _attributes = attributes;
            _richTextTagProvider = richTextTagProvider;
            _viewKey = viewKey;
            valueType ??= rawValue.GetType();
            _elementType = rawValue is Array array
                ? array.GetType().GetElementType()
                : ReflectUtils.GetSelfAndBaseTypesFromType(valueType)
                      .Select(each => (Type: each, Element: ReflectUtils.GetElementType(each)))
                      .Where(each => each.Type != each.Element).Select(each => each.Element).FirstOrDefault() ??
                  typeof(object);
            Type listType = typeof(List<>).MakeGenericType(_elementType);

            if (valueType.IsArray)
            {
                _creationType = valueType;
            }
            else if (valueType.IsAssignableFrom(listType))
            {
                _creationType = listType;
            }
            else if (typeof(IList).IsAssignableFrom(valueType) && !valueType.IsAbstract &&
                     valueType.GetConstructor(Type.EmptyTypes) != null)
            {
                _creationType = valueType;
            }
            else
            {
                _creationType = null;
            }

            _rawValue = rawValue;
            _values = values;
            _beforeSet = beforeSet;
            _setter = setter;
            _targets = targets;
            _searchableAll = _settings.SearchableAll;
            _searchableCols = _settings.SearchableCols;
            _customSearch = CreateExtraSearch();
            _extraSearch = _customSearch != null;

            Add(_foldout = new CollectionFoldout(label) { viewDataKey = viewKey });
            _foldout.contentContainer.style.marginLeft = 0;
            if (labelGrayColor)
            {
                _foldout.style.color = AbsRenderer.ReColor;
            }

            _foldout.Add(_searchField = new ToolbarSearchField
            {
                style =
                {
                    marginRight = 3,
                    width = StyleKeyword.Auto,
                    minWidth = 0,
                    flexGrow = 1,
                    flexShrink = 1,
                },
            });
            _searchField.RegisterValueChangedCallback(_ => RefreshSearch(true));
            _foldout.Add(_emptyNotice = new Label("Table is empty"));
            _foldout.Add(_listView = new MultiColumnListView
            {
                virtualizationMethod = CollectionVirtualizationMethod.DynamicHeight,
                selectionType = SelectionType.Multiple,
                reorderMode = ListViewReorderMode.Animated,
                showBorder = true,
                viewDataKey = viewKey + ".rows",
                itemsSource = new List<int>(),
            });
            _listView.itemIndexChanged += (from, to) =>
            {
                IList list = (IList)_rawValue;
                int fromIndex = _pageIndices[from];
                int toIndex = _pageIndices[to];
                _beforeSet?.Invoke(_rawValue);
                object item = list[fromIndex];
                int direction = fromIndex < toIndex ? 1 : -1;
                for (int index = fromIndex; index != toIndex; index += direction)
                {
                    list[index] = list[index + direction];
                }

                list[toIndex] = item;
                Commit();
            };

            VisualElement footer = new VisualElement
            {
                style =
                {
                    flexDirection = FlexDirection.Row,
                    justifyContent = Justify.FlexEnd,
                },
            };
            _foldout.Add(footer);

            _pager = new ListViewPagerElement
            {
                style =
                {
                    display = _settings.NumberOfItemsPerPage > 0 ? DisplayStyle.Flex : DisplayStyle.None,
                },
            };
            footer.Add(_pager);
            footer.Add(_buttons = new ListViewFooterButtonsElement());

            _pager.NumberOfItemsPerPageField.SetValueWithoutNotify(Mathf.Max(0, _settings.NumberOfItemsPerPage));
            _pager.PagePreButton.clicked += () => SetPage(_page - 1);
            _pager.PageNextButton.clicked += () => SetPage(_page + 1);
            _pager.PageField.RegisterValueChangedCallback(evt => SetPage(evt.newValue - 1));
            _pager.NumberOfItemsPerPageField.RegisterValueChangedCallback(evt =>
            {
                _pager.NumberOfItemsPerPageField.SetValueWithoutNotify(Mathf.Max(0, evt.newValue));
                SetPage(_page);
            });
            _foldout.ArraySizeField.RegisterValueChangedCallback(evt => Resize(evt.newValue));
            _pager.NumberOfItemsTotalField.RegisterValueChangedCallback(evt => Resize(evt.newValue));
            _buttons.AddButton.clicked += () =>
            {
                Resize(_values.Length + 1);
                SetPage(int.MaxValue);
            };
            _buttons.RemoveButton.clicked += () =>
            {
                int[] indices = _listView.selectedIndices.Select(each => (int)_listView.itemsSource[each])
                    .OrderByDescending(each => each).ToArray();
                if (indices.Length == 0 && _values.Length > 0)
                {
                    indices = new[] { _values.Length - 1 };
                }

                Remove(indices);
            };
            _foldout.MenuButton.clicked += ShowMenu;
            BuildColumns();
            RefreshControls();
            _searchField.style.display = _searchableAll ? DisplayStyle.Flex : DisplayStyle.None;
            RefreshSearch();
            schedule.Execute(AdvanceSearch).Every(1);
            RegisterCallback<DetachFromPanelEvent>(_ => StopSearch());
            RegisterCallback<AttachToPanelEvent>(_ => RefreshSearch());
        }

        public void Refresh(string label, object rawValue, object[] values, Action<object> beforeSet,
            Action<object> setter, IReadOnlyList<object> targets)
        {
            _foldout.SetText(label);
            bool changed = !ReferenceEquals(_rawValue, rawValue) || !_values.SequenceEqual(values);
            _rawValue = rawValue;
            _values = values;
            _beforeSet = beforeSet;
            _setter = setter;
            _targets = targets;
            RefreshControls();
            Type rowType = values.FirstOrDefault(each => !RuntimeUtil.IsNull(each))?.GetType();
            if (rowType != _rowType)
            {
                BuildColumns();
                changed = true;
            }

            // ReSharper disable once InvertIf
            if (changed)
            {
                RefreshSearch();
                _listView.RefreshItems();
            }
        }

        private bool CanEdit() => _rawValue is IList { IsReadOnly: false };

        private bool CanResize()
        {
            if (_rawValue == null)
                return _setter != null && _creationType != null;
            if (!CanEdit())
            {
                return false;
            }

            if (_rawValue is Array)
            {
                return _setter != null;
            }

            return !((IList)_rawValue).IsFixedSize;
        }

        private void RefreshControls()
        {
            _foldout.ArraySizeField.SetValueWithoutNotify(_values.Length);
            _pager.NumberOfItemsTotalField.SetValueWithoutNotify(_values.Length);
            _foldout.ArraySizeField.SetEnabled(CanResize() && !(_settings.HideAddButton && _settings.HideRemoveButton));
            _pager.NumberOfItemsTotalField.SetEnabled(_foldout.ArraySizeField.enabledSelf);
            _buttons.AddButton.SetEnabled(CanResize());
            _buttons.RemoveButton.SetEnabled(CanResize());
            _buttons.AddButton.style.display = _settings.HideAddButton ? DisplayStyle.None : DisplayStyle.Flex;
            _buttons.RemoveButton.style.display = _settings.HideRemoveButton ? DisplayStyle.None : DisplayStyle.Flex;
            _listView.reorderable = CanEdit();
            _emptyNotice.style.display = _values.Length == 0 ? DisplayStyle.Flex : DisplayStyle.None;
            _listView.style.display = _values.Length == 0 ? DisplayStyle.None : DisplayStyle.Flex;
        }

        private void BuildColumns()
        {
            _rowType = _values.FirstOrDefault(each => !RuntimeUtil.IsNull(each))?.GetType();
            _columns.Clear();
            _listView.columns.Clear();
            object sample = _values.FirstOrDefault(each => !RuntimeUtil.IsNull(each));
            // ReSharper disable once PossibleNullReferenceException
            if (sample != null && !_rowType.IsPrimitive && !_rowType.IsEnum && _rowType != typeof(string))
            {
                foreach (SaintsFieldWithInfo info in GetMembers(sample))
                {
                    string title = info.PlayaAttributes.OfType<TableColumnAttribute>().FirstOrDefault()?.Title ??
                                   AbsRenderer.GetFriendlyName(info);
                    ColumnData column = _columns.FirstOrDefault(each => each.Title == title);
                    if (column == null)
                    {
                        _columns.Add(column = new ColumnData { Title = title });
                    }

                    column.Members.Add(info);
                    column.Hidden |= info.PlayaAttributes.Any(each => each is TableHideAttribute);
                }
            }

            if (_columns.Count == 0)
            {
                _columns.Add(new ColumnData
                {
                    Title = "Value",
                });
            }

            TableHeadersAttribute headers = _attributes.OfType<TableHeadersAttribute>().FirstOrDefault();
            HashSet<string> hiddenHeaders = new HashSet<string>();
            if (headers != null)
            {
                foreach (TableHeadersAttribute.Header header in headers.Headers)
                {
                    object names = header.Name;
                    if (header.IsCallback && _targets.Count > 0)
                    {
                        (string error, object value) = Util.GetOfNoParams<object>(_targets[0], header.Name, null);
                        names = error == "" ? value : null;
                    }

                    IEnumerable<string> titles = names is string s ? new[] { s } :
                        names is IEnumerable enumerable ? enumerable.Cast<object>().Where(each => each != null)
                            .Select(each => each.ToString()) :
                        Array.Empty<string>();
                    hiddenHeaders.UnionWith(titles.SelectMany(each =>
                        new[] { each, ObjectNames.NicifyVariableName(each) }));
                }
            }

            foreach (ColumnData data in _columns)
            {
                Column column = new Column
                {
                    name = data.Title,
                    title = data.Title,
                    stretchable = true,
                    visible = !data.Hidden && (headers == null || (headers.IsHide
                        ? !hiddenHeaders.Contains(data.Title)
                        : hiddenHeaders.Contains(data.Title))),
                    makeHeader = () =>
                    {
                        VisualElement header = new VisualElement();
                        header.Add(new Label(data.Title));
                        ToolbarSearchField search = new ToolbarSearchField
                        {
                            value = data.Search,
                            style =
                            {
                                display = _searchableCols ? DisplayStyle.Flex : DisplayStyle.None,
                                marginRight = 3,
                                width = StyleKeyword.Auto,
                                minWidth = 0,
                                flexGrow = 1,
                                flexShrink = 1,
                            },
                        };
                        search.RegisterValueChangedCallback(evt =>
                        {
                            data.Search = evt.newValue;
                            RefreshSearch(true);
                        });
                        header.Add(search);
                        return header;
                    },
                    makeCell = MakeCell,
                    bindCell = (element, index) =>
                        BindCell((TableCellFoldableElement)element, (int)_listView.itemsSource[index], data),
                    unbindCell = (element, _) => DestroyCell((TableCellFoldableElement)element),
                };
                float width = SessionState.GetFloat($"{_viewKey}[{data.Title}:width]", float.NaN);
                if (!float.IsNaN(width) && width > 0)
                {
                    column.width = Length.Percent(width);
                }
#if UNITY_6000_0_OR_NEWER
                column.propertyChanged += (_, args) =>
                {
                    if (args.propertyName == nameof(Column.width))
                    {
                        schedule.Execute(() =>
                        {
                            float tableWidth = _listView.resolvedStyle.width;
                            if (tableWidth > 0 && !float.IsNaN(tableWidth))
                            {
                                float percent = column.width.unit == LengthUnit.Percent
                                    ? column.width.value
                                    : column.width.value / tableWidth * 100f;
                                SessionState.SetFloat($"{_viewKey}[{data.Title}:width]", percent);
                            }
                        });
                    }
                };
#endif
                _listView.columns.Add(column);
            }
        }

        private static IEnumerable<SaintsFieldWithInfo> GetMembers(object value)
        {
            List<SaintsFieldWithInfo> members = SaintsEditor.HelperGetSaintsFieldWithInfo(null,
                    new Dictionary<string, SerializedProperty>(), null, null, -1, new[] { value })
                .Where(TableRenderer.SaintsFieldInfoShouldDraw).ToList();
            const BindingFlags flags = BindingFlags.Instance | BindingFlags.Public;
            foreach (MemberInfo member in value.GetType().GetFields(flags).Cast<MemberInfo>()
                         .Concat(value.GetType().GetProperties(flags)
                             .Where(each => each.CanRead && each.GetIndexParameters().Length == 0)))
            {
                if (members.Any(each => each.FieldInfo == member || each.PropertyInfo == member) ||
                    ReflectCache.GetCustomAttributes<HideInInspector>(member).Any())
                {
                    continue;
                }

                Type type = member is FieldInfo field ? field.FieldType : ((PropertyInfo)member).PropertyType;
                if (AbsRenderer.SkipTypeDrawing(type))
                {
                    continue;
                }

                members.Add(new SaintsFieldWithInfo
                {
                    MemberId = member.Name,
                    FieldInfo = member as FieldInfo,
                    PropertyInfo = member as PropertyInfo,
                    AttributeMemberInfo = member,
                    PlayaAttributes = ReflectCache.GetCustomAttributes(member).OfType<IPlayaAttribute>().ToArray(),
                    Targets = new[] { value },
                    RenderType = member is FieldInfo
                        ? SaintsRenderType.NonSerializedField
                        : SaintsRenderType.NativeProperty,
                });
            }

            return members.Where(each =>
                each.FieldInfo != null || each.PropertyInfo != null || each.MethodInfo != null);
        }

        private TableCellFoldableElement MakeCell()
        {
            TableCellFoldableElement cell = new TableCellFoldableElement();
            cell.schedule.Execute(() =>
            {
                if (cell.userData is CellData data)
                {
                    foreach (Action update in data.Updates)
                    {
                        update();
                    }
                }
            }).Every(100);
            cell.RegisterValueChangedCallback(evt =>
            {
                if (!(cell.userData is CellData data))
                {
                    return;
                }

                foreach (TableCellFoldableElement other in _listView.Query<TableCellFoldableElement>().Build())
                {
                    if (other.userData is CellData row && row.Index == data.Index)
                    {
                        other.SetValueWithoutNotify(evt.newValue);
                    }
                }
            });
            cell.RegisterCallback<GeometryChangedEvent>(_ =>
            {
                if (cell.userData is CellData data)
                {
                    TableCellFoldableElement first = _listView.Query<TableCellFoldableElement>().Build()
                        .Where(each =>
                            each.userData is CellData row && row.Index == data.Index && each.resolvedStyle.width > 0)
                        .OrderBy(each => each.worldBound.x).FirstOrDefault();
                    cell.ToggleDisplay(ReferenceEquals(cell, first));
                }
            });
            return cell;
        }

        private static void DestroyCell(TableCellFoldableElement cell)
        {
            if (cell.userData is CellData data)
            {
                foreach (AbsRenderer renderer in data.Renderers)
                {
                    renderer.OnDestroyUIToolkit();
                }
            }

            cell.Clear();
            cell.userData = null;
        }

        private void BindCell(TableCellFoldableElement cell, int index, ColumnData column)
        {
            DestroyCell(cell);
            object target = _values[index];
            CellData data = new CellData { Index = index };
            cell.userData = data;
            string key = $"{_viewKey}:{index}";
            if (_settings.DefaultCollapse && !SessionState.GetBool(key + ".initialized", false))
            {
                SessionState.SetBool(key, false);
            }

            SessionState.SetBool(key + ".initialized", true);
            cell.SetViewKey(key);
            cell.BindButton(() =>
            {
                if (RuntimeUtil.IsNull(target))
                {
                    return "Null";
                }

                try
                {
                    return column.Members.Count == 0
                        ? target.ToString()
                        : string.Join(", ",
                            column.Members.Where(each => each.MethodInfo == null)
                                .Select(each => ReadMember(target, each)?.ToString()));
                }
                catch (Exception e)
                {
                    return e.InnerException?.Message ?? e.Message;
                }
            });

            if (RuntimeUtil.IsNull(target) || column.Members.Count == 0)
            {
                VisualElement item = UIToolkitEdit.UIToolkitValueEdit(null, "", _elementType, target,
                    _ => _beforeSet?.Invoke(_rawValue), CanEdit() ? newValue => SetItem(index, newValue) : null,
                    false, true, Array.Empty<Attribute>(), _targets, _richTextTagProvider, key + ".value").result;
                if (item != null)
                {
                    cell.Add(item);
                }

                return;
            }

            foreach (SaintsFieldWithInfo info in column.Members)
            {
                if (info.MethodInfo != null)
                {
                    SaintsFieldWithInfo methodInfo = info.RefreshTargets(new[] { target });
                    if (CanEdit())
                    {
                        methodInfo.TargetParent = new RowTarget(this, index);
                        methodInfo.TargetMemberInfo = typeof(RowTarget).GetProperty(nameof(RowTarget.Value));
                        methodInfo.TargetMemberIndex = -1;
                    }

                    foreach (SaintsFieldWithRenderer rendererInfo in SaintsEditor.HelperMakeRenderer(null, methodInfo)
                                 .SelectMany(each => each))
                    {
                        AbsRenderer renderer = rendererInfo.Renderer;
                        if (renderer == null)
                        {
                            continue;
                        }

                        renderer.InDirectHorizontalLayout = renderer.InAnyHorizontalLayout = true;
                        data.Renderers.Add(renderer);
                        VisualElement item = renderer.CreateVisualElement(cell);
                        if (item != null)
                        {
                            cell.Add(item);
                        }
                    }

                    continue;
                }

                MemberInfo member = (MemberInfo)info.FieldInfo ?? info.PropertyInfo;
                VisualElement container = new VisualElement();
                cell.Add(container);

                data.Updates.Add(Update);
                Update();
                continue;

                void Update()
                {
                    try
                    {
                        Type memberType = info.FieldInfo?.FieldType ?? info.PropertyInfo.PropertyType;
                        bool writable = info.FieldInfo != null
                            ? !info.FieldInfo.IsLiteral && !info.FieldInfo.IsInitOnly
                            : info.PropertyInfo.CanWrite;
                        VisualElement oldItem = container.Children().FirstOrDefault();
                        if (oldItem is HelpBox)
                        {
                            oldItem = null;
                        }

                        VisualElement item = UIToolkitEdit.UIToolkitValueEdit(oldItem, column.Members.Count == 1 ? "" : AbsRenderer.GetFriendlyName(info), memberType, ReadMember(target, info), _ => _beforeSet?.Invoke(_rawValue), writable && (!target.GetType().IsValueType || CanEdit())
                                ? newValue =>
                                {
                                    if (info.FieldInfo != null)
                                    {
                                        info.FieldInfo.SetValue(target, newValue);
                                    }
                                    else
                                    {
                                        info.PropertyInfo.SetValue(target, newValue);
                                    }

                                    if (CanEdit())
                                    {
                                        SetItem(index, target);
                                    }
                                }
                                : null, false, true, ReflectCache.GetCustomAttributes(member), new[] { target }, _richTextTagProvider, key + "." + info.MemberId)
                            .result;
                        if (item != null)
                        {
                            container.Clear();
                            container.Add(item);
                        }
                    }
                    catch (Exception e)
                    {
                        container.Clear();
                        container.Add(new HelpBox(e.InnerException?.Message ?? e.Message, HelpBoxMessageType.Error));
                    }
                }
            }
        }

        private static object ReadMember(object target, SaintsFieldWithInfo info) =>
            info.FieldInfo != null ? info.FieldInfo.GetValue(target) : info.PropertyInfo?.GetValue(target);

        private void SetItem(int index, object value)
        {
            ((IList)_rawValue)[index] = value;
            _values[index] = value;
            _setter?.Invoke(_rawValue);
            Type rowType = _values.FirstOrDefault(each => !RuntimeUtil.IsNull(each))?.GetType();
            if (rowType != _rowType)
            {
                BuildColumns();
            }
            RefreshSearch();
        }

        private void Commit()
        {
            _values = ((IEnumerable)_rawValue).Cast<object>().ToArray();
            _setter?.Invoke(_rawValue);
            Type rowType = _values.FirstOrDefault(each => !RuntimeUtil.IsNull(each))?.GetType();
            if (rowType != _rowType)
            {
                BuildColumns();
            }

            RefreshControls();
            RefreshSearch();
        }

        private void Resize(int size)
        {
            size = Mathf.Max(0, size);
            if (!CanResize() || size == _values.Length)
            {
                RefreshControls();
                return;
            }

            if (size < _values.Length)
            {
                Remove(Enumerable.Range(size, _values.Length - size).Reverse().ToArray());
                return;
            }

            _beforeSet?.Invoke(_rawValue);

            // ReSharper disable once ConvertIfStatementToNullCoalescingAssignment
            if (_rawValue == null)
            {
                _rawValue = _creationType.IsArray
                    ? Array.CreateInstance(_elementType, 0)
                    : Activator.CreateInstance(_creationType);
            }

            if (_rawValue is Array array)
            {
                Array result = Array.CreateInstance(_elementType, size);
                Array.Copy(array, result, array.Length);
                _rawValue = result;
            }
            else
            {
                IList list = (IList)_rawValue;
                while (list.Count < size)
                {
                    list.Add(_elementType.IsValueType ? Activator.CreateInstance(_elementType) : null);
                }
            }

            Commit();
        }

        private void Remove(int[] indices)
        {
            if (!CanResize() || indices.Length == 0)
            {
                return;
            }

            _beforeSet?.Invoke(_rawValue);
            if (_rawValue is Array)
            {
                Array result = Array.CreateInstance(_elementType, _values.Length - indices.Length);
                int next = 0;
                for (int index = 0; index < _values.Length; index++)
                {
                    if (!indices.Contains(index))
                    {
                        result.SetValue(_values[index], next++);
                    }
                }

                _rawValue = result;
            }
            else
            {
                foreach (int index in indices)
                {
                    ((IList)_rawValue).RemoveAt(index);
                }
            }

            Commit();
        }

        private void StopSearch()
        {
            _search?.Dispose();
            _search = null;
        }

        private void RefreshSearch(bool debounce = false)
        {
            StopSearch();
            _searchTime = EditorApplication.timeSinceStartup + (debounce ? 0.6 : 0);
            bool hasSearch = (_searchableAll && !string.IsNullOrWhiteSpace(_searchField.value)) ||
                             (_searchableCols && _columns.Any(each => !string.IsNullOrWhiteSpace(each.Search)));
            if (!hasSearch)
            {
                _filteredIndices.Clear();
                _filteredIndices.AddRange(Enumerable.Range(0, _values.Length));
                SetPage(_page);
                return;
            }

            _search = Search().GetEnumerator();
        }

        private IEnumerable<int> Search()
        {
            ListSearchToken[] globalTokens = _searchableAll
                ? SerializedUtils.ParseSearch(_searchField.value).ToArray()
                : Array.Empty<ListSearchToken>();
            (ColumnData Data, ListSearchToken[] Tokens)[] columns = _columns
                .Where(each => _searchableCols && !string.IsNullOrWhiteSpace(each.Search))
                .Select(each => (each, SerializedUtils.ParseSearch(each.Search).ToArray())).ToArray();
            for (int index = 0; index < _values.Length; index++)
            {
                object value = _values[index];

                bool Matches(IReadOnlyList<ListSearchToken> tokens, IEnumerable<object> candidates) =>
                    tokens.Count == 0 ||
                    (_extraSearch && _customSearch != null && _customSearch(index, tokens)) ||
                    (_defaultSearch && tokens.All(token => candidates.Any(candidate =>
                        Util.SearchObject(candidate, token.Token, new HashSet<object>(), _objectSearch))));

                bool match = Matches(globalTokens, new[] { value });
                foreach ((ColumnData data, ListSearchToken[] tokens) in columns)
                {
                    IEnumerable<object> candidates = data.Members.Count == 0
                        ? new[] { value }
                        : data.Members.Where(each => each.MethodInfo == null)
                            .Select(each => ReadSearchMember(value, each));
                    match &= Matches(tokens, candidates);
                }

                // Yield misses too, to bound the work per editor update.
                yield return match ? index : -1;
            }
        }

        private void AdvanceSearch()
        {
            // ReSharper disable once CompareOfFloatsByEqualityOperator
            if (_search == null || (_searchTime != double.MaxValue && EditorApplication.timeSinceStartup < _searchTime))
            {
                return;
            }

            // ReSharper disable once CompareOfFloatsByEqualityOperator
            if (_searchTime != double.MaxValue)
            {
                _filteredIndices.Clear();
                _searchTime = double.MaxValue;
            }

            // Continue an active search even after its debounce timer has elapsed.
            for (int count = 0; count < 10; count++)
            {
                if (!_search.MoveNext())
                {
                    StopSearch();
                    SetPage(_page);
                    return;
                }

                if (_search.Current >= 0)
                {
                    _filteredIndices.Add(_search.Current);
                }
            }

            SetPage(_page);
        }

        private static object ReadSearchMember(object value, SaintsFieldWithInfo info)
        {
            if (RuntimeUtil.IsNull(value))
            {
                return null;
            }

            try
            {
                return ReadMember(value, info);
            }
            catch (Exception)
            {
                return null;
            }
        }

        private void SetPage(int page)
        {
            int perPage = _pager.NumberOfItemsPerPageField.value;
            int pageCount = perPage > 0 ? Mathf.Max(1, Mathf.CeilToInt((float)_filteredIndices.Count / perPage)) : 1;
            _page = Mathf.Clamp(page, 0, pageCount - 1);
            _pager.PageField.SetValueWithoutNotify(_page + 1);
            _pager.PageLabel.text = $" / {pageCount}";
            _pager.PagePreButton.SetEnabled(_page > 0);
            _pager.PageNextButton.SetEnabled(_page < pageCount - 1);
            List<int> indices = perPage > 0
                ? _filteredIndices.Skip(_page * perPage).Take(perPage).ToList()
                : new List<int>(_filteredIndices);
            if (!_listView.itemsSource.Cast<int>().SequenceEqual(indices))
            {
                _pageIndices = indices;
                _listView.itemsSource = new List<int>(indices);
            }
        }

        private Func<int, IReadOnlyList<ListSearchToken>, bool> CreateExtraSearch()
        {
            if (string.IsNullOrEmpty(_settings.ExtraSearch) || _targets.Count == 0)
            {
                return null;
            }

            // ReSharper disable once ForeachCanBeConvertedToQueryUsingAnotherGetEnumerator
            foreach (Type type in ReflectUtils.GetSelfAndBaseTypesFromType(_targets[0].GetType()))
            {
                foreach (MethodInfo method in type.GetMethods(ReflectUtils.FindTargetBindAttr))
                {
                    ParameterInfo[] parameters = method.GetParameters();
                    if (method.Name != _settings.ExtraSearch || method.ReturnType != typeof(bool) ||
                        parameters.Length < 2 || parameters.Length > 3 ||
                        !typeof(IEnumerable<ListSearchToken>).IsAssignableFrom(parameters[^1]
                            .ParameterType))
                    {
                        continue;
                    }

                    bool valueAndIndex = parameters.Length == 3 &&
                                         _elementType.IsAssignableFrom(parameters[0].ParameterType) &&
                                         parameters[1].ParameterType == typeof(int);
                    bool valueOnly = parameters.Length == 2 &&
                                     _elementType.IsAssignableFrom(parameters[0].ParameterType);
                    bool indexOnly = parameters.Length == 2 && !valueOnly && parameters[0].ParameterType == typeof(int);
                    if (valueAndIndex || valueOnly || indexOnly)
                    {

                        return (index, tokens) =>
                        {
                            object[] invokeParams;
                            if (indexOnly)
                            {
                                invokeParams = new object[] { index, tokens };
                            }
                            else
                            {
                                invokeParams = valueAndIndex
                                    ? new[] { _values[index], index, tokens }
                                    : new[] { _values[index], tokens };
                            }

                            return (bool)method.Invoke(_targets[0], invokeParams);
                        };
                    }
                }
            }

            return null;
        }

        private void ShowMenu()
        {
            GenericDropdownMenu menu = new GenericDropdownMenu();
            if (_setter == null)
            {
                menu.AddDisabledItem("Set To Null", false);
            }
            else
            {
                menu.AddItem("Set To Null", false, () =>
                {
                    _beforeSet?.Invoke(_rawValue);
                    _setter(null);
                });
            }

            menu.AddSeparator("");
            bool paging = _pager.style.display != DisplayStyle.None;
            menu.AddItem("Paging", paging, () =>
            {
                _pager.style.display = paging ? DisplayStyle.None : DisplayStyle.Flex;
                _pager.NumberOfItemsPerPageField.value = paging ? 0 :
                    _settings.NumberOfItemsPerPage > 0 ? _settings.NumberOfItemsPerPage :
                    Mathf.Max(5, _values.Length / 2);
            });
            menu.AddItem("Search Table", _searchableAll, () =>
            {
                _searchableAll = !_searchableAll;
                RefreshSearchDisplay();
            });
            menu.AddItem("Search Columns", _searchableCols, () =>
            {
                _searchableCols = !_searchableCols;
                RefreshSearchDisplay();
            });
            menu.AddItem("Default Search", _defaultSearch, () =>
            {
                _defaultSearch = !_defaultSearch;
                RefreshSearch();
            });
            menu.AddItem("Object Search", _objectSearch, () =>
            {
                _objectSearch = !_objectSearch;
                RefreshSearch();
            });
            if (_customSearch != null)
            {
                menu.AddItem("Extra Search", _extraSearch, () =>
                {
                    _extraSearch = !_extraSearch;
                    RefreshSearch();
                });
            }

            menu.AddSeparator("");
            menu.AddItem("Collapse All", false, () => ToggleAll(false));
            menu.AddItem("Expand All", false, () => ToggleAll(true));
            Rect bounds = _foldout.MenuButton.worldBound;
#if !UNITY_6000_3_OR_NEWER
            bounds.xMin = bounds.xMax - Mathf.Max(bounds.width, 120f);
#endif
            menu.DropDown(bounds, _foldout.MenuButton,
#if UNITY_6000_3_OR_NEWER
                DropdownMenuSizeMode.Auto
#else
                true
#endif
            );
        }

        private void RefreshSearchDisplay()
        {
            _searchField.style.display = _searchableAll ? DisplayStyle.Flex : DisplayStyle.None;
            // Recreate headers to apply the column search visibility.
            _listView.Rebuild();
            RefreshSearch();
        }

        private void ToggleAll(bool expand)
        {
            for (int index = 0; index < _values.Length; index++)
            {
                string key = $"{_viewKey}:{index}";
                SessionState.SetBool(key, expand);
                SessionState.SetBool(key + ".initialized", true);
            }

            foreach (TableCellFoldableElement cell in _listView.Query<TableCellFoldableElement>().Build())
            {
                cell.SetValueWithoutNotify(expand);
            }
        }
    }
}
#endif
