namespace SaintsField.Utils
{
    public static class RuntimeUtil
    {
        public static (string content, bool isCallback) ParseCallback(string content, bool isCallback=false)
        {
            if (isCallback || content is null)
            {
                return (content, isCallback);
            }

            if (content.StartsWith("\\"))
            {
                return (content.Substring(1, content.Length - 1), false);
            }

            if (content.StartsWith("$"))
            {
                return (content.Substring(1, content.Length - 1), true);
            }

            return (content, false);
        }
    }
}
