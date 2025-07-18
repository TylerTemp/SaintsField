#if UNITY_2021_3_OR_NEWER
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

                genericDropdownMenu.AddItem("<color=#00ffffff><b>D</b>ynamic</color>", value == (int) PersistentArgument.ValueType.Dynamic, () => value = (int) PersistentArgument.ValueType.Dynamic);
                genericDropdownMenu.AddItem("<color=#00ff00><b>S</b>erialized</color>", value == (int) PersistentArgument.ValueType.Serialized, () => value = (int) PersistentArgument.ValueType.Serialized);

                genericDropdownMenu.AddSeparator("");
                const string defaultLabel = "<b>U</b>se Default";
                if (IsOptionalProp.boolValue)
                {
                    genericDropdownMenu.AddItem(defaultLabel, value == (int) PersistentArgument.ValueType.OptionalDefault, () => value = (int) PersistentArgument.ValueType.OptionalDefault);
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

        private PersistentArgument.ValueType _curValue;

        public void SetValueWithoutNotify(int newValue)
        {
            _curValue = (PersistentArgument.ValueType)newValue;
            // _button.text = newValue.ToString();
            switch (_curValue)
            {
                case PersistentArgument.ValueType.Dynamic:
                {
                    _button.text = "D";
                    _button.style.backgroundImage = StyleKeyword.None;
                    _button.AddToClassList(ClassRuntime);
                    _button.RemoveFromClassList(ClassBoth);
                }
                    break;
                case PersistentArgument.ValueType.Serialized:
                {
                    _button.text = "S";
                    _button.style.backgroundImage = StyleKeyword.None;
                    _button.AddToClassList(ClassBoth);
                    _button.RemoveFromClassList(ClassRuntime);
                }
                    break;
                case PersistentArgument.ValueType.OptionalDefault:
                {
                    _button.text = "";
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

                PersistentArgument.ValueType previous = _curValue;
                SetValueWithoutNotify(value);

                using ChangeEvent<int> evt = ChangeEvent<int>.GetPooled((int)previous, value);
                evt.target = this;
                SendEvent(evt);
            }
        }
    }
}
#endif
