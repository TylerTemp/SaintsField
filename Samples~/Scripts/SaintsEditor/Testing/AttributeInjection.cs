using UnityEngine;

namespace SaintsField.Samples.Scripts.SaintsEditor.Testing
{
    public class AttributeInjection : MonoBehaviour
    {
        [ValueAttribute(1, typeof(LabelTextAttribute), "$" + nameof(Nest1LabelName), true)]
        public SaintsArray<string[]> arr;

        public string Nest1LabelName(int index, string[] nest1Value)
        {
            return "Hi";
        }
    }
}
