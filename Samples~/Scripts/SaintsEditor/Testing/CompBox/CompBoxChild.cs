namespace SaintsField.Samples.Scripts.SaintsEditor.Testing.CompBox
{

    [CompInfoBox("This is always at top", EMessageType.None)]
    [CompInfoBox("$" + nameof(dynamicContent))]
    [CompText("<color=gray>Text is also always at top")]
    [CompText("$" + nameof(dynamicContent))]
    public class CompBoxChild : CompBoxParent
    {
        public string dynamicContent;
    }
}
