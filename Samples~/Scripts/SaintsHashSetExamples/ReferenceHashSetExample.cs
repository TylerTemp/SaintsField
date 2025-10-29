using SaintsField.Samples.Scripts.Interface;
using SaintsField.Samples.Scripts.SaintsDictExamples;

namespace SaintsField.Samples.Scripts.SaintsHashSetExamples
{
    public class ReferenceHashSetExample : SaintsMonoBehaviour
    {
        public SaintsHashSet<IInterface1> refHashSet;

        // [Button]
        // // [ShowInInspector]
        // private IInterface1[] r()
        // {
        //     return refHashSet.ToArray();
        // }

        public SaintsHashSet<SaintsDictReference.Sub1> noPolymorphism;
        public ReferenceHashSet<SaintsDictReference.Sub1> polymorphism;
    }
}
