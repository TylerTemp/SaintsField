using SaintsField.Playa;

namespace SaintsField.Samples.Scripts.SaintsEditor
{
    public class PlayaSeparatorExample : SaintsMonoBehaviour
    {

        [Separator("Separator", EAlign.Center)]
        public string separator;

        [Separator("Left", EAlign.Start)]
        public string left;

        [Separator("$" + nameof(right), EAlign.End)]
        public string right;

        [Separator(EColor.Aqua)]
        [Separator(20)]
        [Separator("Space 20")]
        public string[] arr;

        [Separator("End", below: true)] public string end;
    }
}
