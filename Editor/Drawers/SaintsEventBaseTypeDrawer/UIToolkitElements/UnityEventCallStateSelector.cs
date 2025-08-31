#if SAINTSFIELD_SERIALIZATION && !SAINTSFIELD_SERIALIZATION_DISABLED && UNITY_2022_2_OR_NEWER && !SAINTSFIELD_UI_TOOLKIT_DISABLE
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
    // ReSharper disable once PartialTypeWithSinglePart
    public partial class UnityEventCallStateSelector: BindableElement, INotifyValueChanged<int>
    {
        public new class UxmlTraits : BindableElement.UxmlTraits { }
        public new class UxmlFactory : UxmlFactory<UnityEventCallStateSelector, UxmlTraits> { }

        private static VisualTreeAsset _treeAsset;
        private const string ClassBoth = "call-state-button-both";
        private const string ClassRuntime = "call-state-button-runtime";

        private readonly Button _button;
        private readonly StyleBackground _styleBackground;

        public UnityEventCallStateSelector()
        {
            if(_treeAsset == null)
            {
                _treeAsset = Util.LoadResource<VisualTreeAsset>("UIToolkit/SaintsEvent/CallStateButton.uxml");
            }

            _treeAsset.CloneTree(this);
            _button = this.Q<Button>();
            _styleBackground = _button.style.backgroundImage;

            _button.clicked += () =>
            {
                GenericDropdownMenu genericDropdownMenu = new GenericDropdownMenu();
                foreach ((UnityEventCallState unityEventCallState, string label) in new[]
                         {
                             (UnityEventCallState.Off, "<b>O</b>ff"),
                             (UnityEventCallState.EditorAndRuntime, "<color=#00ff00><b>E</b>ditor</color> & Runtime"),
                             (UnityEventCallState.RuntimeOnly, "<color=#00ffffff><b>R</b>untime</color> Only"),
                         })
                {
                    genericDropdownMenu.AddItem(label,
                        unityEventCallState == _curValue, () =>
                        {
                            value = (int)unityEventCallState;
                        });
                }

                Rect bound = _button.worldBound;
                if (bound.width < 150)
                {
                    bound.width = 150;
                }

                genericDropdownMenu.DropDown(bound, _button, true);
            };
        }

        private UnityEventCallState _curValue;

        public void SetValueWithoutNotify(int newValue)
        {
            _curValue = (UnityEventCallState)newValue;
            // _button.text = newValue.ToString();
            switch (_curValue)
            {
                case UnityEventCallState.Off:
                {
                    _button.text = "";
                    _button.tooltip = "Off";
                    _button.style.backgroundImage = _styleBackground;
                }
                    break;
                case UnityEventCallState.EditorAndRuntime:
                {
                    _button.text = "E";
                    _button.tooltip = "Editor & Runtime";
                    _button.style.backgroundImage = StyleKeyword.None;
                    _button.AddToClassList(ClassBoth);
                    _button.RemoveFromClassList(ClassRuntime);
                }
                    break;
                case UnityEventCallState.RuntimeOnly:
                {
                    _button.text = "R";
                    _button.tooltip = "Runtime Only";
                    _button.style.backgroundImage = StyleKeyword.None;
                    _button.AddToClassList(ClassRuntime);
                    _button.RemoveFromClassList(ClassBoth);
                }
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(newValue), newValue, null);
            }
        }

        private bool _firstSet;

        public int value
        {
            get => (int)_curValue;
            set
            {
                if (value == (int)_curValue && _firstSet)
                {
                    return;
                }

                _firstSet = true;

                UnityEventCallState previous = _curValue;
                SetValueWithoutNotify(value);

                using ChangeEvent<int> evt = ChangeEvent<int>.GetPooled((int)previous, value);
                evt.target = this;
                SendEvent(evt);
            }
        }
    }
}
#endif
