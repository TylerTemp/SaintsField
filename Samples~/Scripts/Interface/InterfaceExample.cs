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
        [RichLabel("<label />")]
        public Interface1[] myInherentInterface1Lis;

        public SaintsInterface<Component, IInterface2> myInter2;
        public SaintsInterface<ScriptableObject, IInterface2> mySoInter2;

        [RichLabel("<color=green><label/>")]
        public SaintsInterface<UnityEngine.Object, IInterface2> myAnyInter2;

        // private void Awake()
        // {
        //     Debug.Log(myInter1.I);  // the actual interface object
        //     Debug.Log(myInter1.V);  // the actual serialized object
        //
        //     myInter1.I.Method1();
        // }
    }
}
