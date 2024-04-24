using SaintsField.Playa;
using UnityEngine;

namespace SaintsField.Samples.Scripts.IssueAndTesting.Issue.Issue11
{
    public class Issue11Property : MonoBehaviour
    {
        public bool boolValue;

        // DisableIf works on serialized property as expected
        [field: SerializeField, DisableIf(nameof(boolValue))]
        public Color ColorAutoProperty { get; private set; }

        // non serialized property needs `Playa` to help
        [ShowInInspector, PlayaShowIf(nameof(boolValue))]
        public static Color MyStaticProp => Color.green;  // native Property

        [ShowInInspector, PlayaShowIf(nameof(boolValue))]
        public static Color staticField = Color.blue;  // non serialized Property

        [ShowInInspector, PlayaShowIf(nameof(boolValue))]
        public static readonly string StaticReadOnlyField = "Building Nothing Out of Something";  // non serialized Field

        [ShowInInspector, PlayaShowIf(nameof(boolValue))]
        public const float MyConst = 3.14f;  // non serialized Field
    }
}
