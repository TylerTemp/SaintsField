using System;
using SaintsField.Playa;
using UnityEngine;

namespace SaintsField.Samples.Scripts.IssueAndTesting.Issue
{
    public class Issue446FlagsEverything : SaintsMonoBehaviour
    {
        [BelowText("<field=B/> (<field/>)")]
        [SerializeField] Flag flag;

        [Button]
        private void ShowValue()
        {
            Debug.Log($"Btn Enum: {flag}, Value: {flag}({Convert.ToString((byte)flag, 2)})");
        }

        private void OnValidate()
        {
            Debug.Log($"OnValidate Enum: {flag}, Value: {flag}({Convert.ToString((byte)flag, 2)})");
        }

        private void Awake()
        {
            Debug.Log($"In Runtime Value: Enum: {flag} ({Convert.ToString((byte)flag, 2)})");
        }

        [Flags]
        enum Flag : byte
        {
            None = byte.MinValue,
            Everything = byte.MaxValue,
            FlagA = 1 << 0,
            FlagB = 1 << 1,
            FlagC = 1 << 2,
            FlagD = 1 << 3,
        }
    }
}
