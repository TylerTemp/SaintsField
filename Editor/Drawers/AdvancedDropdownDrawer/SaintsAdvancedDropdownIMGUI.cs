using System;
using System.Collections.Generic;
using SaintsField.DropdownBase;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace SaintsField.Editor.Drawers.AdvancedDropdownDrawer
{
    public class SaintsAdvancedDropdownIMGUI : AdvancedDropdown
    {
        private readonly IAdvancedDropdownList _dropdownListValue;

        private readonly Dictionary<AdvancedDropdownItem, object> _itemToValue = new Dictionary<AdvancedDropdownItem, object>();
        private readonly Action<object> _setValueCallback;
        private readonly Func<string, Texture2D> _getIconCallback;
        private readonly Rect _showRect;

        public SaintsAdvancedDropdownIMGUI(IAdvancedDropdownList dropdownListValue, Vector2 size, Rect showRect, AdvancedDropdownState state, Action<object> setValueCallback, Func<string, Texture2D> getIconCallback) : base(state)
        {
            _dropdownListValue = dropdownListValue;
            _setValueCallback = setValueCallback;
            _getIconCallback = getIconCallback;
            _showRect = showRect;

            minimumSize = size;
        }

        protected override AdvancedDropdownItem BuildRoot()
        {
            AdvancedDropdownItem root = MakeUnityAdvancedDropdownItem(_dropdownListValue);

            if(_dropdownListValue.children.Count == 0)
            {
                // root.AddChild(new UnityAdvancedDropdownItem("Empty"));
                return root;
            }

            MakeChildren(root, _dropdownListValue.children);

            return root;
        }

        private AdvancedDropdownItem MakeUnityAdvancedDropdownItem(IAdvancedDropdownList item)
        {
            // if (item.isSeparator)
            // {
            //     return new UnityAdvancedDropdownItem("SEPARATOR");
            // }

            return new AdvancedDropdownItem(item.displayName)
            {
                icon = string.IsNullOrEmpty(item.icon) ? null : _getIconCallback(item.icon),
                enabled = !item.disabled,
            };
        }

        private void MakeChildren(AdvancedDropdownItem parent, IEnumerable<IAdvancedDropdownList> children)
        {
            foreach (IAdvancedDropdownList childItem in children)
            {
                if (childItem.isSeparator)
                {
                    parent.AddSeparator();
                }
                else if (childItem.children.Count == 0)
                {
                    // Debug.Log($"{parent.name}/{childItem.displayName}");
                    AdvancedDropdownItem item = MakeUnityAdvancedDropdownItem(childItem);
                    _itemToValue[item] = childItem.value;
                    // Debug.Log($"add {childItem.displayName} => {childItem.value}");
                    parent.AddChild(item);
                }
                else
                {
                    AdvancedDropdownItem subParent = MakeUnityAdvancedDropdownItem(childItem);
                    // Debug.Log($"{parent.name}/{childItem.displayName}[...]");
                    MakeChildren(subParent, childItem.children);
                    parent.AddChild(subParent);
                }
            }
        }

        protected override void ItemSelected(AdvancedDropdownItem item)
        {
            if (!item.enabled)  // WTF Unity?
            {
                // Show(new Rect(_showRect)
                // {
                //     y = 0,
                //     height = 0,
                // });
                // Show(new Rect(_showRect)
                // {
                //     x = 0,
                //     y = -_showRect.y - _showRect.height,
                //     height = 0,
                // });

                // ReSharper disable once InvertIf
                if(_bindWindowPos)
                {
                    Show(_showRect);
                    EditorWindow curFocusedWindow = EditorWindow.focusedWindow;
                    if (curFocusedWindow == null || curFocusedWindow.GetType().ToString() !=
                        "UnityEditor.IMGUI.Controls.AdvancedDropdownWindow")
                    {
                        return;
                    }
                    curFocusedWindow.position = _windowPosition;
                }

                return;
            }

            // Debug.Log($"select {item.name}: {(_itemToValue.TryGetValue(item, out object r) ? r.ToString() : "[NULL]")}");
            if (_itemToValue.TryGetValue(item, out object result))
            {
                _setValueCallback(result);
            }
        }

        private bool _bindWindowPos;
        // private EditorWindow _thisEditorWindow;
        private Rect _windowPosition;

        // hack for Unity allow to click on disabled item...
        public void BindWindowPosition()
        {
            if (_bindWindowPos)
            {
                return;
            }

            EditorWindow window = EditorWindow.focusedWindow;
            if (window == null || window.GetType().ToString() != "UnityEditor.IMGUI.Controls.AdvancedDropdownWindow")
            {
                return;
            }

            _bindWindowPos = true;
            _windowPosition = window.position;
        }
    }
}
