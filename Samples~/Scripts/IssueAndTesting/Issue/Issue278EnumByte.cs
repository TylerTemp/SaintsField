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

        [ShowIf(nameof(type), Type.SetIncreasePerInput)]
        public float newIncreasePerInput;

        [ShowIf(nameof(type), Type.AddToIncreasePerInput)]
        public float addToIncreasePerInput;

        [ShowIf(nameof(type), Type.SetResult)]
        public float newResult;

        [ShowIf(nameof(type), Type.AddToResult)]
        public float addToResult;
    }
}
