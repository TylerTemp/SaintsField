using UnityEngine;

namespace SaintsField.Samples.Scripts.IssueAndTesting.Issue.Issue138
{
    public class DynamicArraySizeNormal : MonoBehaviour
    {
        [MinValue(1), Range(1, 10)] public int intValue;
        [ArraySize(nameof(intValue))] public string[] dynamic1;

        [Space]
        public Vector2Int v2Value;
        [ArraySize(nameof(v2Value))] public string[] dynamic2;

        private (int min, int max) TupleCallback() => (intValue, intValue + 3);
        [Space]
        [ArraySize(nameof(TupleCallback))] public string[] dynamic3;
    }
}
