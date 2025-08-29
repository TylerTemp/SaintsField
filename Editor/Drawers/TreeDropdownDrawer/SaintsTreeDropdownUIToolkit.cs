#if UNITY_2021_3_OR_NEWER
using System;
using SaintsField.Editor.Drawers.AdvancedDropdownDrawer;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace SaintsField.Editor.Drawers.TreeDropdownDrawer
{
    public class SaintsTreeDropdownUIToolkit : PopupWindowContent
    {
        private readonly float _width;
        private readonly AdvancedDropdownMetaInfo _metaInfo;
        private readonly Action<object, bool> _setValue;

        private readonly float _maxHeight;
        private readonly bool _allowUnSelect;

        public SaintsTreeDropdownUIToolkit(AdvancedDropdownMetaInfo metaInfo, float width, float maxHeight, bool allowUnSelect, Action<object, bool> setValue)
        {
            _width = width;
            _metaInfo = metaInfo;
            _setValue = setValue;
            _maxHeight = maxHeight;
            _allowUnSelect = allowUnSelect;
        }

        public override void OnGUI(Rect rect)
        {
            // Intentionally left empty
        }

        //Set the window size
        public override Vector2 GetWindowSize()
        {
            return new Vector2(_width, Mathf.Min(_maxHeight, _treeDropdownElement.GetMaxHeight()));
        }

        private SaintsTreeDropdownElement _treeDropdownElement;

        public override void OnOpen()
        {
            _treeDropdownElement = CloneTree();

            _treeDropdownElement.OnClickedEvent.AddListener(OnClicked);
            // _treeDropdownElement.RegisterCallback<GeometryChangedEvent>(GeoUpdateWindowSize);
            // editorWindow.rootVisualElement.Add(_treeDropdownElement);

            ScrollView scrollView = new ScrollView();
            scrollView.Add(_treeDropdownElement);
            editorWindow.rootVisualElement.Add(scrollView);
        }

        private void OnClicked(object value, bool isOn, bool isPrimary)
        {
            _setValue(value, isOn);
            if (isPrimary)
            {
                editorWindow.Close();
            }
        }

        private SaintsTreeDropdownElement CloneTree()
        {
            return new SaintsTreeDropdownElement(_metaInfo, _allowUnSelect);
        }


        // // Yep, hack around...
        // private void GeoUpdateWindowSize(GeometryChangedEvent evt)
        // {
        //     // var root = editorWindow.rootVisualElement;
        //     // ScrollView scrollView = editorWindow.rootVisualElement.Q<ScrollView>();
        //     // VisualElement contentContainer = scrollView.contentContainer;
        //     // // var height = contentContainer.resolvedStyle.height;
        //     //
        //     // VisualElement toolbarSearchContainer = editorWindow.rootVisualElement.Q<VisualElement>("saintsfield-advanced-dropdown-search-container");
        //     // ToolbarBreadcrumbs toolbarBreadcrumbs = editorWindow.rootVisualElement.Q<ToolbarBreadcrumbs>();
        //     //
        //     // float height = contentContainer.resolvedStyle.height + toolbarSearchContainer.resolvedStyle.height + toolbarBreadcrumbs.resolvedStyle.height + 8;
        //     // float height = _treeDropdownElement.resolvedStyle.height;
        //
        //     // editorWindow.maxSize = editorWindow.minSize = new Vector2(_width, Mathf.Min(height, _maxHeight));
        // }

#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_ADVANCED_DROPDOWN
        public override void OnClose()
        {
            Debug.Log("Popup closed: " + this);
        }
#endif
    }
}

#endif
