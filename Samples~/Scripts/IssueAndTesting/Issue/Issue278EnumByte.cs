using SaintsField.Samples.Scripts.SaintsEditor;

namespace SaintsField.Samples.Scripts.IssueAndTesting.Issue
{
    public class Issue278EnumByte : SaintsMonoBehaviour
    {
        public enum Type : byte
        {
            SetIncreasePerInput,
            AddToIncreasePerInput,
            SetResult,
            AddToResult,
        }

        // [BelowRichLabel("$" + nameof(type))]
        public Type type;

        [FieldShowIf(nameof(type), Type.SetIncreasePerInput)]
        public float newIncreasePerInput;

        [FieldShowIf(nameof(type), Type.AddToIncreasePerInput)]
        public float addToIncreasePerInput;

        [FieldShowIf(nameof(type), Type.SetResult)]
        public float newResult;

        [FieldShowIf(nameof(type), Type.AddToResult)]
        public float addToResult;
    }
}
