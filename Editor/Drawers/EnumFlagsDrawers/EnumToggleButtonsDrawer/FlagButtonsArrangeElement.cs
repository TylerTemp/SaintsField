using SaintsField.Editor.Drawers.ValueButtonsDrawer;
using SaintsField.Editor.UIToolkitElements.ValueButtons;

namespace SaintsField.Editor.Drawers.EnumFlagsDrawers.EnumToggleButtonsDrawer
{
    public class FlagButtonsArrangeElement: AbsValueButtonsArrangeElement<FlagButton>
    {
        public FlagButtonsArrangeElement(AbsValueButtonsCalcElement<FlagButton> valueButtonsCalcElement) : base(valueButtonsCalcElement, MakeRow())
        {
        }

        protected override AbsValueButtonsRow<FlagButton> MakeValueButtonsRow()
        {
            return MakeRow();
        }

        private static AbsValueButtonsRow<FlagButton> MakeRow()
        {
            return new FlagButtonsRow(false);
        }
    }
}
