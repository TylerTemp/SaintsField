using System;
using System.Collections.Generic;
using SaintsField.Playa;

namespace SaintsField.Samples.Scripts.SaintsEditor
{
    public partial class SerDateTimeExample : SaintsMonoBehaviour
    {
        [SaintsSerialized]
        private DateTime _dt;

        [Serializable]
        public partial class MyClass
        {
            [SaintsSerialized]
            private DateTime[] _dtArray;
        }

        public MyClass myClass;

        [SaintsSerialized]
        private List<DateTime> _dtList;

        [LayoutStart("H", ELayout.Horizontal | ELayout.FoldoutBox)]

        [SaintsSerialized]
        public DateTime dt;
        public string s;
    }
}
