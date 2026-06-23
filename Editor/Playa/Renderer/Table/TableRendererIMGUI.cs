using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using SaintsField.Editor.Core;
using SaintsField.Editor.Drawers.SaintsRowDrawer;
using SaintsField.Editor.Playa.Renderer.BaseRenderer;
using SaintsField.Editor.Utils;
using SaintsField.Playa;
using SaintsField.Utils;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;
using Object = UnityEngine.Object;

namespace SaintsField.Editor.Playa.Renderer.Table
{
    public partial class TableRenderer
    {
        private const float HeaderSizeWidthIMGUI = 50f;
        private const float FooterButtonWidthIMGUI = 20f;
        private const float FooterHeightIMGUI = 22f;
        private const float ControlGapIMGUI = 4f;
        private const float TablePaddingIMGUI = 2f;
        private const float NonEmptyTableHeightPaddingIMGUI = 18f;

        private TableTreeViewIMGUI _tableIMGUI;
        private string _tableSignatureIMGUI;
        private string _searchStringIMGUI = "";
        private bool _applicationChangedListenerAddedIMGUI;
        private bool _contextDirtyIMGUI = true;
        private bool _cellCacheDirtyIMGUI;
        private string _contextSignatureIMGUI;
        private TableContextIMGUI _contextIMGUI;
        private readonly Dictionary<string, CellContentIMGUI> _cellContentCacheIMGUI =
            new Dictionary<string, CellContentIMGUI>();
        private readonly Dictionary<string, float> _cellHeightCacheIMGUI = new Dictionary<string, float>();

        private sealed class ColumnInfoIMGUI
        {
            public string Id;
            public string Title;
            public List<string> MemberIds = new List<string>();
            public bool Visible = true;
            public bool IsFallback;
        }

        private sealed class TableContextIMGUI
        {
            public SerializedProperty ArrayProperty;
            public TableAttribute TableAttribute;
            public Type ElementType;
            public List<ColumnInfoIMGUI> Columns = new List<ColumnInfoIMGUI>();
            public bool HasObjectReferencePicker;
        }

        private sealed class CellContentIMGUI
        {
            public string Error = "";
            public SerializedProperty FallbackProperty;
            public Type FallbackType;
            public SerializedObject TargetSerializedObject;
            public List<AbsRenderer> Renderers = new List<AbsRenderer>();
        }

        private class TableTreeViewIMGUI :
            TreeView
#if UNITY_6000_2_OR_NEWER
            <int>
#endif
        {
            private readonly TableRenderer _owner;
            private TableContextIMGUI _context;
            private int _itemCount;
            private bool _loaded;
            private int[] _draggedIndexes = Array.Empty<int>();

            public TableTreeViewIMGUI(
                TreeViewState
#if UNITY_6000_2_OR_NEWER
                <int>
#endif
                    state,
                MultiColumnHeader multiColumnHeader,
                TableRenderer owner,
                TableContextIMGUI context) : base(state, multiColumnHeader)
            {
                _owner = owner;
                _context = context;
                rowHeight = SaintsPropertyDrawer.SingleLineHeight;
                showAlternatingRowBackgrounds = true;
                showBorder = true;
            }

            public void SetContext(TableContextIMGUI context)
            {
                _context = context;
            }

            public void SetItemCount(int itemCount)
            {
                itemCount = Mathf.Max(0, itemCount);
                if (_loaded && _itemCount == itemCount)
                {
                    return;
                }

                _itemCount = itemCount;
                Reload();
                _loaded = true;
            }

            protected override
                TreeViewItem
#if UNITY_6000_2_OR_NEWER
                <int>
#endif
                BuildRoot()
            {
                TreeViewItem
#if UNITY_6000_2_OR_NEWER
                <int>
#endif
                    root = new
                        TreeViewItem
#if UNITY_6000_2_OR_NEWER
                        <int>
#endif
                    {
                        id = -1,
                        depth = -1,
                        displayName = "Root",
                    };

                List<
                    TreeViewItem
#if UNITY_6000_2_OR_NEWER
                    <int>
#endif
                > items = Enumerable.Range(0, _itemCount)
                    .Select(index => new
                        TreeViewItem
#if UNITY_6000_2_OR_NEWER
                        <int>
#endif
                    {
                        id = index,
                        depth = 0,
                        displayName = $"{index}",
                    })
                    .ToList();

                SetupParentsAndChildrenFromDepths(root, items);
                return root;
            }

            protected override float GetCustomRowHeight(
                int row,
                TreeViewItem
#if UNITY_6000_2_OR_NEWER
                <int>
#endif
                    item)
            {
                if (_context?.Columns == null || _context.Columns.Count == 0)
                {
                    return SaintsPropertyDrawer.SingleLineHeight;
                }

                float maxHeight = SaintsPropertyDrawer.SingleLineHeight;
                MultiColumnHeaderState.Column[] columns = multiColumnHeader.state.columns;
                for (int columnIndex = 0; columnIndex < _context.Columns.Count; columnIndex++)
                {
                    if (columnIndex >= columns.Length)
                    {
                        continue;
                    }

                    float width = Mathf.Max(40f, columns[columnIndex].width);
                    maxHeight = Mathf.Max(maxHeight, _owner.GetCellHeightIMGUI(_context, item.id,
                        _context.Columns[columnIndex], width));
                }

                return maxHeight;
            }

            protected override void RowGUI(RowGUIArgs args)
            {
                TreeViewItem
#if UNITY_6000_2_OR_NEWER
                <int>
#endif
                    item = args.item;

                for (int visibleColumn = 0; visibleColumn < args.GetNumVisibleColumns(); visibleColumn++)
                {
                    int columnIndex = args.GetColumn(visibleColumn);
                    if (_context == null || columnIndex < 0 || columnIndex >= _context.Columns.Count)
                    {
                        continue;
                    }

                    _owner.DrawCellIMGUI(args.GetCellRect(visibleColumn), _context, item.id,
                        _context.Columns[columnIndex]);
                }
            }

            protected override bool CanStartDrag(CanStartDragArgs args)
            {
                _draggedIndexes = args.draggedItemIDs.OrderByDescending(each => each).ToArray();
                return true;
            }

            protected override void SetupDragAndDrop(SetupDragAndDropArgs args)
            {
                DragAndDrop.PrepareStartDrag();
                DragAndDrop.StartDrag("SaintsTable");
            }

            protected override DragAndDropVisualMode HandleDragAndDrop(DragAndDropArgs args)
            {
                if (args.performDrop && _draggedIndexes.Length > 0 && _itemCount > 0)
                {
                    int toIndex = Mathf.Clamp(args.insertAtIndex, 0, _itemCount - 1);

                    foreach (int fromIndex in _draggedIndexes)
                    {
                        _context.ArrayProperty.MoveArrayElement(fromIndex, toIndex);
                    }

                    _context.ArrayProperty.serializedObject.ApplyModifiedProperties();
                    _draggedIndexes = Array.Empty<int>();
                    _owner.NotifyTableChangedIMGUI();
                }

                return DragAndDropVisualMode.Move;
            }
        }

        public override void OnDestroy()
        {
            if (_applicationChangedListenerAddedIMGUI)
            {
                SaintsEditorApplicationChanged.OnAnyEvent.RemoveListener(MarkTableCacheDirtyIMGUI);
                _applicationChangedListenerAddedIMGUI = false;
            }

            ClearCellCacheIMGUI();
            _tableIMGUI = null;
        }

        public override void OnSearchField(string searchString)
        {
            base.OnSearchField(searchString);
            _searchStringIMGUI = searchString ?? "";
        }

        protected override void RenderTargetIMGUI(float width, PreCheckResult preCheckResult)
        {
            EnsureApplicationChangedListenerIMGUI();
            float height = GetFieldHeightIMGUI(width, preCheckResult);
            if (height <= Mathf.Epsilon)
            {
                return;
            }

            Rect rect = EditorGUILayout.GetControlRect(false, height, GUILayout.ExpandWidth(true));
            RenderPositionTargetIMGUI(rect, preCheckResult);
        }

        protected override float GetFieldHeightIMGUI(float width, PreCheckResult preCheckResult)
        {
            EnsureApplicationChangedListenerIMGUI();
            if (!ShouldDrawIMGUI(preCheckResult))
            {
                return 0f;
            }

            (string error, TableContextIMGUI context) = GetContextIMGUI();
            if (error != "")
            {
                return SaintsPropertyDrawer.SingleLineHeight + ImGuiHelpBox.GetHeight(error, width, MessageType.Error);
            }

            SerializedProperty arrayProperty = context.ArrayProperty;
            if (!arrayProperty.isExpanded)
            {
                return SaintsPropertyDrawer.SingleLineHeight;
            }

            float contentHeight;
            if (arrayProperty.arraySize == 0)
            {
                contentHeight = SaintsPropertyDrawer.SingleLineHeight + TablePaddingIMGUI * 2f;
            }
            else if (context.HasObjectReferencePicker)
            {
                contentHeight = SaintsPropertyDrawer.SingleLineHeight
                                + TablePaddingIMGUI * 2f
                                + NonEmptyTableHeightPaddingIMGUI;
            }
            else
            {
                EnsureTableIMGUI(context, width);
                contentHeight = (_tableIMGUI?.totalHeight ?? SaintsPropertyDrawer.SingleLineHeight)
                                + TablePaddingIMGUI * 2f
                                + NonEmptyTableHeightPaddingIMGUI;
            }

            bool drawFooter = !context.TableAttribute.HideAddButton || !context.TableAttribute.HideRemoveButton;
            return SaintsPropertyDrawer.SingleLineHeight + contentHeight + (drawFooter ? FooterHeightIMGUI : 0f);
        }

        protected override void RenderPositionTargetIMGUI(Rect position, PreCheckResult preCheckResult)
        {
            EnsureApplicationChangedListenerIMGUI();
            if (!ShouldDrawIMGUI(preCheckResult))
            {
                return;
            }

            (string error, TableContextIMGUI context) = GetContextIMGUI();
            if (error != "")
            {
                DrawContextErrorIMGUI(position, error);
                return;
            }

            SerializedProperty arrayProperty = context.ArrayProperty;
            if (CanDrawTreeViewIMGUI(context))
            {
                EnsureTableIMGUI(context, position.width);
            }

            (Rect headerRect, Rect leftRect) = RectUtils.SplitHeightRect(position, SaintsPropertyDrawer.SingleLineHeight);
            DrawHeaderIMGUI(headerRect, context);

            if (!arrayProperty.isExpanded)
            {
                return;
            }

            bool drawFooter = !context.TableAttribute.HideAddButton || !context.TableAttribute.HideRemoveButton;
            Rect bodyRect = leftRect;
            Rect footerRect = Rect.zero;
            if (drawFooter)
            {
                (bodyRect, footerRect) = RectUtils.SplitHeightRect(leftRect,
                    Mathf.Max(0f, leftRect.height - FooterHeightIMGUI));
            }

            DrawBodyIMGUI(bodyRect, context);

            if (drawFooter)
            {
                DrawFooterIMGUI(footerRect, context);
            }
        }

        private bool ShouldDrawIMGUI(PreCheckResult preCheckResult)
        {
            return preCheckResult.IsShown
                   && Util.UnityDefaultSimpleSearch(FieldWithInfo.SerializedProperty.displayName, _searchStringIMGUI);
        }

        private void EnsureApplicationChangedListenerIMGUI()
        {
            if (_applicationChangedListenerAddedIMGUI)
            {
                return;
            }

            _applicationChangedListenerAddedIMGUI = true;
            SaintsEditorApplicationChanged.OnAnyEvent.AddListener(MarkTableCacheDirtyIMGUI);
        }

        private void MarkTableCacheDirtyIMGUI()
        {
            _contextDirtyIMGUI = true;
            _contextIMGUI = null;
            _contextSignatureIMGUI = null;
            _tableSignatureIMGUI = null;
            _cellCacheDirtyIMGUI = true;
            _cellHeightCacheIMGUI.Clear();
        }

        private void ClearCellCacheIMGUI()
        {
            foreach (CellContentIMGUI content in _cellContentCacheIMGUI.Values)
            {
                foreach (AbsRenderer renderer in content.Renderers)
                {
                    renderer.OnDestroy();
                }
            }

            _cellContentCacheIMGUI.Clear();
            _cellHeightCacheIMGUI.Clear();
            _cellCacheDirtyIMGUI = false;
        }

        private (string error, TableContextIMGUI context) GetContextIMGUI()
        {
            string signature = GetContextSignatureIMGUI();
            if (!_contextDirtyIMGUI && _contextIMGUI != null && _contextSignatureIMGUI == signature)
            {
                return ("", _contextIMGUI);
            }

            ClearCellCacheIMGUI();
            (string error, TableContextIMGUI context) = BuildContextIMGUI();
            if (error != "")
            {
                _contextDirtyIMGUI = true;
                _contextIMGUI = null;
                _contextSignatureIMGUI = null;
                return (error, null);
            }

            _contextDirtyIMGUI = false;
            _contextIMGUI = context;
            _contextSignatureIMGUI = signature;
            return ("", context);
        }

        private string GetContextSignatureIMGUI()
        {
            SerializedProperty arrayProperty = FieldWithInfo.SerializedProperty;
            if (arrayProperty == null)
            {
                return "<null>";
            }

            string propertyId = SerializedUtils.GetUniqueId(arrayProperty);
            if (!arrayProperty.isArray)
            {
                return $"{propertyId}:<not-array>";
            }

            string firstSignature = "";
            if (arrayProperty.arraySize > 0)
            {
                SerializedProperty firstProp = arrayProperty.GetArrayElementAtIndex(0);
                firstSignature = GetSerializedPropertySignatureIMGUI(firstProp);
            }

            return $"{propertyId}:{arrayProperty.arraySize}:{firstSignature}";
        }

        private static string GetSerializedPropertySignatureIMGUI(SerializedProperty property)
        {
            switch (property.propertyType)
            {
                case SerializedPropertyType.ObjectReference:
                    return $"{property.propertyType}:{property.type}:{GetObjectReferenceSignatureIMGUI(property.objectReferenceValue)}";
                case SerializedPropertyType.ManagedReference:
                    return $"{property.propertyType}:{property.managedReferenceFullTypename}";
                default:
                    return $"{property.propertyType}:{property.type}";
            }
        }

        private static string GetObjectReferenceSignatureIMGUI(Object value)
        {
            if (!value)
            {
                return "0";
            }

            return value.
#if UNITY_6000_4_OR_NEWER
                GetEntityId
#else
                GetInstanceID
#endif
                    ().ToString();
        }

        private (string error, TableContextIMGUI context) BuildContextIMGUI()
        {
            SerializedProperty arrayProperty = FieldWithInfo.SerializedProperty;
            if (arrayProperty == null || !arrayProperty.isArray)
            {
                return ($"{FieldWithInfo} is not an array/list", null);
            }

            TableAttribute tableAttribute = FieldWithInfo.PlayaAttributes.OfType<TableAttribute>().FirstOrDefault();
            if (tableAttribute == null)
            {
                return ($"{arrayProperty.propertyPath} has no TableAttribute", null);
            }

            Type sourceType = FieldWithInfo.FieldInfo?.FieldType ?? FieldWithInfo.PropertyInfo?.PropertyType;
            Type elementType = ReflectUtils.GetElementType(sourceType);
            if (elementType == null)
            {
                return ($"Failed to resolve element type for {arrayProperty.propertyPath}", null);
            }

            TableContextIMGUI context = new TableContextIMGUI
            {
                ArrayProperty = arrayProperty,
                TableAttribute = tableAttribute,
                ElementType = elementType,
            };

            if (arrayProperty.arraySize == 0)
            {
                return ("", context);
            }

            SerializedProperty firstProp = arrayProperty.GetArrayElementAtIndex(0);
            if (firstProp.propertyType == SerializedPropertyType.ObjectReference)
            {
                Object schemaObject = MakeSourceIMGUI(arrayProperty)
                    .Select(each => each.objectReferenceValue)
                    .FirstOrDefault(each => each);
                if (!schemaObject)
                {
                    context.HasObjectReferencePicker = true;
                    return ("", context);
                }

                context.Columns = BuildObjectColumnsIMGUI(schemaObject);
            }
            else
            {
                (string error, List<ColumnInfoIMGUI> columns) = BuildValueColumnsIMGUI(firstProp);
                if (error != "")
                {
                    return (error, null);
                }

                context.Columns = columns;
            }

            if (context.Columns.Count == 0)
            {
                context.Columns.Add(new ColumnInfoIMGUI
                {
                    Id = "__value",
                    Title = "Value",
                    IsFallback = true,
                    Visible = true,
                });
            }

            return ("", context);
        }

        private List<ColumnInfoIMGUI> BuildObjectColumnsIMGUI(Object schemaObject)
        {
            using (SerializedObject serializedObject = new SerializedObject(schemaObject))
            {
                Dictionary<string, SerializedProperty> serializedPropertyDict = SerializedUtils
                    .GetAllField(serializedObject)
                    .Where(each => each != null)
                    .ToDictionary(each => each.name, each => each.Copy());

                IEnumerable<SaintsFieldWithInfo> saintsFieldWithInfos = SaintsEditor
                    .HelperGetSaintsFieldWithInfo(FieldWithInfo.SerializedProperty.serializedObject,
                        serializedPropertyDict, null, null, -1, new[] { schemaObject })
                    .Where(SaintsEditor.SaintsFieldInfoShouldDraw);

                return BuildColumnsFromFieldInfosIMGUI(saintsFieldWithInfos);
            }
        }

        private (string error, List<ColumnInfoIMGUI> columns) BuildValueColumnsIMGUI(SerializedProperty firstProp)
        {
            Dictionary<string, SerializedProperty> firstSerializedPropertyDict = SerializedUtils.GetPropertyChildren(firstProp)
                .Where(each => each != null)
                .ToDictionary(each => each.name);

            if (firstSerializedPropertyDict.Count == 0)
            {
                return ("", new List<ColumnInfoIMGUI>
                {
                    new ColumnInfoIMGUI
                    {
                        Id = "__value",
                        Title = "Value",
                        IsFallback = true,
                        Visible = true,
                    },
                });
            }

            MemberInfo info = (MemberInfo)FieldWithInfo.FieldInfo ?? FieldWithInfo.PropertyInfo;
            (PropertyAttribute[] _, object parentRefreshed) =
                SerializedUtils.GetAttributesAndDirectParent<PropertyAttribute>(firstProp);
            (string error, int _, object value) firstPropValue = Util.GetValue(firstProp, info, parentRefreshed);
            if (firstPropValue.error != "")
            {
                return (firstPropValue.error, null);
            }

            IEnumerable<SaintsFieldWithInfo> saintsFieldWithInfos = SaintsEditor
                .HelperGetSaintsFieldWithInfo(FieldWithInfo.SerializedProperty.serializedObject,
                    firstSerializedPropertyDict, null, null, -1, new[] { firstPropValue.value })
                .Where(SaintsEditor.SaintsFieldInfoShouldDraw);

            return ("", BuildColumnsFromFieldInfosIMGUI(saintsFieldWithInfos));
        }

        private List<ColumnInfoIMGUI> BuildColumnsFromFieldInfosIMGUI(IEnumerable<SaintsFieldWithInfo> saintsFieldWithInfos)
        {
            (HashSet<string> valueTableHeaders, bool headerIsHide) = BuildHeaderFilterIMGUI();
            Dictionary<string, ColumnInfoIMGUI> columnByName = new Dictionary<string, ColumnInfoIMGUI>();
            Dictionary<string, bool> columnToDefaultHide = new Dictionary<string, bool>();

            foreach (SaintsFieldWithInfo saintsFieldWithInfo in saintsFieldWithInfos)
            {
                string columnName = AbsRenderer.GetFriendlyName(saintsFieldWithInfo);
                foreach (IPlayaAttribute playaAttribute in saintsFieldWithInfo.PlayaAttributes)
                {
                    if (playaAttribute is TableColumnAttribute tableColumnAttribute)
                    {
                        columnName = tableColumnAttribute.Title;
                        break;
                    }
                }

                bool hidden = HeaderDefaultHideIMGUI(columnName, valueTableHeaders, headerIsHide);
                if (!hidden)
                {
                    foreach (IPlayaAttribute playaAttribute in saintsFieldWithInfo.PlayaAttributes)
                    {
                        if (playaAttribute is TableHideAttribute)
                        {
                            hidden = true;
                            break;
                        }
                    }
                }

                if (!columnByName.TryGetValue(columnName, out ColumnInfoIMGUI columnInfo))
                {
                    columnInfo = new ColumnInfoIMGUI
                    {
                        Id = columnName,
                        Title = columnName,
                        Visible = !hidden,
                    };
                    columnByName[columnName] = columnInfo;
                }

                if (hidden)
                {
                    columnToDefaultHide[columnName] = true;
                }

                columnInfo.MemberIds.Add(saintsFieldWithInfo.MemberId);
            }

            foreach (KeyValuePair<string, bool> pair in columnToDefaultHide)
            {
                if (columnByName.TryGetValue(pair.Key, out ColumnInfoIMGUI columnInfo))
                {
                    columnInfo.Visible = !pair.Value;
                }
            }

            List<ColumnInfoIMGUI> columns = columnByName.Values
                .Where(each => each.Visible)
                .ToList();
            foreach (ColumnInfoIMGUI columnInfo in columns)
            {
                columnInfo.Id = string.Join(";", columnInfo.MemberIds);
            }

            return columns;
        }

        private (HashSet<string> valueTableHeaders, bool headerIsHide) BuildHeaderFilterIMGUI()
        {
            MemberInfo info = (MemberInfo)FieldWithInfo.FieldInfo ?? FieldWithInfo.PropertyInfo;
            TableHeadersAttribute tableHeadersAttribute = ReflectCache.GetCustomAttributes<TableHeadersAttribute>(info)
                .FirstOrDefault();
            HashSet<string> valueTableHeaders = new HashSet<string>();
            bool headerIsHide = true;
            if (tableHeadersAttribute == null)
            {
                return (valueTableHeaders, headerIsHide);
            }

            headerIsHide = tableHeadersAttribute.IsHide;
            foreach (TableHeadersAttribute.Header header in tableHeadersAttribute.Headers)
            {
                List<string> rawNames = new List<string>();
                if (header.IsCallback)
                {
                    (string error, object value) = Util.GetOfNoParams<object>(FieldWithInfo.Targets[0], header.Name,
                        null);
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
                    else if (value is string s)
                    {
                        rawNames.Add(s);
                    }
                    else if (value is IEnumerable<string> strings)
                    {
                        rawNames.AddRange(strings.Where(each => each != null));
                    }
                    else if (value is IEnumerable<object> objects)
                    {
                        rawNames.AddRange(objects.Where(each => !RuntimeUtil.IsNull(each))
                            .Select(each => each.ToString()));
                    }
                }
                else
                {
                    rawNames.Add(header.Name);
                }

                valueTableHeaders.UnionWith(rawNames.SelectMany(each => new[]
                {
                    each,
                    ObjectNames.NicifyVariableName(each),
                }));
            }

            return (valueTableHeaders, headerIsHide);
        }

        private static bool HeaderDefaultHideIMGUI(string value, ICollection<string> valueTableHeaders,
            bool headerIsHide)
        {
            bool inHeader = valueTableHeaders.Contains(value);
            return headerIsHide ? inHeader : !inHeader;
        }

        private static bool CanDrawTreeViewIMGUI(TableContextIMGUI context)
        {
            return context.ArrayProperty.arraySize > 0
                   && !context.HasObjectReferencePicker
                   && context.Columns.Count > 0;
        }

        private void EnsureTableIMGUI(TableContextIMGUI context, float width)
        {
            if (!CanDrawTreeViewIMGUI(context))
            {
                _tableIMGUI = null;
                _tableSignatureIMGUI = null;
                return;
            }

            string signature = GetTableSignatureIMGUI(context);
            if (_tableIMGUI == null || _tableSignatureIMGUI != signature)
            {
                _tableSignatureIMGUI = signature;

                MultiColumnHeaderState.Column[] columns = context.Columns
                    .Select(column => new MultiColumnHeaderState.Column
                    {
                        headerContent = new GUIContent(column.Title),
                        autoResize = true,
                        canSort = false,
                        allowToggleVisibility = true,
                        minWidth = 40f,
                        width = Mathf.Max(40f, width / Mathf.Max(1, context.Columns.Count)),
                    })
                    .ToArray();

                _tableIMGUI = new TableTreeViewIMGUI(
                    new TreeViewState
#if UNITY_6000_2_OR_NEWER
                    <int>
#endif
                    (),
                    new MultiColumnHeader(new MultiColumnHeaderState(columns)),
                    this,
                    context);
            }

            _tableIMGUI.SetContext(context);
            _tableIMGUI.SetItemCount(context.ArrayProperty.arraySize);
        }

        private static string GetTableSignatureIMGUI(TableContextIMGUI context)
        {
            string columnSignature = string.Join("|", context.Columns.Select(each =>
                $"{each.Id}:{each.Title}:{each.Visible}:{each.IsFallback}"));
            return $"{context.ArrayProperty.propertyPath}:{context.HasObjectReferencePicker}:{columnSignature}";
        }

        private void NotifyTableChangedIMGUI()
        {
            _tableIMGUI?.Reload();
            MarkTableCacheDirtyIMGUI();
            SaintsEditorApplicationChanged.OnSaintsFieldChangedEvent.Invoke();
        }

        private void DrawHeaderIMGUI(Rect rect, TableContextIMGUI context)
        {
            SerializedProperty arrayProperty = context.ArrayProperty;
            Rect sizeRect = new Rect(rect)
            {
                x = rect.xMax - HeaderSizeWidthIMGUI,
                width = HeaderSizeWidthIMGUI,
            };
            Rect foldoutRect = new Rect(rect)
            {
                width = Mathf.Max(0f, rect.width - HeaderSizeWidthIMGUI - ControlGapIMGUI),
            };

            arrayProperty.isExpanded = EditorGUI.Foldout(foldoutRect, arrayProperty.isExpanded, arrayProperty.displayName,
                true);

            using (new EditorGUI.DisabledScope(context.TableAttribute.HideAddButton
                                               && context.TableAttribute.HideRemoveButton))
            using (EditorGUI.ChangeCheckScope changed = new EditorGUI.ChangeCheckScope())
            {
                int newValue = EditorGUI.DelayedIntField(sizeRect, GUIContent.none, arrayProperty.arraySize);
                if (changed.changed)
                {
                    int oldValue = arrayProperty.arraySize;
                    int changedValue = ChangeArraySize(newValue, arrayProperty);
                    if (changedValue != oldValue)
                    {
                        NotifyTableChangedIMGUI();
                    }
                }
            }
        }

        private void DrawBodyIMGUI(Rect rect, TableContextIMGUI context)
        {
            Rect contentRect = new Rect(rect)
            {
                x = rect.x + TablePaddingIMGUI,
                y = rect.y + TablePaddingIMGUI,
                width = Mathf.Max(0f, rect.width - TablePaddingIMGUI * 2f),
                height = Mathf.Max(0f, rect.height - TablePaddingIMGUI * 2f),
            };

            if (context.ArrayProperty.arraySize == 0)
            {
                GUI.Box(contentRect, GUIContent.none, EditorStyles.helpBox);
                EditorGUI.LabelField(contentRect, "Table is empty");
                return;
            }

            if (context.HasObjectReferencePicker)
            {
                SerializedProperty firstProp = context.ArrayProperty.GetArrayElementAtIndex(0);
                using (EditorGUI.ChangeCheckScope changed = new EditorGUI.ChangeCheckScope())
                {
                    Object result = EditorGUI.ObjectField(contentRect, GUIContent.none, firstProp.objectReferenceValue,
                        context.ElementType, true);
                    if (changed.changed)
                    {
                        firstProp.objectReferenceValue = result;
                        firstProp.serializedObject.ApplyModifiedProperties();
                        NotifyTableChangedIMGUI();
                    }
                }
                return;
            }

            _tableIMGUI?.OnGUI(contentRect);

            int beforeDragDropSize = context.ArrayProperty.arraySize;
            DragAndDropImGui(contentRect, context.ElementType, context.ArrayProperty);
            if (context.ArrayProperty.arraySize != beforeDragDropSize)
            {
                NotifyTableChangedIMGUI();
            }
        }

        private void DrawFooterIMGUI(Rect rect, TableContextIMGUI context)
        {
            TableAttribute tableAttribute = context.TableAttribute;
            float x = rect.xMax;

            if (!tableAttribute.HideRemoveButton)
            {
                x -= FooterButtonWidthIMGUI;
                Rect removeRect = new Rect(rect)
                {
                    x = x,
                    width = FooterButtonWidthIMGUI,
                };

                if (GUI.Button(removeRect, "-", EditorStyles.miniButtonRight))
                {
                    IReadOnlyList<int> selected = _tableIMGUI?.GetSelection().ToArray() ?? Array.Empty<int>();
                    DeleteArrayElement(context.ArrayProperty, selected);
                    NotifyTableChangedIMGUI();
                }
            }

            if (!tableAttribute.HideAddButton)
            {
                x -= FooterButtonWidthIMGUI;
                Rect addRect = new Rect(rect)
                {
                    x = x,
                    width = FooterButtonWidthIMGUI,
                };

                if (GUI.Button(addRect, "+", EditorStyles.miniButtonLeft))
                {
                    ChangeArraySize(context.ArrayProperty.arraySize + 1, context.ArrayProperty);
                    NotifyTableChangedIMGUI();
                }
            }
        }

        private float GetCellHeightIMGUI(TableContextIMGUI context, int rowIndex, ColumnInfoIMGUI column,
            float width)
        {
            string heightKey = $"{GetCellCacheKeyIMGUI(context, rowIndex, column)}:{Mathf.RoundToInt(width)}";
            if (_cellHeightCacheIMGUI.TryGetValue(heightKey, out float cachedHeight))
            {
                return cachedHeight;
            }

            CellContentIMGUI content = GetCellContentIMGUI(context, rowIndex, column);
            float height;
            if (content.Error != "")
            {
                height = ImGuiHelpBox.GetHeight(content.Error, width, MessageType.Error);
            }
            else if (content.FallbackProperty != null)
            {
                height = EditorGUI.GetPropertyHeight(content.FallbackProperty, GUIContent.none, true);
            }
            else if (content.Renderers.Count == 0)
            {
                height = SaintsPropertyDrawer.SingleLineHeight;
            }
            else
            {
                height = content.Renderers.Sum(renderer => renderer.GetHeightIMGUI(width));
            }

            _cellHeightCacheIMGUI[heightKey] = height;
            return height;
        }

        private void DrawCellIMGUI(Rect rect, TableContextIMGUI context, int rowIndex, ColumnInfoIMGUI column)
        {
            CellContentIMGUI content = GetCellContentIMGUI(context, rowIndex, column);
            if (content.Error != "")
            {
                ImGuiHelpBox.Draw(rect, content.Error, MessageType.Error);
                return;
            }

            if (content.FallbackProperty != null)
            {
                EditorGUI.PropertyField(rect, content.FallbackProperty, GUIContent.none, true);
                return;
            }

            Rect leftRect = rect;
            foreach (AbsRenderer renderer in content.Renderers)
            {
                float height = renderer.GetHeightIMGUI(rect.width);
                (Rect rendererRect, Rect newLeftRect) = RectUtils.SplitHeightRect(leftRect, height);
                leftRect = newLeftRect;
                renderer.RenderPositionIMGUI(rendererRect);
            }
        }

        private CellContentIMGUI GetCellContentIMGUI(TableContextIMGUI context, int rowIndex, ColumnInfoIMGUI column)
        {
            if (_cellCacheDirtyIMGUI)
            {
                ClearCellCacheIMGUI();
            }

            string key = GetCellCacheKeyIMGUI(context, rowIndex, column);
            if (_cellContentCacheIMGUI.TryGetValue(key, out CellContentIMGUI cachedContent))
            {
                return cachedContent;
            }

            CellContentIMGUI content = BuildCellContentIMGUI(context, rowIndex, column);
            _cellContentCacheIMGUI[key] = content;
            return content;
        }

        private static string GetCellCacheKeyIMGUI(TableContextIMGUI context, int rowIndex, ColumnInfoIMGUI column)
        {
            return $"{context.ArrayProperty.propertyPath}:{context.ArrayProperty.arraySize}:{rowIndex}:{column.Id}:{column.IsFallback}";
        }

        private CellContentIMGUI BuildCellContentIMGUI(TableContextIMGUI context, int rowIndex, ColumnInfoIMGUI column)
        {
            CellContentIMGUI content = new CellContentIMGUI();
            SerializedProperty arrayProperty = context.ArrayProperty;
            if (rowIndex < 0 || rowIndex >= arrayProperty.arraySize)
            {
                return content;
            }

            SerializedProperty targetProp = arrayProperty.GetArrayElementAtIndex(rowIndex);
            targetProp.isExpanded = true;

            if (column.IsFallback)
            {
                content.FallbackProperty = targetProp;
                content.FallbackType = context.ElementType;
                return content;
            }

            if (targetProp.propertyType == SerializedPropertyType.ObjectReference)
            {
                Object targetObject = targetProp.objectReferenceValue;
                if (RuntimeUtil.IsNull(targetObject))
                {
                    content.FallbackProperty = targetProp;
                    content.FallbackType = context.ElementType;
                    return content;
                }

                content.TargetSerializedObject = new SerializedObject(targetObject);
                Dictionary<string, SerializedProperty> targetPropertyDict = SerializedUtils
                    .GetAllField(content.TargetSerializedObject)
                    .Where(each => each != null)
                    .ToDictionary(each => each.name, each => each.Copy());

                IEnumerable<SaintsFieldWithInfo> saintsFieldWithInfos = SaintsEditor
                    .HelperGetSaintsFieldWithInfo(arrayProperty.serializedObject, targetPropertyDict, null, null, -1,
                        new[] { targetObject })
                    .Where(each => column.MemberIds.Contains(each.MemberId));

                FillRenderersIMGUI(content, saintsFieldWithInfos, column.MemberIds.Count == 1);
                return content;
            }

            MemberInfo info = (MemberInfo)FieldWithInfo.FieldInfo ?? FieldWithInfo.PropertyInfo;
            (PropertyAttribute[] _, object parentRefreshed) =
                SerializedUtils.GetAttributesAndDirectParent<PropertyAttribute>(targetProp);
            (string error, int _, object value) targetPropValue = Util.GetValue(targetProp, info, parentRefreshed);
            if (targetPropValue.error != "")
            {
                content.Error = targetPropValue.error;
                return content;
            }

            Dictionary<string, SerializedProperty> targetSerializedPropertyDict = SerializedUtils.GetPropertyChildren(targetProp)
                .Where(each => each != null)
                .ToDictionary(each => each.name);

            if (targetSerializedPropertyDict.Count == 0)
            {
                content.FallbackProperty = targetProp;
                content.FallbackType = context.ElementType;
                return content;
            }

            IEnumerable<SaintsFieldWithInfo> valueFieldInfos = SaintsEditor
                .HelperGetSaintsFieldWithInfo(arrayProperty.serializedObject, targetSerializedPropertyDict, null, null,
                    -1, new[] { targetPropValue.value })
                .Where(each => column.MemberIds.Contains(each.MemberId));

            FillRenderersIMGUI(content, valueFieldInfos, column.MemberIds.Count == 1);
            return content;
        }

        private void FillRenderersIMGUI(CellContentIMGUI content,
            IEnumerable<SaintsFieldWithInfo> saintsFieldWithInfos, bool saintsRowInline)
        {
            List<SaintsFieldWithInfo> allSaintsFieldWithInfos = saintsFieldWithInfos.ToList();
            int serCount = allSaintsFieldWithInfos.Count(each => each.SerializedProperty != null);
            bool noLabel = serCount <= 1;

            using (new SaintsRowAttributeDrawer.ForceInlineScoop(saintsRowInline ? 1 : 0))
            {
                foreach (SaintsFieldWithInfo saintsFieldWithInfo in allSaintsFieldWithInfos)
                {
                    foreach (IReadOnlyList<AbsRenderer> renderers in SaintsEditor.HelperMakeRenderer(
                                 FieldWithInfo.SerializedProperty.serializedObject, saintsFieldWithInfo))
                    {
                        foreach (AbsRenderer renderer in renderers)
                        {
                            renderer.NoLabel = noLabel;
                            renderer.InDirectHorizontalLayout = renderer.InAnyHorizontalLayout = true;
                            content.Renderers.Add(renderer);
                        }
                    }
                }
            }
        }

        private void DrawContextErrorIMGUI(Rect position, string error)
        {
            (Rect labelRect, Rect helpRect) = RectUtils.SplitHeightRect(position, SaintsPropertyDrawer.SingleLineHeight);
            EditorGUI.LabelField(labelRect, FieldWithInfo.SerializedProperty.displayName);
            ImGuiHelpBox.Draw(helpRect, error, MessageType.Error);
        }

        private static List<SerializedProperty> MakeSourceIMGUI(SerializedProperty arrayProp)
        {
            return Enumerable.Range(0, arrayProp.arraySize)
                .Select(arrayProp.GetArrayElementAtIndex)
                .ToList();
        }
    }
}
