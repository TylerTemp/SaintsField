using System;
using UnityEngine;

namespace SaintsField.Samples.Scripts.Interface
{
    public class InterfaceExample: MonoBehaviour
    {
        // old unity
        [Serializable]
        public class MyInter1 : SaintsObjectInterface<IInterface1> { }

        public MyInter1 myInter1;
    }
}
