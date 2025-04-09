using SaintsField.Playa;

namespace SaintsField.Samples.Scripts.SaintsEditor
{
    public class PlayaSeparatorExample : SaintsMonoBehaviour
    {

        [PlayaSeparator("Separator", EAlign.Center)]
        public string separator;

        [PlayaSeparator("Left", EAlign.Start)]
        public string left;

        [PlayaSeparator("$" + nameof(right), EAlign.End)]
        public string right;

        [PlayaSeparator(EColor.Aqua)]
        [PlayaSeparator(20)]
        [PlayaSeparator("Space 20")]
        public string[] arr;

        [PlayaSeparator("End", below: true)] public string end;
    }
}
