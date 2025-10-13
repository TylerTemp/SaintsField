using UnityEngine;

namespace SaintsField.Samples.Scripts.IssueAndTesting.Testing
{
    public class DateTimeTest: MonoBehaviour
    {
        [DateTime, AboveRichLabel("<field/>")]
        public long dt;
    }
}
