#if UNITY_2021_3_OR_NEWER
using System;
using System.Collections.Generic;
using SaintsField.Editor.Drawers.AdvancedDropdownDrawer;
using SaintsField.Utils;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace SaintsField.Editor.Drawers.TreeDropdownDrawer
{
    public class SaintsTreeDropdownUIToolkit : PopupWindowContent
    {
        private readonly float _width;
        private readonly AdvancedDropdownMetaInfo _metaInfo;
        private readonly Func<object, bool, IReadOnlyList<object>> _setValue;

        private readonly float _maxHeight;
        private readonly bool _allowUnSelect;

        public SaintsTreeDropdownUIToolkit(AdvancedDropdownMetaInfo metaInfo, float width, float maxHeight, bool allowUnSelect, Func<object, bool, IReadOnlyList<object>> setValue)
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
            _treeDropdownElement = new SaintsTreeDropdownElement(_metaInfo, _allowUnSelect);

            _treeDropdownElement.OnClickedEvent.AddListener(OnClicked);
            // _treeDropdownElement.RegisterCallback<GeometryChangedEvent>(GeoUpdateWindowSize);
            // editorWindow.rootVisualElement.Add(_treeDropdownElement);

            // ScrollView scrollView = new ScrollView();
            // scrollView.Add(_treeDropdownElement);

            // _treeDropdownElement.ScrollToElementEvent.AddListener(scrollView.ScrollTo);

            // scrollView.RegisterCallback<AttachToPanelEvent>(_ =>
            // {
            //     scrollView.schedule.Execute(() =>
            //     {
            //         if (_treeDropdownElement.CurrentFocus != null)
            //         {
            //             scrollView.ScrollTo(_treeDropdownElement.CurrentFocus);
            //         }
            //         // The delay is required for functional
            //     }).StartingIn(100);
            // });

            editorWindow.rootVisualElement.Add(_treeDropdownElement);
        }

        private void OnClicked(object value, bool isOn, bool isPrimary)
        {
            IReadOnlyList<object> r = _setValue(value, isOn);
            if (!_allowUnSelect || isPrimary || RuntimeUtil.IsNull(r))
            {
                editorWindow.Close();
            }
            else
            {
                _treeDropdownElement.RefreshValues(r);
            }
        }


#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_ADVANCED_DROPDOWN
        public override void OnClose()
        {
            Debug.Log("Popup closed: " + this);
        }
#endif
    }
}

#endif
