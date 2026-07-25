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
        [SaintsRow(inline: true)]
        [LabelText("OK")]
        public FloatWithBaseValue myFloat3;
    }
}
