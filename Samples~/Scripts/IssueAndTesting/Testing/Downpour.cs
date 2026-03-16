using UnityEngine;

namespace SaintsField.Samples.Scripts.IssueAndTesting.Testing
{
    public class Downpour : SaintsMonoBehaviour
    {
        [ValueAttribute(1, typeof(LabelTextAttribute), "Pool <index />")] public SaintsArray<string[]> sa;
        [ValueAttribute(1, typeof(LabelTextAttribute), "Pool <index />")]
        public SaintsArray<string>[] saOutArr;

        [LabelText("OutLabelText")]
        public SaintsArray<string>[] saOutArrLabelDirect;

        // Dont break the old behaviors
        public ReferenceHashSet<IDummy> refHashSet;

        [KeyAttribute(typeof(ExpandableAttribute))]
        [ValueAttribute(typeof(ExpandableAttribute))]
        public SaintsDictionary<MonoBehaviour, MonoBehaviour> injectDict;

        public SaintsDictionary<IDummy, IDummy> refInterfaceDict;
    }
}
