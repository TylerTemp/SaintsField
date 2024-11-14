using System;
using SaintsField.Playa;
using UnityEngine;

namespace SaintsField.Samples.Scripts.IssueAndTesting.Issue
{
    public class Issue80 : MonoBehaviour
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
            [LayoutStart("Hi", ELayout.FoldoutBox)]
            public string myStruct;

            public ClassDirect nestedClass;
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

        // [SerializeReference, ReferencePicker, ReferenceDisplay]
        // public IRefInterface structImpl;
        //
        // [SerializeReference, ReferencePicker]
        // public IRefInterface plain;

        [SerializeReference, OnValueChanged(nameof(ValueChanged)), ReferencePicker, SaintsRow]
        public IRefInterface saints;

        [SerializeReference, ReferencePicker(hideLabel: true), SaintsRow(inline: true)]
        public IRefInterface inline;

        private void ValueChanged(object v) => Debug.Log(v);

        // [SerializeReference, ReferencePicker]
        // public IRefInterface myInterface;
#else
        [InfoBox("This feature is only available in Unity 2021.3 or newer.")]
        public string info;
#endif

    }
}
