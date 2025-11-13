using System;
using System.Linq;
using SaintsField.Editor.Core;
using SaintsField.Editor.Utils;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace SaintsField.Editor.Drawers.AnimatorStateDrawer
{
    public class SubStateMachineNameChain : BindableElement, INotifyValueChanged<string[]>
    {
        private readonly TextField _textField;

        public SubStateMachineNameChain()
        {
            Add(_textField = new TextField());
        }

        private string[] _cachedValue = Array.Empty<string>();

        public void SetValueWithoutNotify(string[] newValue)
        {
            _cachedValue = newValue;
            _textField.SetValueWithoutNotify(string.Join(" > ", newValue));
        }

        public string[] value
        {
            get => _cachedValue;
            set
            {
                if (_cachedValue.Length == value.Length && _cachedValue.SequenceEqual(value))
                {
                    return;
                }

                string[] previous = this.value;
                SetValueWithoutNotify(value);

                using ChangeEvent<string[]> evt = ChangeEvent<string[]>.GetPooled(previous, value);
                evt.target = this;
                SendEvent(evt);
            }
        }

        public void TrackProp(SerializedProperty arrayProp)
        {
            this.TrackPropertyValue(arrayProp, _ =>
            {
                value = Enumerable.Range(0, arrayProp.arraySize)
                    .Select(each => arrayProp.GetArrayElementAtIndex(each).stringValue).ToArray();
            });
        }
    }

    public class SubStateMachineNameChainField : BaseField<string[]>
    {
        public readonly SubStateMachineNameChain SubStateMachineNameChain;
        public SubStateMachineNameChainField(string label, SubStateMachineNameChain visualInput) : base(label, visualInput)
        {
            SubStateMachineNameChain = visualInput;
        }

        public override void SetValueWithoutNotify(string[] newValue)
        {
            SubStateMachineNameChain.SetValueWithoutNotify(newValue);
        }

        public override string[] value
        {
            get => SubStateMachineNameChain.value;
            set => SubStateMachineNameChain.value = value;
        }
    }

    public class AnimatorStateDetailPanel: BindableElement, INotifyValueChanged<AnimatorState>
    {
        private AnimatorState _cachedValue;

        public AnimatorStateDetailPanel()
        {
            style.backgroundColor = EColor.CharcoalGray.GetColor();
            style.paddingLeft = SaintsPropertyDrawer.IndentWidth;
        }

        public void BindStructProperty(SerializedProperty property)
        {
            foreach (SerializedProperty serializedProperty in new[]
                         {
                             "layerIndex",
                             "stateName",
                             "stateNameHash",
                             "stateSpeed",
                             "stateTag",
                             "animationClip",
                             // "subStateMachineNameChain",
                         }
                         .Select(each => FindPropertyRelative(property, each))
                         .Where(each => each != null))
            {
                PropertyField subField = new PropertyField(serializedProperty,
                    ObjectNames.NicifyVariableName(serializedProperty.displayName));
                subField.Bind(property.serializedObject);
                subField.SetEnabled(false);
                Add(subField);
            }

            SerializedProperty subStateMachineNameChainProp = FindPropertyRelative(property, "subStateMachineNameChain");
            if(subStateMachineNameChainProp != null)
            {
                SubStateMachineNameChain subStateMachineNameChain = new SubStateMachineNameChain
                {
                    bindingPath = subStateMachineNameChainProp.propertyPath,
                };
                subStateMachineNameChain.TrackProp(subStateMachineNameChainProp);
                SubStateMachineNameChainField baseF = new SubStateMachineNameChainField("State Machine", subStateMachineNameChain);
                baseF.AddToClassList(SubStateMachineNameChainField.alignedFieldUssClassName);
                baseF.SetEnabled(false);
                Add(baseF);
            }
        }

        private bool _init;
        private IntegerField _layerIndexField;
        private TextField _stateNameField;
        private IntegerField _stateNameHashField;
        private FloatField _stateSpeedField;
        private TextField _stateTagField;
        private ObjectField _animationClipField;
        private SubStateMachineNameChain _stateMachineField;

        public void UpdateStruct(AnimatorState newState)
        {
            if (!_init)
            {
                _init = true;
                Add(_layerIndexField = new IntegerField("Layer Index")
                {
                    value = newState.layerIndex,
                });
                Add(_stateNameField = new TextField("State Name")
                {
                    value = newState.stateName,
                });
                Add(_stateNameHashField = new IntegerField("State Name Hash")
                {
                    value = newState.stateNameHash,
                });
                Add(_stateSpeedField = new FloatField("State Speed")
                {
                    value = newState.stateSpeed,
                });
                Add(_stateTagField = new TextField("State Tag")
                {
                    value = newState.stateTag,
                });
                Add(_animationClipField = new ObjectField("Animation Clip")
                {
                    objectType = typeof(AnimationClip),
                    value = newState.animationClip,
                });
                Add(_stateMachineField = new SubStateMachineNameChain()
                {
                    value = newState.subStateMachineNameChain ?? Array.Empty<string>(),
                });
            }

            SetValueWithoutNotify(newState);
        }

        private static SerializedProperty FindPropertyRelative(SerializedProperty property, string name) =>
            property.FindPropertyRelative(name) ?? SerializedUtils.FindPropertyByAutoPropertyName(property, name);

        public void SetValueWithoutNotify(AnimatorState newValue)
        {
            _cachedValue = newValue;

            if(_init)
            {
                _layerIndexField.SetValueWithoutNotify(newValue.layerIndex);
                _stateNameField.SetValueWithoutNotify(newValue.stateName ?? string.Empty);
                _stateNameHashField.SetValueWithoutNotify(newValue.stateNameHash);
                _stateSpeedField.SetValueWithoutNotify(newValue.stateSpeed);
                _stateTagField.SetValueWithoutNotify(newValue.stateTag ?? string.Empty);
                _animationClipField.SetValueWithoutNotify(newValue.animationClip);
                _stateMachineField.SetValueWithoutNotify(newValue.subStateMachineNameChain ?? Array.Empty<string>());
            }
        }

        public AnimatorState value
        {
            get => _cachedValue;
            set
            {
                if (_cachedValue == value)
                {
                    return;
                }

                AnimatorState previous = this.value;
                SetValueWithoutNotify(value);

                using ChangeEvent<AnimatorState> evt = ChangeEvent<AnimatorState>.GetPooled(previous, value);
                evt.target = this;
                SendEvent(evt);
            }
        }
    }
}
