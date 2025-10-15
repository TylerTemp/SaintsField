// ReSharper disable once CheckNamespace
namespace SaintsField.Playa
{
    public class PlayaSeparatorAttribute: SeparatorAttribute
    {
        public PlayaSeparatorAttribute() : base(null) {}

        public PlayaSeparatorAttribute(EColor color) : base(null, color) {}

        public PlayaSeparatorAttribute(EColor color, int space) : base(null, color, EAlign.Start, false, space) {}

        public PlayaSeparatorAttribute(EColor color, bool below) : base(null, color, EAlign.Start, false, 0, below) {}

        public PlayaSeparatorAttribute(EColor color, int space, bool below) : base(null, color, EAlign.Start, false, space, below) {}

        public PlayaSeparatorAttribute(int space) : base(null, EColor.Clear, EAlign.Start, false, space) {}

        public PlayaSeparatorAttribute(int space, bool below) : base(null, EColor.Clear, EAlign.Start, false, space, below) {}

        public PlayaSeparatorAttribute(string title, EAlign eAlign, bool isCallback = false, int space = 0, bool below = false)
            : base(title, EColor.Gray, eAlign, isCallback, space, below) {}

        public PlayaSeparatorAttribute(string title, EColor color = EColor.Gray, EAlign eAlign = EAlign.Start, bool isCallback = false, int space = 0, bool below = false)
            : base(title, color, eAlign, isCallback, space, below) {}
    }
}
