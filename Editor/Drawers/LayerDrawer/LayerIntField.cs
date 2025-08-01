#if UNITY_2021_3_OR_NEWER
using SaintsField.Editor.Utils;
using UnityEngine.UIElements;

namespace SaintsField.Editor.Drawers.LayerDrawer
{
    public class LayerIntDropdown : BindableElement, INotifyValueChanged<int>
    {
        private readonly Label _label;

        private int _value = -1;

        public readonly Button Button;

        public LayerIntDropdown()
        {
            TemplateContainer dropdownElement = UIToolkitUtils.CloneDropdownButtonTree();
            dropdownElement.style.flexGrow = 1;

            Button = dropdownElement.Q<Button>();

            Button.style.flexGrow = 1;

            _label = Button.Q<Label>();

            Add(dropdownElement);
        }

        public void SetValueWithoutNotify(int newValue)
        {
            _value = newValue;

            foreach (LayerUtils.LayerInfo layerInfo in LayerUtils.GetAllLayers())
            {
                if (layerInfo.Value == newValue)
                {
                    _label.text = LayerUtils.LayerInfoLabelUIToolkit(layerInfo);
                    return;
                }

                _label.text =
                    LayerUtils.LayerInfoLabelUIToolkit(new LayerUtils.LayerInfo("<color=red>?</color>", newValue));
            }
        }

        public int value
        {
            get => _value;
            set
            {
                if (_value == value)
                {
                    return;
                }

                int previous = this.value;
                SetValueWithoutNotify(value);

                using ChangeEvent<int> evt = ChangeEvent<int>.GetPooled(previous, value);
                evt.target = this;
                SendEvent(evt);
            }
        }
    }

    public class LayerIntField: BaseField<int>
    {
        public readonly Button Button;

        public LayerIntField(string label, LayerIntDropdown layerIntDropdown) : base(label, layerIntDropdown)
        {
            Button = layerIntDropdown.Button;
        }
    }
}
#endif
