#if UNITY_2021_3_OR_NEWER
using SaintsField.Editor.Utils;
using UnityEngine.UIElements;

namespace SaintsField.Editor.Drawers.LayerDrawer
{
    public class LayerStringDropdown : BindableElement, INotifyValueChanged<string>
    {
        private readonly Label _label;

        private string _value = "";

        public readonly Button Button;

        public LayerStringDropdown()
        {
            TemplateContainer dropdownElement = UIToolkitUtils.CloneDropdownButtonTree();
            dropdownElement.style.flexGrow = 1;

            Button = dropdownElement.Q<Button>();

            Button.style.flexGrow = 1;

            _label = Button.Q<Label>();

            Add(dropdownElement);
        }

        public void SetValueWithoutNotify(string newValue)
        {
            _value = newValue;

            foreach (LayerUtils.LayerInfo layerInfo in LayerUtils.GetAllLayers())
            {
                if (layerInfo.Name == newValue)
                {
                    _label.text = LayerUtils.LayerInfoLabelUIToolkit(layerInfo);
                    return;
                }

                _label.text = $"<color=red>?</color> {newValue}";
            }
        }

        public string value
        {
            get => _value;
            set
            {
                if (_value == value)
                {
                    return;
                }

                string previous = this.value;
                SetValueWithoutNotify(value);

                using ChangeEvent<string> evt = ChangeEvent<string>.GetPooled(previous, value);
                evt.target = this;
                SendEvent(evt);
            }
        }
    }

    public class LayerStringField: BaseField<string>
    {
        public readonly Button Button;

        public LayerStringField(string label, LayerStringDropdown layerIntDropdown) : base(label, layerIntDropdown)
        {
            Button = layerIntDropdown.Button;
        }
    }
}
#endif
