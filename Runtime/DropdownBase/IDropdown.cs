using System.Collections.Generic;
using UnityEngine;

namespace SaintsField.DropdownBase
{
    public interface IDropdown: IReadOnlyList<IDropdown>
    {
        IReadOnlyList<string> absolutePathFragments { get; }

        string displayName { get; }
        object value { get; }
        IReadOnlyList<IDropdown> children { get; }
        bool disabled { get; }
        bool obsolete { get; set; }
        string icon { get; }
        Color? color { get; }
        bool isSeparator { get; }

        int ChildCount();
        int SepCount();

        void SelfCompact();

        ICollection<string> ExtraSearches { get; }
    }
}
