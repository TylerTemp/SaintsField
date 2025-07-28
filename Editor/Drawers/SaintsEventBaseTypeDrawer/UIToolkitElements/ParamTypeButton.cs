#if SAINTSFIELD_SERIALIZATION && !SAINTSFIELD_SERIALIZATION_DISABLED && UNITY_2022_2_OR_NEWER && !SAINTSFIELD_UI_TOOLKIT_DISABLE
using System;
using SaintsField.Editor.Utils;
using SaintsField.Events;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UIElements;

namespace SaintsField.Editor.Drawers.SaintsEventBaseTypeDrawer.UIToolkitElements
{
#if UNITY_6000_0_OR_NEWER
    [UxmlElement]
#endif
    // ReSharper disable once PartialTypeWithSinglePart
    public partial class ParamTypeButton: BindableElement, INotifyValueChanged<int>
    {
        public new class UxmlTraits : BindableElement.UxmlTraits { }
        public new class UxmlFactory : UxmlFactory<ParamTypeButton, UxmlTraits> { }

        private static VisualTreeAsset _treeAsset;
        private const string ClassBoth = "call-state-button-both";
        private const string ClassRuntime = "call-state-button-runtime";

        private readonly Button _button;
        private readonly StyleBackground _styleBackground;

        public SerializedProperty IsOptionalProp { private get; set; }

        public ParamTypeButton(): this(null){}

        public ParamTypeButton(SerializedProperty isOptionalProp)
        {
            IsOptionalProp = isOptionalProp;
            if(_treeAsset == null)
            {
                _treeAsset = Util.LoadResource<VisualTreeAsset>("UIToolkit/SaintsEvent/CallStateButton.uxml");
            }

            _treeAsset.CloneTree(this);
            _button = this.Q<Button>();
            _styleBackground = _button.style.backgroundImage;

            // SerializedProperty isOptionalProp = persistentArgumentProp.FindPropertyRelative(nameof(PersistentArgument.isOptional));
            // SerializedProperty valueType =
            //     persistentArgumentProp.FindPropertyRelative(nameof(PersistentArgument.valueType));

            _button.clicked += () =>
            {
                GenericDropdownMenu genericDropdownMenu = new GenericDropdownMenu();

                genericDropdownMenu.AddItem("<color=#00ffffff><b>D</b>ynamic</color>", value == (int) PersistentArgument.CallType.Dynamic, () => value = (int) PersistentArgument.CallType.Dynamic);
                genericDropdownMenu.AddItem("<color=#00ff00><b>S</b>erialized</color>", value == (int) PersistentArgument.CallType.Serialized, () => value = (int) PersistentArgument.CallType.Serialized);

                genericDropdownMenu.AddSeparator("");
                const string defaultLabel = "<color=#ffffff><b>U</b></color>se Default";
                if (IsOptionalProp.boolValue)
                {
                    genericDropdownMenu.AddItem(defaultLabel, value == (int) PersistentArgument.CallType.OptionalDefault, () => value = (int) PersistentArgument.CallType.OptionalDefault);
                }
                else
                {
                    genericDropdownMenu.AddDisabledItem(defaultLabel, false);
                }

                Rect bound = _button.worldBound;
                if (bound.width < 150)
                {
                    bound.width = 150;
                }

                genericDropdownMenu.DropDown(bound, _button, true);
            };
        }

        private PersistentArgument.CallType _curValue;

        public void SetValueWithoutNotify(int newValue)
        {
            _curValue = (PersistentArgument.CallType)newValue;
            // _button.text = newValue.ToString();
            switch (_curValue)
            {
                case PersistentArgument.CallType.Dynamic:
                {
                    _button.text = "D";
                    _button.tooltip = "Dynamic";
                    _button.style.backgroundImage = StyleKeyword.None;
                    _button.AddToClassList(ClassRuntime);
                    _button.RemoveFromClassList(ClassBoth);
                }
                    break;
                case PersistentArgument.CallType.Serialized:
                {
                    _button.text = "S";
                    _button.tooltip = "Serialized";
                    _button.style.backgroundImage = StyleKeyword.None;
                    _button.AddToClassList(ClassBoth);
                    _button.RemoveFromClassList(ClassRuntime);
                }
                    break;
                case PersistentArgument.CallType.OptionalDefault:
                {
                    _button.text = "";
                    _button.tooltip = "Use Default";
                    _button.style.backgroundImage = _styleBackground;
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

                PersistentArgument.CallType previous = _curValue;
                SetValueWithoutNotify(value);

                using ChangeEvent<int> evt = ChangeEvent<int>.GetPooled((int)previous, value);
                evt.target = this;
                SendEvent(evt);
            }
        }
    }
}
#endif
