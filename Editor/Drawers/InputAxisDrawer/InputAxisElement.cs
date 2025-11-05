#if UNITY_2021_3_OR_NEWER
using System;
using SaintsField.Editor.Drawers.AdvancedDropdownDrawer;
using SaintsField.Editor.Drawers.TreeDropdownDrawer;
using SaintsField.Editor.UIToolkitElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace SaintsField.Editor.Drawers.InputAxisDrawer
{
    public class InputAxisElement: StringDropdownElement
    {
        private VisualElement _boundElement;

        public void BindBound(VisualElement target) => _boundElement = target;

        public InputAxisElement()
        {
            Button.clicked += MakeDropdown;
        }

        public override void SetValueWithoutNotify(string newValue)
        {
            CachedValue = newValue;

            // ReSharper disable once LoopCanBeConvertedToQuery
            foreach (string axisName in InputAxisUtils.GetAxisNames())
            {
                // ReSharper disable once InvertIf
                if (axisName == newValue)
                {
                    SetLabelString(newValue);
                    return;
                }
            }

            SetLabelString(string.IsNullOrEmpty(newValue)? "": $"<color=red>?</color> ({newValue})");
        }

        private void MakeDropdown()
        {
            AdvancedDropdownList<string> dropdown = new AdvancedDropdownList<string>();
            dropdown.Add("[Empty String]", string.Empty);
            dropdown.AddSeparator();

            string selectedName = null;
            foreach (string axisName in InputAxisUtils.GetAxisNames())
            {
                dropdown.Add(axisName, axisName);
                // ReSharper disable once InvertIf
                if (axisName == value)
                {
                    selectedName = axisName;
                }
            }

            dropdown.AddSeparator();
            dropdown.Add("Open Input Manager...", null, false, "d_editicon.sml");

            AdvancedDropdownMetaInfo metaInfo = new AdvancedDropdownMetaInfo
            {
                CurValues = selectedName is null ? Array.Empty<object>(): new object[] { selectedName },
                DropdownListValue = dropdown,
                SelectStacks = Array.Empty<AdvancedDropdownAttributeDrawer.SelectStack>(),
            };

            VisualElement root = _boundElement ?? this;

            (Rect dropdownWorldBound, float maxHeight) = SaintsAdvancedDropdownUIToolkit.GetProperPos(root.worldBound);

            SaintsTreeDropdownUIToolkit sa = new SaintsTreeDropdownUIToolkit(
                metaInfo,
                root.worldBound.width,
                maxHeight,
                false,
                (curItem, _) =>
                {
                    string curValue = (string)curItem;
                    if (curValue == null)
                    {
                        InputAxisUtils.OpenInputManager();
                        return null;
                    }

                    value = curValue;
                    return new[] { curValue };
                }
            );

            UnityEditor.PopupWindow.Show(dropdownWorldBound, sa);
        }

    }

    public class InputAxisField : BaseField<string>
    {
        private readonly InputAxisElement _inputAxisElement;

        public InputAxisField(string label, InputAxisElement visualInput) : base(label, visualInput)
        {
            visualInput.BindBound(this);
            _inputAxisElement = visualInput;
        }

        public override void SetValueWithoutNotify(string newValue)
        {
            _inputAxisElement.SetValueWithoutNotify(newValue);
        }

        public override string value
        {
            get => _inputAxisElement.value;
            set => _inputAxisElement.value = value;
        }
    }
}
#endif
