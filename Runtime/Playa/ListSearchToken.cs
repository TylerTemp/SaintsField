namespace SaintsField.Playa
{
    public readonly struct ListSearchToken
    {
        public readonly ListSearchType Type;  // filter type
        public readonly string Token;  // search string

        public ListSearchToken(ListSearchType type, string token)
        {
            Type = type;
            Token = token;
        }
    }
}
