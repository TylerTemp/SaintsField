using System;

namespace SaintsField.Samples.Scripts.SaintsEditor
{
    public class FieldTagSub : SaintsMonoBehaviour
    {
        [Serializable]
        public struct MyStruct
        {
            public string myName;
        }

        [LabelText("<field.myName />")]
        [AboveText("<field.myName />")]
        [BelowText("<field.myName />")]
        public MyStruct myStruct;
    }
}
