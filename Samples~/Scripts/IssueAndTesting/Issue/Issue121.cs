using UnityEngine;

namespace SaintsField.Samples.Scripts.IssueAndTesting.Issue
{
    public class Issue121 : MonoBehaviour
    {
        [SerializeField]
        private bool _overrideValue;

        [SerializeField, ShowIf(nameof(_overrideValue)), Min(0), Tooltip("A value of 0 means it's infinite")]
        private int _valueOverride;

        [SerializeField, MinValue(0), ShowIf(nameof(_overrideValue)), Tooltip("A value of 0 means it's infinite")]
        private int _valueOverrideUsingSaintsField;
    }
}
