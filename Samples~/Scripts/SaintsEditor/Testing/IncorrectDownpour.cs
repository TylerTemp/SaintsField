using UnityEngine;

namespace SaintsField.Samples.Scripts.SaintsEditor.Testing
{
    public class IncorrectDownpour : SaintsMonoBehaviour
    {
        // [SerializeField
        //  , LabelText("Override Label")
        //  , AboveText
        // ]
        // [FieldLabelText("$" + nameof(PoolName))]
        // // [ValueAttribute()]
        // private SaintsArray<Scriptable>[] _conf = {};
        //
        // private string PoolName(SaintsArray<Scriptable> pool, int index)
        // {
        //     return $"[{index}] pool";
        // }

        // [ValueAttribute(typeof(LabelTextAttribute), "OK!")] public SaintsArray<Scriptable[]> arrayInsideSaintsContentLabel;
        // [ValueAttribute(typeof(LabelTextAttribute), new object[]{null})]

        // [ValueAttribute(1, typeof(LabelTextAttribute), "LABEL1")]
        // [ValueAttribute(2, typeof(LabelTextAttribute), "LABEL2")]
        // // [ValueAttribute(2, typeof(FieldLabelTextAttribute), "FIELDLABEL2")]  // no longer necessary
        // public SaintsArray<Scriptable[]> arrayInsideSaints;
        //
        // [ValueAttribute(1, typeof(LabelTextAttribute), "LABEL1")]
        // [ValueAttribute(2, typeof(LabelTextAttribute), "LABEL2")]
        // // [ValueAttribute(1, typeof(FieldLabelTextAttribute), "FIELDLABEL1")]
        // // [ValueAttribute(2, typeof(FieldLabelTextAttribute), "FIELDLABEL2")]  // this will get skip cuz only playa get this level
        // // [ValueAttribute(3, typeof(FieldLabelTextAttribute), "FIELDLABEL3")]
        // public SaintsArray<Scriptable>[] saintsInsideArray;

        [ValueAttribute(1, typeof(LabelTextAttribute), "Random Pool <index/>")]
        [ValueAttribute(2, typeof(LabelTextAttribute), "Option <index/>")]
        [ValueAttribute(2, typeof(ExpandableAttribute))]
        public SaintsArray<Scriptable>[] exampleExpand;

        [KeyAttribute(typeof(ExpandableAttribute))]
        [ValueAttribute(2, typeof(ExpandableAttribute))]
        public SaintsDictionary<Component, Scriptable[]> exampleDict;

        [ValueAttribute(2, typeof(InfoBoxAttribute), "$../../" + nameof(GetMessage), false)]
        public SaintsArray<int>[] callbackNote;

        private string GetMessage(int num) => num >= 0? "": $"Should not be negative: {num}";
    }
}
