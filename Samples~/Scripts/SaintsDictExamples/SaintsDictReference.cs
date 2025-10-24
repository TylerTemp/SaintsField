using System;
using System.Collections.Generic;
using SaintsField.Samples.Scripts.Interface;
using UnityEngine;

namespace SaintsField.Samples.Scripts.SaintsDictExamples
{
    public class SaintsDictReference : SaintsMonoBehaviour
    {
#if UNITY_2021_3_OR_NEWER
        [Serializable]
        public abstract class BaseC
        {
            public string absC;
        }

        [Serializable]
        public class Sub1 : BaseC
        {
            public string sub1;
        }

        [Serializable]
        public class Sub2 : Sub1
        {
            public string sub2;
        }

        [Serializable]
        public struct MyS
        {
            public string s;
        }

        // -----------------

        public SaintsDictionary<IInterface1, IInterface1> interfaceDict;

        public SaintsDictionary<IInterface1, List<IInterface1>> interfaceDictLis;

        public SaintsDictionary<MyS, MyS[]> serializableDictArrVal;
        public SaintsDictionary<MyS, MyS> serializableDict;

        public SaintsDictionary<BaseC, BaseC> absDict;

        [KeyAttribute(typeof(SerializeReference))]
        // [ValueAttribute(typeof(SerializeReference))]
        public SaintsDictionary<Sub1, Sub1> dymDict;

        [KeyAttribute(typeof(PropRangeAttribute), 0f, 10f, -1f)]
        [ValueAttribute(typeof(ExpandableAttribute))]
        [ValueAttribute(typeof(RequiredAttribute))]
        public SaintsDictionary<int, SpriteRenderer> valueInject;
#endif
    }
}
