using UnityEngine;

namespace SaintsField.Samples.Scripts.IssueAndTesting.Issue.Issue8
{
    public class MinMax : MinMaxBase
    {
        [MinValue(0), MaxValue(nameof(upLimit))] public int min0Max;
        [MinValue(nameof(upLimit)), MaxValue(10)] public float fMinMax10;
    }
}
