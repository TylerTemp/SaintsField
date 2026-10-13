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
        // [ValueAttribute(2, typeof(FieldLabelTextAttribute), "FIELDLABEL2")]
        // public SaintsArray<Scriptable[]> arrayInsideSaints;

        [ValueAttribute(1, typeof(LabelTextAttribute), "LABEL1")]
        [ValueAttribute(1, typeof(FieldLabelTextAttribute), "FIELDLABEL1")]
        public SaintsArray<Scriptable>[] saintsInsideArray;
    }
}
