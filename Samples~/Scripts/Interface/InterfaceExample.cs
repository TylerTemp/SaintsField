using System;
using UnityEngine;

namespace SaintsField.Samples.Scripts.Interface
{
    public class InterfaceExample: MonoBehaviour
    {
        [OnValueChanged(":Debug.Log")]
        public SaintsInterface<IInterface1> i1;

        public SaintsInterface<Component, IInterface1> myInter1;

        [Serializable]
        private struct NorStructInterface1 : IInterface1
        {
            public int common;
            public string structString;

            public void Method1()
            {
            }

            public override string ToString()
            {
                return $"<SInterface1 common={common} structString={structString}>";
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
                return $"<CInterface1 common={common} classString={classString}>";
            }
        }

        public SaintsInterface<IInterface1> myInherentInterface1;
        [FieldLabelText("<label />")]
        public SaintsInterface<IInterface1>[] myInherentInterface1Lis;

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
