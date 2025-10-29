using SaintsField.Samples.Scripts.Interface;
using SaintsField.Samples.Scripts.SaintsDictExamples;
using UnityEngine;

namespace SaintsField.Samples.Scripts.SaintsArrayExamples
{
    public class SaintsArrayRefExample : SaintsMonoBehaviour
    {
        public SaintsArray<IInterface1[]> inters;

        // [Button]
        // private IInterface1[][] D()
        // {
        //     Debug.Log($"length={inters.Count}");
        //     foreach (IInterface1[] i in inters)
        //     {
        //         Debug.Log(i);
        //     }
        //     return inters;
        // }

        [ValueAttribute(typeof(SerializeReference))]  // Still need some work...
        public SaintsArray<SaintsDictReference.Sub1[]> refSub1;
    }
}
