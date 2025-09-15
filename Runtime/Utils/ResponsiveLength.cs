namespace SaintsField.Utils
{
    public readonly struct ResponsiveLength
    {
        public readonly Utils.ResponsiveType Type;
        public readonly float Value;

        public ResponsiveLength(Utils.ResponsiveType type, float value)
        {
            Value = value;
            Type = type;
        }
    }
}
