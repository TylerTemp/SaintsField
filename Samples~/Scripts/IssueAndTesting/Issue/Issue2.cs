using UnityEngine;

namespace SaintsField.Samples.Scripts.IssueAndTesting.Issue
{
    public class Issue2 : MonoBehaviour
    {
        public bool enableVisualDebug;

        [SepTitle("Separate Here", EColor.Black)]
        [FieldReadOnly, SerializeField] private float _currentForwardSpeedInUnits;
        [FieldReadOnly, SerializeField] private float _currentSpeedInUnits;
        [FieldReadOnly, SerializeField] private float _currentSpeedInKmph;
        [FieldReadOnly, SerializeField] private float _maxSpeedKmph;

        public int throttle;
        public int currentGear;
    }
}
