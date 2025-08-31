using System;
using SaintsField.Playa;
using UnityEngine;

namespace SaintsField.Samples.Scripts.SaintsHashSetExamples
{
    public class ReferenceHashSetExample : MonoBehaviour
    {
#if UNITY_2021_3_OR_NEWER
        public interface IReference
        {
            string Name { get; }
        }

        [Serializable]
        public class ClassRef: IReference
        {
            [field: SerializeField]
            public string Name { get; private set; }

            public override string ToString()
            {
                return $"<ClassRef: {Name}/>";
            }
        }

        [Serializable]
        public struct StructRef: IReference
        {
            [SerializeField] public int i;

            [ShowInInspector]
            public string Name => $"int:{i}";

            public override string ToString()
            {
                return $"<StructRef: {Name}/>";
            }
        }
        public ReferenceHashSet<IReference> refHashSet;
#endif
    }
}
