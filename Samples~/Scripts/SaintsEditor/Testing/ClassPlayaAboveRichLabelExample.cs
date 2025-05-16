using System;
using SaintsField.Playa;

namespace SaintsField.Samples.Scripts.SaintsEditor.Testing
{
    [PlayaAboveRichLabel("<color=gray>This is a class message")]
    [PlayaAboveRichLabel("$" + nameof(dynamicContent))]
    public class ClassPlayaAboveRichLabelExample : SaintsMonoBehaviour
    {
        [ResizableTextArea]
        public string dynamicContent;

        [Serializable]
        [PlayaAboveRichLabel("<color=gray>--This is a struct message--")]
        public struct MyStruct
        {
            public string structString;
        }

        public MyStruct myStruct;
    }
}
