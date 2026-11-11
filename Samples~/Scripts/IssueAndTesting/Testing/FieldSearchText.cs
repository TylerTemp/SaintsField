namespace SaintsField.Samples.Scripts.IssueAndTesting.Testing
{
    public class FieldSearchText : SaintsMonoBehaviour
    {
        [InfoBox("Search This")]
        [InfoBox("Not That")]
        [Separator]  // always hide when search
        [Separator("Search This")]
        [Separator("Not That")]
        [AboveText("Search This")]
        [BelowText("Not That")]
        public string searchText;
    }
}
