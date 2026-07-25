using SaintsField.Playa;

namespace SaintsField.Samples.Scripts.Issue420
{
    public class Issue420Mono : SaintsMonoBehaviour
    {
        public IntWithBaseValue myInt1;
        // public IntWithBaseValue myInt2;
        // public IntWithBaseValue myInt3;
        //
        // public FloatWithBaseValue myFloat1;
        // public FloatWithBaseValue myFloat2;
        [LayoutStart("This is a Label Field layout", ELayout.LabelField)]
        [AboveText(nameof(myFloat3), paddingLeft: 0)]

        [SaintsRow(inline: true)]
        public FloatWithBaseValue myFloat3;
    }
}
