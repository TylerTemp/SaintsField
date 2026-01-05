using SaintsField.Playa;
using UnityEngine;

namespace SaintsField.Samples.Scripts.IssueAndTesting.Issue.Issue8
{
    public class OtherKindsBase : MonoBehaviour
    {
        [ShowInInspector, Ordered]
        public const bool ConstTrue = true;

        [FieldDisableIf(nameof(ConstTrue)), Ordered] public string constTrueDisable;

        [ShowInInspector, Ordered]
        public static readonly bool StaticTrue = true;

        [FieldDisableIf(nameof(StaticTrue)), Ordered] public string staticTrueDisable;
    }
}
