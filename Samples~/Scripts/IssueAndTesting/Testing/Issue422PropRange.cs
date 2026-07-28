using UnityEngine;

namespace SaintsField.Samples.Scripts.IssueAndTesting.Testing
{
    public class Issue422PropRange : MonoBehaviour
    {
        [SerializeField]
        [PropRange(4, 8)]
        [FieldBelowText("$" + nameof(_testRange))]
        private int _testRange;

        [SerializeField, PropRange(4, 8)] private uint _uint;
        [SerializeField, PropRange(4, 8)] private long _long;
        [SerializeField, PropRange(4, 8)] private ulong _ulong;

        [SerializeField]
        [PropRange(4, 8)]
        [FieldBelowText("$" + nameof(_testfRange))]
        private float _testfRange;
    }
}
