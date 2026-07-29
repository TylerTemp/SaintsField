using System;
using SaintsField.Playa;

namespace SaintsField.Samples.Scripts.SaintsEditor.Testing
{
    public class ShowIfFix : SaintsMonoBehaviour
    {
        [Serializable]
        public enum Id
        {
            None,
            One,
            Two,
            Three,
        }

        public Id id;

        [ShowIf(nameof(id), Id.One)]
        [ShowIf(nameof(id), Id.Two)]
        [AboveText("f")] public int number;

        [ShowIf(nameof(id), Id.Two)]
        [ShowIf(nameof(id), Id.Three)]
        [AboveText("f")]
        public int number2;
    }
}
