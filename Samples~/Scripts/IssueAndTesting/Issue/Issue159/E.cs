using System;
using UnityEngine;

namespace SaintsField.Samples.Scripts.IssueAndTesting.Issue.Issue159
{
    // [CreateAssetMenu(menuName = "SaintsField/Issue 159 Sample")]
    class E : ScriptableObject
    {
        public U U;
    }

    [Serializable]
    class U
    {
        [OnValueChanged("C")]
        public S S;

        public float W;

        private void C()
        {
            // float newValue = RandomFloat();
            W = RandomFloat();
            // Debug.Log($"Changed, W={W}, newValue={newValue}");
        }

        private float RandomFloat()
        {
            return UnityEngine.Random.Range(0f, 100f);
        }
    }

    [Serializable]
    class S
    {
        [PropRange(0, 1)] public float W;
    }
}
