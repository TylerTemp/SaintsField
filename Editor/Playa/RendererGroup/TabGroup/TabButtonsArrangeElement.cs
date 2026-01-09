using SaintsField.Editor.UIToolkitElements.ValueButtons;

namespace SaintsField.Editor.Playa.RendererGroup.TabGroup
{
    public class TabButtonsArrangeElement: AbsValueButtonsArrangeElement<TabButton>
    {
        public TabButtonsArrangeElement(AbsValueButtonsCalcElement<TabButton> valueButtonsCalcElement) : base(valueButtonsCalcElement, MakeRow())
        {
        }

        protected override AbsValueButtonsRow<TabButton> MakeValueButtonsRow()
        {
            return MakeRow();
        }

        private static AbsValueButtonsRow<TabButton> MakeRow()
        {
            return new TabButtonsRow();
        }
    }
}
