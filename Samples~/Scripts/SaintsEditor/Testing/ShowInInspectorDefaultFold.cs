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

#pragma warning disable CS0414 // Field is assigned but its value is never used
        [ShowInInspector] private MyStruct _myStruct = new MyStruct();
#pragma warning restore CS0414 // Field is assigned but its value is never used
    }
}
