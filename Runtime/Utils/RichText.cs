namespace ExtInspector.Utils
{
    public class RichText
    {
        public class RichTextPayload {}

        public class LabelPayload: RichTextPayload {}

        public class ColoredLabelPayload : LabelPayload
        {
            public readonly EColor Color;
            public ColoredLabelPayload(EColor color)
            {
                Color = color;
            }
        }

        public class TextPayload : RichTextPayload
        {
            public readonly string Text;
            public TextPayload(string text) => Text = text;
        }

        public class ColoredTextPayload : TextPayload
        {
            public readonly EColor Color;
            public ColoredTextPayload(string text, EColor color) : base(text)
            {
                Color = color;
            }
        }

        public class IconPayload : RichTextPayload
        {
            public readonly string IconResourcePath;
            public readonly bool IsEditorResource;

            public IconPayload(string iconResourcePath, bool isEditorResource)
            {
                IconResourcePath = iconResourcePath;
                IsEditorResource = isEditorResource;
            }
        }

        public class ColoredIconPayload : IconPayload
        {
            public readonly EColor Color;
            public ColoredIconPayload(string iconResourcePath, bool isEditorResource, EColor color) : base(iconResourcePath, isEditorResource)
            {
                Color = color;
            }
        }
    }
}
