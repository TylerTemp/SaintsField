using System;
using System.Linq;
using SaintsField.Editor.Core;
using SaintsField.Editor.Utils;
using UnityEditor;
using UnityEditor.UIElements;
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

    public class ObjectBaseField : BaseField<UnityEngine.Object>
    {
        public ObjectBaseField(string label, VisualElement visualInput) : base(label, visualInput)
        {
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
                ObjectBaseField baseF = new ObjectBaseField("State Machine", subStateMachineNameChain);
                baseF.AddToClassList(ObjectBaseField.alignedFieldUssClassName);
                baseF.SetEnabled(false);
                Add(baseF);
            }
        }

        private static SerializedProperty FindPropertyRelative(SerializedProperty property, string name) =>
            property.FindPropertyRelative(name) ?? SerializedUtils.FindPropertyByAutoPropertyName(property, name);

        public void SetValueWithoutNotify(AnimatorState newValue)
        {
            _cachedValue = newValue;
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
