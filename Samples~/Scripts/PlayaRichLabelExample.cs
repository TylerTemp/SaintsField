using System.Collections.Generic;
using SaintsField.Playa;
using UnityEngine;

namespace SaintsField.Samples.Scripts
{
    public class PlayaRichLabelExample : MonoBehaviour
    {
        [PlayaRichLabel(nameof(MethodLabel), true)]
        public string[] myArray;

        private string MethodLabel(IReadOnlyList<string> values)
        {
            Debug.Log($"callback get: {values}, {values==null}");
            return $"<color=green><label />";
        }
    }
}
