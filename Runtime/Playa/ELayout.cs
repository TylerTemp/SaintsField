using System;

namespace SaintsField.Playa
{
    [Flags]
    public enum ELayout
    {
        Vertical = 0,  // default is vertical
        Horizontal = 1,
        Background = 1 << 1,  // a background for title and content. This is used to make it like a GroupBox
        ContentBackground = 1 << 2,  // a background for content only. This is used to make it like a TitleGroup
        Foldout = 1 << 3,  // expandable?
        Tab = 1 << 4,  // tab page
        Title = 1 << 5,  // title
    }
}
