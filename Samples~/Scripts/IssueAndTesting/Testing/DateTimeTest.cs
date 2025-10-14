using SaintsField.Playa;
using UnityEngine;

namespace SaintsField.Samples.Scripts.IssueAndTesting.Testing
{
    public class DateTimeTest: MonoBehaviour
    {
        [DateTime]
        public long dt;

        [ShowInInspector] public long v => dt;
    }
}
