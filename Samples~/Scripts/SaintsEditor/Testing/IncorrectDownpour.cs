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
        [ValueAttribute(2, typeof(FieldLabelTextAttribute), "LABEL!")]
        public SaintsArray<Scriptable[]> arrayInsideSaints;
    }
}
