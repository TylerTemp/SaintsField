using System;

namespace SaintsField.Playa
{
    [Flags]
    public enum ELayout
    {
        Vertical = 0,  // default is vertical
        Horizontal = 1,
        Foldout = 1 << 1,  // expandable?
        Background = 1 << 2,  // a background for title and content. This is used to make it like a GroupBox
        ContentBackground = 1 << 3,  // a background for content only. This is used to make it like a TitleGroup
    }
}
