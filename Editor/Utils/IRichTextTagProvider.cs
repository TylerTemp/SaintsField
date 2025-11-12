namespace SaintsField.Editor.Utils
{
    public interface IRichTextTagProvider
    {
        string GetLabel();
        string GetContainerType();
        string GetContainerTypeBaseType();
        string GetIndex(string formatter);
        string GetField(string rawContent, string tagName, string tagValue);
    }
}
