using System.Collections.Generic;
using System.Linq;
using SaintsField.Playa;
using SaintsField.Samples.Scripts.SaintsEditor;

namespace SaintsField.Samples.Scripts
{
    public class PlayaRichLabelExample : SaintsMonoBehaviour
    {
        [LabelText("<color=lime>It's Labeled!")]
        public List<string> myList;

        [LabelText(nameof(MethodLabel), true)]
        public string[] myArray;

        // ReSharper disable once ParameterTypeCanBeEnumerable.Local
        private string MethodLabel(string[] values)
        {
            return $"<color=green><label /> {string.Join("", values.Select(_ => "<icon=star.png />"))}";
        }
    }
}
