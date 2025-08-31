using System;
using SaintsField.Playa;

namespace SaintsField.Samples.Scripts.SaintsEditor.Testing
{
    public class ShowInInspectorDefaultFold : SaintsMonoBehaviour
    {
        [Serializable]
        public struct MyStruct
        {
            public string name;
        }

        [ShowInInspector] private MyStruct _myStruct = new MyStruct();
    }
}
