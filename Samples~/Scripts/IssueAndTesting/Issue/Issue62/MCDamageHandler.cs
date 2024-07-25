using System.Collections;
using System.Collections.Generic;
using SaintsField.Samples.Scripts.IssueAndTesting.Issue46;
using UnityEngine;

namespace SaintsField.Samples.Scripts.IssueAndTesting.Issue62
{
    public class MCDamageHandler : MonoBehaviour
    {
        [Expandable, Required] public MCTeam team;

        [SerializeField, HideIf(nameof(isRelay))]
        float _health = 100f;

        [HideIf(nameof(isRelay))] internal float _currentHealth;

        public virtual float Health
        {
            get { return _currentHealth; }
            set { _currentHealth = value; }
        }

        public float normalizedHealth => Health / _health;

        [GetComponent, HideIf(nameof(isRelay))]
        public Rigidbody rigidBody;

        [HideIf(nameof(isRelay))] public Transform targetOffset;
        public virtual void OnEnable() => Health = _currentHealth = _health;
        public bool IsHealthLessThan(float h) => _currentHealth < h;
        public bool IsHealth_PercentLessThan(float percent) => Health / _health * 100.0 < percent;
        public virtual bool isRelay => false;
    }
}
