using SaintsField.Playa;
using UnityEngine;

namespace SaintsField.Samples.Scripts.SaintsEditor.Testing
{
    public class AttributeInjection : SaintsMonoBehaviour
    {
        [ValueAttribute(1, typeof(LabelTextAttribute), "$" + nameof(Nest1LabelName), true)]
        // [SaintsArray(numberOfItemsPerPage: 5)]
        // [SaintsArray]
        public SaintsArray<string[]> arr;

        public string Nest1LabelName(int index, string[] nest1Value)
        {
            return "Hi";
        }
    }
}
