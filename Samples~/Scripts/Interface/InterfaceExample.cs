using System;
using UnityEngine;

namespace SaintsField.Samples.Scripts.Interface
{
    public class InterfaceExample: MonoBehaviour
    {
        public SaintsObjInterface<IInterface1> i1;

        public SaintsInterface<Component, IInterface1> myInter1;

        // for old unity
        [Serializable]
        public class Interface1 : SaintsInterface<Component, IInterface1>
        {
            public Interface1(Component obj) : base(obj)
            {
            }
        }

        [Serializable]
        private struct NorStructInterface1 : IInterface1
        {
            public int common;
            public string structString;

            public void Method1()
            {
            }
        }

        [Serializable]
        private struct NorClassInterface1 : IInterface1
        {
            public int common;
            public string classString;

            public void Method1()
            {
            }

            public override string ToString()
            {
                return $"<NorClassInterface1 common={common} classString={classString}>";
            }
        }

        public Interface1 myInherentInterface1;
        [FieldLabelText("<label />")]
        public Interface1[] myInherentInterface1Lis;

        public SaintsInterface<Component, IInterface2> myInter2;
        public SaintsInterface<ScriptableObject, IInterface2> mySoInter2;

        [FieldLabelText("<color=green><label/>")]
        public SaintsInterface<UnityEngine.Object, IInterface2> myAnyInter2;

        private void Awake()
        {
            Debug.Log(myInter1.I);  // the actual interface object
            Debug.Log(myInter1.V);  // the actual serialized object

            myInter1.I.Method1();
        }
    }
}
