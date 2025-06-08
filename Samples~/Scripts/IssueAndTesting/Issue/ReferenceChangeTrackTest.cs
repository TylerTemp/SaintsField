using System;
using UnityEngine;

namespace SaintsField.Samples.Scripts.IssueAndTesting.Issue
{
    public class ReferenceChangeTrackTest : MonoBehaviour
    {
#if UNITY_2021_3_OR_NEWER
        public interface IRefInterface
        {
            public int TheInt { get; }
        }

        [Serializable]
        public struct StructImpl : IRefInterface
        {
            [field: SerializeField]
            public int TheInt { get; set; }
            public string myStruct;
        }

        [Serializable]
        public class ClassDirect: IRefInterface
        {
            [field: SerializeField, Range(0, 10)]
            public int TheInt { get; set; }
        }

        // [Serializable]
        public abstract class ClassSubAbs : ClassDirect
        {
            public abstract string AbsValue { get; }
        }

        [Serializable]
        public class ClassSub1 : ClassSubAbs
        {
            public string sub1;
            public override string AbsValue => $"Sub1: {sub1}";
        }

        [Serializable]
        public class ClassSub2 : ClassSubAbs
        {
            public string sub2;
            public override string AbsValue => $"Sub2: {sub2}";
        }

        [SerializeReference, ReferencePicker, OnValueChanged(nameof(Changed))]
        public IRefInterface structImpl;

        private void Changed(IRefInterface o)
        {
            // Debug.Log($"changed {o}");
        }
#endif
    }
}
