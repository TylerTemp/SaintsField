namespace SaintsField.Playa
{
    public readonly struct ListSearchToken
    {
        public readonly ListSearchType Type;
        public readonly string Token;

        public ListSearchToken(ListSearchType type, string token)
        {
            Type = type;
            Token = token;
        }
    }
}
