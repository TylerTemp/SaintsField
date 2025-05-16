using System;
using SaintsField.Playa;

namespace SaintsField.Samples.Scripts.SaintsEditor.Testing
{
    [PlayaInfoBox("This is a class message", EMessageType.None)]
    [PlayaInfoBox("$" + nameof(dynamicContent))]
    public class ClassInfoBoxExample : SaintsMonoBehaviour
    {
        public string dynamicContent;

        [Serializable]
        [PlayaInfoBox("This is a struct message")]
        public struct MyStruct
        {
            public string structString;
        }

        public MyStruct myStruct;
    }
}
