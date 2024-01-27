using UnityEngine;

namespace SaintsField.Samples.Scripts.IssueAndTesting.Issue
{
    public class Issue2 : MonoBehaviour
    {
        public bool enableVisualDebug;

        [SepTitle("Separate Here", EColor.Black)]
        [ReadOnly, SerializeField] private float _currentForwardSpeedInUnits;
        [ReadOnly, SerializeField] private float _currentSpeedInUnits;
        [ReadOnly, SerializeField] private float _currentSpeedInKmph;
        [ReadOnly, SerializeField] private float _maxSpeedKmph;

        public int throttle;
        public int currentGear;
    }
}
