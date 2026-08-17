using UnityEngine;

namespace SaintsField.Samples.Scripts.IssueAndTesting.Issue
{
    public class Issue431MultipleMinMaxSer : MonoBehaviour
    {
        [SerializeField, MinMaxSlider(-180f, 180f), FieldBelowText("$" + nameof(_randomRotationRange))] private Vector2 _randomRotationRange;
        [SerializeField, MinMaxSlider(-180, 180), FieldBelowText("$" + nameof(_randomRotationRangeInt))] private Vector2Int _randomRotationRangeInt;

    }
}
