using UnityEngine;
using UnityEngine.UI;

namespace SaintsField.Samples.Scripts.IssueAndTesting.Testing
{
    public class Downpour : SaintsMonoBehaviour
    {
        [ValueAttribute(1, typeof(LabelTextAttribute), "Pool <index />")] public SaintsArray<string[]> sa;
        [ValueAttribute(1, typeof(LabelTextAttribute), "Pool <index />")]
        public SaintsArray<string>[] saOutArr;
    }
}
