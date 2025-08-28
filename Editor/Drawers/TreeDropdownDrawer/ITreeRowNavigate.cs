using SaintsField.Editor.UIToolkitElements;

namespace SaintsField.Editor.Drawers.TreeDropdownDrawer
{
    public interface ITreeRowNavigate
    {
        ITreeRowNavigate Parent { get; }
        void MoveSlibling(TreeRowAbsElement self, bool up);
        void SetFocused();
    }
}
