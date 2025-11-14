using System.Collections.Generic;
using SaintsField.Playa;
using SaintsField.Samples.Scripts.Interface;
using SaintsField.Samples.Scripts.SaintsDictExamples;
using UnityEngine;

namespace SaintsField.Samples.Scripts.SaintsEditor
{
    public partial class SerHashSetExample : SaintsMonoBehaviour
    {
        [SaintsSerialized]
        public HashSet<string> stringHashSet;
        [SaintsSerialized]
        public HashSet<IInterface1> refHashSet;

        [SaintsSerialized]
        public HashSet<SaintsDictReference.Sub1> noPolymorphism;

        [SaintsSerialized, ValueAttribute(typeof(SerializeReference))]
        public ReferenceHashSet<SaintsDictReference.Sub1> polymorphism;
    }
}
