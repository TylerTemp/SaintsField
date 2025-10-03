using System;
using SaintsField.Playa;
using UnityEngine;

namespace SaintsField.Samples.Scripts.SaintsEditor.Issues.Issue315
{
    [Serializable]
    public class ValidCell
    {
        public enum Type
        {
            T1,
            T2,
        }

        public Type type;
        [ShowIf(nameof(type), Type.T2)] public GameObject worldObject;
    }
}
