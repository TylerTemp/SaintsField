using System;
using System.Collections.Generic;
using System.Linq;
using SaintsField.DropdownBase;
using SaintsField.Editor.Core;
using SaintsField.Editor.Drawers.AdvancedDropdownDrawer;
using SaintsField.Editor.Utils;
using SaintsField.Playa;
using SaintsField.Utils;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace SaintsField.Editor.Drawers.TreeDropdownDrawer
{
    public class SaintsTreeDropdownIMGUI : PopupWindowContent
    {
        private const float SearchHeight = 20f;
        private const float FooterPadding = 4f;
        private const float DefaultRowHeight = 20f;
        private const float SeparatorHeight = 4f;

        private readonly float _width;
        private readonly float _maxHeight;
        private readonly AdvancedDropdownMetaInfo _metaInfo;
        private readonly Func<object, bool, IReadOnlyList<object>> _setValue;
        private readonly bool _allowUnSelect;
        private readonly RichTextDrawer _richTextDrawer = new RichTextDrawer();

        private readonly TreeViewState
#if UNITY_6000_2_OR_NEWER
            <int>
#endif
            _treeViewState = new TreeViewState
#if UNITY_6000_2_OR_NEWER
                <int>
#endif
                ();
        private readonly SearchField _searchField = new SearchField();

        private PopupTreeView _treeView;
        private string _search = "";
        private bool _focusSearch;

        public SaintsTreeDropdownIMGUI(AdvancedDropdownMetaInfo metaInfo, float width, float maxHeight, bool allowUnSelect, Func<object, bool, IReadOnlyList<object>> setValue)
        {
            _metaInfo = metaInfo;
            _width = width;
            _maxHeight = maxHeight;
            _allowUnSelect = allowUnSelect;
            _setValue = setValue;
        }

        public override Vector2 GetWindowSize()
        {
            float treeHeight = _treeView?.totalHeight ?? EstimateTreeHeight();
            float height = SearchHeight + FooterPadding + treeHeight;
            return new Vector2(_width, Mathf.Min(_maxHeight, height));
        }

        public override void OnOpen()
        {
            _treeView = new PopupTreeView(_treeViewState, _metaInfo, _allowUnSelect, _richTextDrawer, OnItemTriggered);
            _treeView.SetSearch(_search);
            _focusSearch = true;
        }

        public override void OnClose()
        {
        }

        public override void OnGUI(Rect rect)
        {
            if (_treeView == null)
            {
                OnOpen();
            }
            Debug.Assert(_treeView != null);

            Rect searchRect = new Rect(rect.x, rect.y, rect.width, SearchHeight);
            Rect treeRect = new Rect(rect.x, rect.y + SearchHeight, rect.width, rect.height - SearchHeight - FooterPadding);

            EditorGUI.BeginChangeCheck();
            string newSearch = _searchField.OnGUI(searchRect, _search);
            if (EditorGUI.EndChangeCheck())
            {
                _search = newSearch;
                _treeView.SetSearch(_search);
                editorWindow.Repaint();
            }

            if (_focusSearch && Event.current.type == EventType.Repaint)
            {
                _focusSearch = false;
                _searchField.SetFocus();
            }

            _treeView.OnGUI(treeRect);
        }

        public void SetSearch(string search)
        {
            _search = search ?? "";
            _treeView?.SetSearch(_search);
        }

        private IReadOnlyList<object> OnItemTriggered(object value, bool isOn, bool isPrimary)
        {
            IReadOnlyList<object> current = _setValue(value, isOn);
            if (!_allowUnSelect || isPrimary || RuntimeUtil.IsNull(current))
            {
                editorWindow.Close();
            }
            else
            {
                editorWindow.Repaint();
            }

            return current;
        }

        private float EstimateTreeHeight()
        {
            int visibleCount = _metaInfo.DropdownListValue?.Count ?? 0;
            return visibleCount * DefaultRowHeight;
        }

        private sealed class PopupTreeView : TreeView
#if UNITY_6000_2_OR_NEWER
            <int>
#endif
        {
            private enum NodeKind
            {
                Group,
                Value,
                Separator,
            }

            private sealed class NodeData
            {
                public NodeKind Kind;
                public object Value;
                public bool Disabled;
                public bool IsOn;
                public bool HasIcon;
                public RichTextDrawer.RichTextChunk[] RichChunks;
            }

            private static Texture2D _checkIcon;
            private static Texture2D _indentGuideIcon;

            private readonly AdvancedDropdownMetaInfo _metaInfo;
            private readonly bool _allowUnSelect;
            private readonly RichTextDrawer _richTextDrawer;
            private readonly Func<object, bool, bool, IReadOnlyList<object>> _onTriggered;
            private readonly RichTextDrawer.EmptyRichTextTagProvider _emptyRichTextTagProvider = new RichTextDrawer.EmptyRichTextTagProvider();

            private readonly Dictionary<int, NodeData> _nodeData = new Dictionary<int, NodeData>();

            private HashSet<object> _currentValues;
            private string _search = "";
            private int _nextId = 1;
            private int _suppressPrimaryClickId = -1;

            public PopupTreeView(TreeViewState
#if UNITY_6000_2_OR_NEWER
                <int>
#endif
                state, AdvancedDropdownMetaInfo metaInfo, bool allowUnSelect, RichTextDrawer richTextDrawer,
                Func<object, bool, bool, IReadOnlyList<object>> onTriggered) : base(state)
            {
                _metaInfo = metaInfo;
                _allowUnSelect = allowUnSelect;
                _richTextDrawer = richTextDrawer;
                _onTriggered = onTriggered;
                _currentValues = metaInfo.CurValues.ToHashSet();

                _checkIcon ??= Util.LoadResource<Texture2D>("check.png");
                _indentGuideIcon ??= Util.LoadResource<Texture2D>("tree-sep-vertical.png");

                showAlternatingRowBackgrounds = true;
                showBorder = true;
                rowHeight = DefaultRowHeight;

                Reload();
                ExpandAll();
            }

            public void SetSearch(string search)
            {
                string useSearch = search ?? "";
                if (_search == useSearch)
                {
                    return;
                }

                _search = useSearch;
                Reload();
                ExpandAll();
            }

            public void RefreshValues(IReadOnlyList<object> currentValues)
            {
                _currentValues = currentValues.ToHashSet();
                foreach (NodeData each in _nodeData.Values)
                {
                    if (each.Kind == NodeKind.Value)
                    {
                        each.IsOn = _currentValues.Contains(each.Value);
                    }
                }

                Repaint();
            }

            protected override TreeViewItem
#if UNITY_6000_2_OR_NEWER
                <int>
#endif
                BuildRoot()
            {
                _nodeData.Clear();
                _nextId = 1;

                TreeViewItem
#if UNITY_6000_2_OR_NEWER
                    <int>
#endif
                    root = new TreeViewItem
#if UNITY_6000_2_OR_NEWER
                        <int>
#endif
                        { id = 0, depth = -1, displayName = "Root" };

                List<TreeViewItem
#if UNITY_6000_2_OR_NEWER
                    <int>
#endif
                > rows = string.IsNullOrWhiteSpace(_search)
                    ? BuildTreeRows(_metaInfo.DropdownListValue.children, 0)
                    : BuildSearchRows();

                SetupParentsAndChildrenFromDepths(root, rows);
                return root;
            }

            protected override float GetCustomRowHeight(int row, TreeViewItem
#if UNITY_6000_2_OR_NEWER
                <int>
#endif
                item)
            {
                return _nodeData.TryGetValue(item.id, out NodeData nodeData) && nodeData.Kind == NodeKind.Separator
                    ? SeparatorHeight
                    : DefaultRowHeight;
            }

            protected override void SingleClickedItem(int id)
            {
                if (id == _suppressPrimaryClickId)
                {
                    _suppressPrimaryClickId = -1;
                    return;
                }

                if (!_nodeData.TryGetValue(id, out NodeData nodeData))
                {
                    return;
                }

                switch (nodeData.Kind)
                {
                    case NodeKind.Group:
                        SetExpanded(id, !IsExpanded(id));
                        break;
                    case NodeKind.Value when !nodeData.Disabled:
                        bool newValue = !_allowUnSelect || !nodeData.IsOn;
                        ApplySelection(nodeData, newValue, true);
                        break;
                }
            }

            protected override void RowGUI(RowGUIArgs args)
            {
                if (!_nodeData.TryGetValue(args.item.id, out NodeData nodeData))
                {
                    return;
                }

                if (nodeData.Kind == NodeKind.Separator)
                {
                    Rect sepRect = args.rowRect;
                    sepRect.y += (sepRect.height - 1f) * 0.5f;
                    sepRect.height = 1f;
                    EditorGUI.DrawRect(sepRect, EColor.EditorSeparator.GetColor());
                    return;
                }

                DrawIndentGuides(args);

                if (nodeData.Kind == NodeKind.Value)
                {
                    DrawValueOverlay(args, nodeData);
                }
                else
                {
                    DrawGroupOverlay(args, nodeData);
                }
            }

            private static void DrawIndentGuides(RowGUIArgs args)
            {
                if (_indentGuideIcon == null || args.item.depth <= 0)
                {
                    return;
                }

                Rect guideRect = new Rect(args.rowRect)
                {
                    width = SaintsPropertyDrawer.IndentWidth,
                };

                Color oldColor = GUI.color;
                for (int indentIndex = 0; indentIndex < args.item.depth; indentIndex++)
                {
                    guideRect.x = args.rowRect.x + indentIndex * SaintsPropertyDrawer.IndentWidth;
                    float alpha = (4 - indentIndex % 4) / 10f;
                    GUI.color = new Color(1f, 1f, 1f, alpha);
                    GUI.DrawTexture(guideRect, _indentGuideIcon, ScaleMode.StretchToFill, true);
                }
                GUI.color = oldColor;
            }

            private void DrawGroupOverlay(RowGUIArgs args, NodeData nodeData)
            {
                Rect contentRect = GetOverlayContentRect(args.item, args.rowRect);
                EditorGUI.BeginDisabledGroup(nodeData.Disabled);
                Rect labelRect = new Rect(contentRect)
                {
                    xMin = contentRect.xMin + (nodeData.HasIcon ? 14f : 0f),
                };
                _richTextDrawer.DrawChunks(labelRect, nodeData.RichChunks);
                EditorGUI.EndDisabledGroup();
            }

            private void DrawValueOverlay(RowGUIArgs args, NodeData nodeData)
            {
                Rect contentRect = GetOverlayContentRect(args.item, args.rowRect);
                Rect markerRect = new Rect(contentRect.x, contentRect.y + 1f, RichTextDrawer.ImageWidth, EditorGUIUtility.singleLineHeight);
                Rect labelRect = new Rect(contentRect)
                {
                    xMin = markerRect.xMax,
                };

                if (_allowUnSelect)
                {
                    bool clickedToggle = Event.current.type == EventType.MouseDown && markerRect.Contains(Event.current.mousePosition);

                    EditorGUI.BeginDisabledGroup(nodeData.Disabled);
                    EditorGUI.BeginChangeCheck();
                    bool newValue = EditorGUI.Toggle(markerRect, nodeData.IsOn);
                    if (EditorGUI.EndChangeCheck())
                    {
                        _suppressPrimaryClickId = clickedToggle ? args.item.id : -1;
                        ApplySelection(nodeData, newValue, false);
                    }
                    EditorGUI.EndDisabledGroup();
                }
                else if (nodeData.IsOn && _checkIcon != null && _checkIcon.width > 1 && _checkIcon.height > 1)
                {
                    GUI.DrawTexture(markerRect, _checkIcon, ScaleMode.ScaleToFit, true);
                }

                EditorGUI.BeginDisabledGroup(nodeData.Disabled);
                _richTextDrawer.DrawChunks(labelRect, nodeData.RichChunks);
                EditorGUI.EndDisabledGroup();
            }

            private void ApplySelection(NodeData nodeData, bool newValue, bool isPrimary)
            {
                if (nodeData.IsOn == newValue && _allowUnSelect)
                {
                    return;
                }

                nodeData.IsOn = newValue;
                if (newValue)
                {
                    _currentValues.Add(nodeData.Value);
                }
                else
                {
                    _currentValues.Remove(nodeData.Value);
                }

                IReadOnlyList<object> refreshed = _onTriggered(nodeData.Value, newValue, isPrimary);
                if (!_allowUnSelect || RuntimeUtil.IsNull(refreshed))
                {
                    return;
                }

                RefreshValues(refreshed);
            }

            private List<TreeViewItem
#if UNITY_6000_2_OR_NEWER
                <int>
#endif
            > BuildTreeRows(IReadOnlyList<IDropdown> dropdowns, int depth)
            {
                List<TreeViewItem
#if UNITY_6000_2_OR_NEWER
                    <int>
#endif
                > rows = new List<TreeViewItem
#if UNITY_6000_2_OR_NEWER
                    <int>
#endif
                >();

                foreach (IDropdown dropdown in dropdowns)
                {
                    if (dropdown.isSeparator)
                    {
                        rows.Add(MakeSeparator(depth));
                        continue;
                    }

                    if (dropdown.ChildCount() > 0)
                    {
                        List<TreeViewItem
#if UNITY_6000_2_OR_NEWER
                            <int>
#endif
                        > children = BuildTreeRows(dropdown.children, depth + 1);
                        if (children.Count == 0)
                        {
                            continue;
                        }

                        string richGroupLabel = string.IsNullOrEmpty(dropdown.icon)
                            ? dropdown.displayName
                            : $"<icon={dropdown.icon}/>{dropdown.displayName}";

                        TreeViewItem
#if UNITY_6000_2_OR_NEWER
                            <int>
#endif
                            groupItem = MakeItem(depth, dropdown.displayName);
                        _nodeData[groupItem.id] = new NodeData
                        {
                            Kind = NodeKind.Group,
                            HasIcon = !string.IsNullOrEmpty(dropdown.icon),
                            RichChunks = RichTextDrawer.ParseRichXmlWithProvider(richGroupLabel, _emptyRichTextTagProvider).ToArray(),
                        };
                        rows.Add(groupItem);
                        rows.AddRange(children);
                        continue;
                    }

                    string richLabel = string.IsNullOrEmpty(dropdown.icon)
                        ? dropdown.displayName
                        : $"<icon={dropdown.icon}/>{dropdown.displayName}";

                    TreeViewItem
#if UNITY_6000_2_OR_NEWER
                        <int>
#endif
                        valueItem = MakeItem(depth, dropdown.displayName);
                    _nodeData[valueItem.id] = new NodeData
                    {
                        Kind = NodeKind.Value,
                        Value = dropdown.value,
                        Disabled = dropdown.disabled,
                        IsOn = _currentValues.Contains(dropdown.value),
                        HasIcon = !string.IsNullOrEmpty(dropdown.icon),
                        RichChunks = RichTextDrawer.ParseRichXmlWithProvider(richLabel, _emptyRichTextTagProvider).ToArray(),
                    };
                    rows.Add(valueItem);
                }

                return rows;
            }

            private List<TreeViewItem
#if UNITY_6000_2_OR_NEWER
                <int>
#endif
            > BuildSearchRows()
            {
                IReadOnlyList<ListSearchToken> searchTokens = SerializedUtils.ParseSearch(_search).ToArray();
                List<TreeViewItem
#if UNITY_6000_2_OR_NEWER
                    <int>
#endif
                > rows = new List<TreeViewItem
#if UNITY_6000_2_OR_NEWER
                    <int>
#endif
                >();

                foreach (AdvancedDropdownAttributeDrawer.FlattenInfo each in AdvancedDropdownAttributeDrawer.Flatten(_metaInfo.DropdownListValue))
                {
                    string pathDisplay = string.Join("/", each.stackDisplays);
                    IEnumerable<string> searches = each.extraSearches.Append(pathDisplay).Append(each.display);
                    if (!searches.Any(search => RuntimeUtil.SimpleSearch(search, searchTokens)))
                    {
                        continue;
                    }

                    string richLabel = string.IsNullOrEmpty(each.icon)
                        ? pathDisplay
                        : $"<icon={each.icon}/>{pathDisplay}";

                    TreeViewItem
#if UNITY_6000_2_OR_NEWER
                        <int>
#endif
                        item = MakeItem(0, pathDisplay);
                    _nodeData[item.id] = new NodeData
                    {
                        Kind = NodeKind.Value,
                        Value = each.value,
                        Disabled = each.disabled,
                        IsOn = _currentValues.Contains(each.value),
                        HasIcon = !string.IsNullOrEmpty(each.icon),
                        RichChunks = RichTextDrawer.ParseRichXmlWithProvider(richLabel, _emptyRichTextTagProvider).ToArray(),
                    };
                    rows.Add(item);
                }

                return rows;
            }

            private TreeViewItem
#if UNITY_6000_2_OR_NEWER
                <int>
#endif
                MakeItem(int depth, string displayName)
            {
                return new TreeViewItem
#if UNITY_6000_2_OR_NEWER
                    <int>
#endif
                {
                    id = _nextId++,
                    depth = depth,
                    displayName = displayName,
                };
            }

            private TreeViewItem
#if UNITY_6000_2_OR_NEWER
                <int>
#endif
                MakeSeparator(int depth)
            {
                TreeViewItem
#if UNITY_6000_2_OR_NEWER
                    <int>
#endif
                    item = MakeItem(depth, "");
                _nodeData[item.id] = new NodeData
                {
                    Kind = NodeKind.Separator,
                    RichChunks = Array.Empty<RichTextDrawer.RichTextChunk>(),
                };
                return item;
            }

            private Rect GetOverlayContentRect(TreeViewItem
#if UNITY_6000_2_OR_NEWER
                <int>
#endif
                item, Rect rowRect)
            {
                Rect contentRect = rowRect;
                contentRect.xMin += GetContentIndent(item);
                contentRect.height = EditorGUIUtility.singleLineHeight;
                contentRect.y += 1f;
                return contentRect;
            }
        }
    }
}
