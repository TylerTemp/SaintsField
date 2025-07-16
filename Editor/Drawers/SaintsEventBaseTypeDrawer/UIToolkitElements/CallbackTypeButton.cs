#if UNITY_2021_3_OR_NEWER
using System;
using SaintsField.Editor.Utils;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UIElements;

namespace SaintsField.Editor.Drawers.SaintsEventBaseTypeDrawer.UIToolkitElements
{
#if UNITY_6000_0_OR_NEWER
    [UxmlElement]
#endif
    public partial class CallbackTypeButton: BindableElement, INotifyValueChanged<bool>
    {
        public new class UxmlTraits : BindableElement.UxmlTraits { }
        public new class UxmlFactory : UxmlFactory<CallbackTypeButton, UxmlTraits> { }

        private static VisualTreeAsset _treeAsset;

        private readonly Button _button;

        public CallbackTypeButton()
        {
            if(_treeAsset == null)
            {
                _treeAsset = Util.LoadResource<VisualTreeAsset>("UIToolkit/SaintsEvent/CallbackTypeButton.uxml");
            }

            _treeAsset.CloneTree(this);
            _button = this.Q<Button>();

            _button.clicked += () => value = !value;
        }

        private bool _curValue;

        public void SetValueWithoutNotify(bool newValue)
        {
            _curValue = newValue;
            string newText = newValue ? "<color=#ffa500>S</color>" : "I";
            if (_button.text != newText)
            {
                _button.text = newText;
            }
        }

        private bool _firstSet;

        public bool value
        {
            get => _curValue;
            set
            {
                if (value == _curValue && _firstSet)
                {
                    return;
                }

                _firstSet = true;

                bool previous = _curValue;
                SetValueWithoutNotify(value);

                using ChangeEvent<bool> evt = ChangeEvent<bool>.GetPooled(previous, value);
                evt.target = this;
                SendEvent(evt);
            }
        }
    }
}
#endif
