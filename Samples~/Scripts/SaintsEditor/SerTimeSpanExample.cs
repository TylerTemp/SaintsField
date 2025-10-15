using System;
using System.Collections.Generic;
using SaintsField.Playa;

namespace SaintsField.Samples.Scripts.SaintsEditor
{
    public partial class SerTimeSpanExample : SaintsMonoBehaviour
    {
        [SaintsSerialized]
        private TimeSpan _dt;

        [Serializable]
        public partial class MyClass
        {
            [SaintsSerialized]
            private TimeSpan[] _dtArray;
        }

        public MyClass myClass;

        [SaintsSerialized]
        private List<TimeSpan> _dtList;

        [LayoutStart("H", ELayout.Horizontal | ELayout.FoldoutBox)]

        [SaintsSerialized]
        public TimeSpan dt;
        [SaintsSerialized]
        public TimeSpan s;
    }
}
