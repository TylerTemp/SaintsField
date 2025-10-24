using SaintsField.Samples.Scripts.Interface;
using SaintsField.Samples.Scripts.SaintsDictExamples;
using UnityEngine;

namespace SaintsField.Samples.Scripts.SaintsHashSetExamples
{
    public class ReferenceHashSetExample : MonoBehaviour
    {
#if UNITY_2021_3_OR_NEWER

        public SaintsHashSet<IInterface1> refHashSet;

        public SaintsHashSet<SaintsDictReference.Sub1> noPolymorphism;
        public ReferenceHashSet<SaintsDictReference.Sub1> polymorphism;
#endif
    }
}
