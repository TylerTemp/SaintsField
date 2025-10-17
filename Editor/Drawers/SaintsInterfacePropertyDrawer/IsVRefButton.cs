#if UNITY_2022_2_OR_NEWER
using SaintsField.Editor.Utils;
using UnityEngine;
using UnityEngine.UIElements;

namespace SaintsField.Editor.Drawers.SaintsInterfacePropertyDrawer
{
#if UNITY_6000_0_OR_NEWER && SAINTSFIELD_UI_TOOLKIT_XUML
    [UxmlElement]
#endif
    // ReSharper disable once PartialTypeWithSinglePart
    public partial class IsVRefButton: BindableElement, INotifyValueChanged<bool>
    {
#if !UNITY_6000_0_OR_NEWER && SAINTSFIELD_UI_TOOLKIT_XUML
        public new class UxmlTraits : BindableElement.UxmlTraits { }
        public new class UxmlFactory : UxmlFactory<IsVRefButton, UxmlTraits> { }
#endif

        private static VisualTreeAsset _treeAsset;
        // private const string ClassBoth = "call-state-button-both";
        // private const string ClassRuntime = "call-state-button-runtime";

        private readonly Button _button;
        // private readonly StyleBackground _styleBackground;

        public IsVRefButton()
        {
            if(_treeAsset == null)
            {
                _treeAsset = Util.LoadResource<VisualTreeAsset>("UIToolkit/SaintsEvent/CallStateButton.uxml");
            }

            _treeAsset.CloneTree(this);
            _button = this.Q<Button>();
            _button.style.backgroundImage = null;
            _button.text = "<color=#00ffff>I</color>";
            _button.tooltip = "Unity Instance";
            // _styleBackground = _button.style.backgroundImage;

            _button.clicked += () =>
            {
                // Debug.Log($"clicked on {value}");
                value = !value;
            };
        }

        private bool _curValue;

        public void SetValueWithoutNotify(bool newValue)
        {
            // Debug.Log($"SetValueWithoutNotify {newValue}");
            _curValue = newValue;
            string newText;
            string tooltipText;
            if (newValue)
            {
                newText = "<color=#ffa500>R</color>";
                tooltipText = "Serializable Reference";
            }
            else
            {
                newText = "<color=#00ffff>I</color>";
                tooltipText = "Unity Instance";
            }
            if (_button.text != newText)
            {
                _button.text = newText;
            }
            if(_button.tooltip != tooltipText)
            {
                _button.tooltip = tooltipText;
            }
        }


        public bool value
        {
            get => _curValue;
            set
            {
                // Debug.Log($"set value {value}");
                if (value == _curValue)
                {
                    return;
                }

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
