using System;
using SaintsField.Playa;

namespace SaintsField.Samples.Scripts.SaintsEditor.Testing
{
    [AboveText("<color=gray>This is a class message")]
    [AboveText("$" + nameof(dynamicContent))]
    public class ClassPlayaAboveRichLabelExample : SaintsMonoBehaviour
    {
        [ResizableTextArea]
        public string dynamicContent;

        [Serializable]
        [AboveText("<color=gray>--This is a struct message--")]
        public struct MyStruct
        {
            public string structString;
        }

        public MyStruct myStruct;
    }
}
