namespace SaintsField.Editor.Drawers.ValueButtonsDrawer
{
    public class ValueButtonsArrangeElement: AbsValueButtonsArrangeElement<ValueButton>
    {
        public ValueButtonsArrangeElement(AbsValueButtonsCalcElement<ValueButton> valueButtonsCalcElement) : base(valueButtonsCalcElement, MakeRow())
        {
        }

        protected override AbsValueButtonsRow<ValueButton> MakeValueButtonsRow()
        {
            return MakeRow();
        }

        private static AbsValueButtonsRow<ValueButton> MakeRow()
        {
            return new ValueButtonsRow();
        }
    }
}
