using System.Collections.Generic;
using SaintsField.Editor.Utils;
using UnityEngine.UIElements;

namespace SaintsField.Editor.UIToolkitElements
{
    public class LeftExpandButton: VisualElement, INotifyValueChanged<bool>
    {
        private static VisualTreeAsset _treeRowTemplate;

        private readonly VisualElement _icon;

        public LeftExpandButton()
        {
            _treeRowTemplate ??= Util.LoadResource<VisualTreeAsset>("UIToolkit/LeftExpandButton.uxml");
            TemplateContainer root = _treeRowTemplate.CloneTree();
            root.style.height = Length.Percent(100);
            root.style.marginBottom = 2;

            _icon = root.Q<VisualElement>(name: "Icon");

            root.Q<Button>().clicked += () =>
            {
                value = !value;
                if (_curViewDataKey != null)
                {
                    CustomViewDataKey[_curViewDataKey] = value;
                }
            };

            Add(root);
        }

        private bool _expanded;

        public void SetValueWithoutNotify(bool newValue)
        {
            _expanded = newValue;

            int rotate = _expanded ? 90 : 0;
            _icon.style.rotate = new Rotate(rotate);
        }

        public bool value
        {
            get => _expanded;
            set
            {
                if (_expanded == value)
                {
                    return;
                }

                bool previous = this.value;
                SetValueWithoutNotify(value);

                using ChangeEvent<bool> evt = ChangeEvent<bool>.GetPooled(previous, value);
                evt.target = this;
                SendEvent(evt);
            }
        }

        private static readonly Dictionary<string, bool> CustomViewDataKey = new Dictionary<string, bool>();
        private string _curViewDataKey;

        public void SetCustomViewDataKey(string customViewDataKey)
        {
            _curViewDataKey = customViewDataKey;
            if (CustomViewDataKey.TryGetValue(customViewDataKey, out bool cachedValue))
            {
                value = cachedValue;
            }
        }
    }
}
