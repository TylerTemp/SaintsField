using System;
using System.Collections.Generic;
using SaintsField.Playa;

namespace SaintsField.Samples.Scripts.SaintsEditor
{
    public partial class SerDateTimeExample : SaintsMonoBehaviour
    {
        [SaintsSerialized, OnValueChanged(":Debug.Log")]
        private DateTime _dt;

        [Serializable]
        public partial class MyClass
        {
            [SaintsSerialized, OnValueChanged(":Debug.Log")]
            private DateTime[] _dtArray;
        }

        public MyClass myClass;

        [SaintsSerialized, OnValueChanged(":Debug.Log")]
        private List<DateTime> _dtList;

        [LayoutStart("H", ELayout.Horizontal | ELayout.FoldoutBox)]

        [SaintsSerialized]
        public DateTime dt;
        public string s;
    }
}
