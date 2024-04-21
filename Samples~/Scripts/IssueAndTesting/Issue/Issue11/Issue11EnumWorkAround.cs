using System;
using UnityEngine;

namespace SaintsField.Samples.Scripts.IssueAndTesting.Issue.Issue11
{
    public class Issue11EnumWorkAround : MonoBehaviour
    {
        [Serializable]
        public enum EnumState
        {
            Off,
            On,
        }

        public EnumState state;

#if UNITY_EDITOR
        [EnableIf(nameof(isOn))]
#endif
        public string editable;

#if UNITY_EDITOR
        private bool isOn() => state == EnumState.On;
#endif
    }
}
