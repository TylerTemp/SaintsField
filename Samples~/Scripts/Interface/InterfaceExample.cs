using System;
using UnityEngine;

namespace SaintsField.Samples.Scripts.Interface
{
    public class InterfaceExample: MonoBehaviour
    {
        public SaintsInterface<Component, IInterface1> myInter1;

        // for old unity
        [Serializable]
        public class Interface1 : SaintsInterface<Component, IInterface1>
        {
        }

        public Interface1 myInherentInterface1;

        public SaintsInterface<Component, IInterface2> myInter2;
    }
}
