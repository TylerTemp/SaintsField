using SaintsField.Playa;
using SaintsField.Samples.Scripts.SaintsEditor;
using UnityEngine;

namespace SaintsField.Samples.Scripts.IssueAndTesting.Issue.Issue138
{
    public class DynamicArraySizeSaints : SaintsMonoBehaviour
    {
        [MinValue(1), Range(1, 10)] public int intValue;
        [ArraySize(nameof(intValue)), ListDrawerSettings] public string[] dynamic1;

        [Space]
        public Vector2Int v2Value;
        [ArraySize(nameof(v2Value)), ListDrawerSettings] public string[] dynamic2;

        private (int min, int max) TupleCallback() => (intValue, intValue + 3);
        [Space]
        [ArraySize(nameof(TupleCallback)), ListDrawerSettings] public string[] dynamic3;

        public int zeroUpdateValue;

        [ArraySize(nameof(zeroUpdateValue)), ListDrawerSettings] public string[] dynamic4;
    }
}
