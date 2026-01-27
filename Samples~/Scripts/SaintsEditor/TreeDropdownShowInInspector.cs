using SaintsField.Playa;

namespace SaintsField.Samples.Scripts.SaintsEditor
{
    public class TreeDropdownShowInInspector : SaintsMonoBehaviour
    {
        private string _selectItem;

        [ShowInInspector, Dropdown(nameof(GetItems))]
        public string SelectedItem
        {
            get => _selectItem;
            set => _selectItem = value;
        }

        [ShowInInspector]
        [Button]
        [Dropdown(nameof(GetItems))]  // change the returned value display
        private string SelectItemWithButton([Dropdown(nameof(GetItems))] string item)
        {
            return item;
        }

        private AdvancedDropdownList<string> GetItems()
        {
            return new AdvancedDropdownList<string>
            {
                {"<color=Aquamarine>Sub/Item 1", "Value 1"},
                {"<color=Aquamarine>Sub/Item 2", "Value 2"},
                {"<color=Aquamarine>Sub/Item 3", "Value 3"},
                {"Item 4", "Value 4"},
                {"Item 5", "Value 5"},
            };
        }
    }
}
