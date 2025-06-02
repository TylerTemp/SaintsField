using System;
using System.Collections.Generic;
using SaintsField.Playa;

namespace SaintsField.Samples.Scripts.SaintsEditor.Testing
{
    public class ShowInInspectorNullListValue : SaintsMonoBehaviour
    {
        [Serializable]
        public struct MyStruct
        {
            public string name;
        }

        [ShowInInspector] private List<MyStruct> myList = new List<MyStruct>();
    }
}
