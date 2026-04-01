using UnityEngine;

namespace SaintsField.Samples.Scripts.IssueAndTesting.Testing
{
    public class Downpour : SaintsMonoBehaviour
    {
        // [ValueAttribute(1, typeof(LabelTextAttribute), "Pool <index />")]
        // [ValueAttribute(2, typeof(LabelTextAttribute), "Lis <index />")]
        [ValueAttribute(3, typeof(LabelTextAttribute), "Item <index />")]
        public SaintsArray<SaintsArray<string>> saNest1;

        // [ValueAttribute(1, typeof(LabelTextAttribute), "Pool <index />")] public SaintsArray<string[]> sa;
        // [ValueAttribute(1, typeof(LabelTextAttribute), "Pool <index />")]
        // public SaintsArray<string>[] saOutArr;
        //
        // [LabelText("OutLabelText")]
        // public SaintsArray<string>[] saOutArrLabelDirect;
        //
        // // Dont break the old behaviors
        // public ReferenceHashSet<IDummy> refHashSet;
        //
        // [KeyAttribute(typeof(ExpandableAttribute))]
        // [ValueAttribute(typeof(ExpandableAttribute))]
        // public SaintsDictionary<MonoBehaviour, MonoBehaviour> injectDict;
        //
        // public SaintsDictionary<IDummy, IDummy> refInterfaceDict;
    }
}
