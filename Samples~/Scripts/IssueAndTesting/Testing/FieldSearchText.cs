using SaintsField.Playa;

namespace SaintsField.Samples.Scripts.IssueAndTesting.Testing
{
    public class FieldSearchText : SaintsMonoBehaviour
    {
        [LayoutStart("Search This", ELayout.TitleBox)]

        [InfoBox("Search This")]
        [InfoBox("Not That")]
        [Separator]  // always hide when search
        [Separator("Search This")]
        [Separator("Not That")]
        [AboveText("Search This")]
        [BelowText("Not That")]
        public string searchText;

        [Separator]
        public string notThat;

        [LayoutStart("Tab Search", ELayout.Tab)]

        [LayoutStart("./Tab Main")]
        public string main1;
        public string main2;

        [LayoutStart("../Tab Second")]
        public string second1;
        public string second2;
    }
}
