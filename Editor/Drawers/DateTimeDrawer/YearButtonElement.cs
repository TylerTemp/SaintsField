#if UNITY_2021_3_OR_NEWER
using SaintsField.Editor.Utils;
using UnityEngine.UIElements;

namespace SaintsField.Editor.Drawers.DateTimeDrawer
{

#if UNITY_6000_0_OR_NEWER && SAINTSFIELD_UI_TOOLKIT_XUML
    [UxmlElement]
#endif
    // ReSharper disable once PartialTypeWithSinglePart
    public partial class YearButtonElement: VisualElement
    {
        public readonly Button Button;
        private static VisualTreeAsset _containerTree;

        // ReSharper disable once MemberCanBePrivate.Global
        // ReSharper disable once UnusedMember.Global
        public YearButtonElement(): this(false, 0){}

        public YearButtonElement(int year) : this(true, year){}

        private YearButtonElement(bool hasYear, int year)
        {
            _containerTree ??= Util.LoadResource<VisualTreeAsset>("UIToolkit/DateTime/YearButton.uxml");
            VisualElement element = _containerTree.CloneTree();

            Button = element.Q<Button>("year-button");
            Button.text = hasYear? year.ToString(): "";

            Add(element);
        }

        public void SetSelected(bool active)
        {
            if (active)
            {
                Button.AddToClassList("selected");
            }
            else
            {
                Button.RemoveFromClassList("selected");
            }
        }

        // public override VisualElement contentContainer => Button;
    }
}
#endif
