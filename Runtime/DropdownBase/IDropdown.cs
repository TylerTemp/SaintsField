using System.Collections.Generic;

namespace SaintsField.DropdownBase
{
    public interface IDropdown: IReadOnlyList<IDropdown>
    {
        IReadOnlyList<string> absolutePathFragments { get; }

        string displayName { get; }
        object value { get; }
        IReadOnlyList<IDropdown> children { get; }
        bool disabled { get; }
        string icon { get; }
        bool isSeparator { get; }

        int ChildCount();
        int SepCount();

        void SelfCompact();

        ICollection<string> ExtraSearches { get; }
    }
}
