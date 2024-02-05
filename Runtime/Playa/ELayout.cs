using System;

namespace SaintsField.Playa
{
    [Flags]
    public enum ELayout
    {
        Vertical = 0,  // default is vertical
        Horizontal = 1,
        Background = 1 << 1,  // a background for the whole layout. This is used to make it like a GroupBox
        TitleOut = 1 << 2,  // Make title looks different
        Foldout = 1 << 3,  // expandable?
        Tab = 1 << 4,  // tab page
        Title = 1 << 5,  // title
    }
}
