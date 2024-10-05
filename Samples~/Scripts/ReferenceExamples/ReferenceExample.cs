using System;
using UnityEngine;

namespace SaintsField.Samples.Scripts.ReferenceExamples
{
    public class ReferenceExample: MonoBehaviour
    {
#if UNITY_2021_3_OR_NEWER
        [Serializable]
        public class Base1Fruit
        {
            public GameObject base1;
        }

        [Serializable]
        public class Base2Fruit: Base1Fruit
        {
            public int base2;
        }

        [Serializable]
        public class Apple : Base2Fruit
        {
            public string apple;
            public GameObject applePrefab;
        }

        [Serializable]
        public class Orange : Base2Fruit
        {
            public bool orange;
        }

        [SerializeReference, ReferencePicker]
        public Base2Fruit item;

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

        [SerializeReference, ReferencePicker]
        public IRefInterface structImpl;

        [SerializeReference, ReferencePicker]
        public IRefInterface myInterface;

        [ReadOnly]
        [SerializeReference, ReferencePicker]
        public IRefInterface myInterfaceDisabled = new ClassSub2();
#else
        [InfoBox("This feature is only available in Unity 2021.3 or newer.")]
        public string info;
#endif

    }
}
