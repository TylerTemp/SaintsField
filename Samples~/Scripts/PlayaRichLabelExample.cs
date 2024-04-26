using System.Collections.Generic;
using System.Linq;
using SaintsField.Playa;
using UnityEngine;

namespace SaintsField.Samples.Scripts
{
    public class PlayaRichLabelExample : MonoBehaviour
    {
        [PlayaRichLabel("<color=lame>It's Labeled!")]
        public List<string> myList;

        [PlayaRichLabel(nameof(MethodLabel), true)]
        public string[] myArray;

        // ReSharper disable once ParameterTypeCanBeEnumerable.Local
        private string MethodLabel(string[] values)
        {
            return $"<color=green><label /> {string.Join("", values.Select(_ => "<icon=star.png />"))}";
        }
    }
}
