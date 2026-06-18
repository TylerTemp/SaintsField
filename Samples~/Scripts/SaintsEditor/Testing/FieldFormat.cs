using System;
using UnityEngine;

namespace SaintsField.Samples.Scripts.SaintsEditor.Testing
{
    public class FieldFormat : SaintsMonoBehaviour
    {
        [Serializable]
        public enum MyEnum
        {
            One = 1,
            Hundred = 100,
            Quarter = 25,
        }

        [BelowText("<field=D/>")]
        public MyEnum id;

    }
}
