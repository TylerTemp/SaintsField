#if UNITY_2021_3_OR_NEWER
using SaintsField.Editor.UIToolkitElements.ValueButtons;

namespace SaintsField.Editor.Playa.RendererGroup.TabGroup
{
    public class TabButtonsArrangeElement: AbsValueButtonsArrangeElement<TabButton>
    {
        public TabButtonsArrangeElement(AbsValueButtonsCalcElement valueButtonsCalcElement) : base(valueButtonsCalcElement, MakeRow())
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
#endif
