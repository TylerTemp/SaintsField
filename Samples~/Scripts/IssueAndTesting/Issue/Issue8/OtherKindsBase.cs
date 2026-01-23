using SaintsField.Playa;
using UnityEngine;

namespace SaintsField.Samples.Scripts.IssueAndTesting.Issue.Issue8
{
    public class OtherKindsBase : MonoBehaviour
    {
        [ShowInInspector]
        public const bool ConstTrue = true;

        [FieldDisableIf(nameof(ConstTrue))] public string constTrueDisable;

        [ShowInInspector]
        public static readonly bool StaticTrue = true;

        [FieldDisableIf(nameof(StaticTrue))] public string staticTrueDisable;
    }
}
