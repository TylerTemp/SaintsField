using System;
using UnityEngine;

namespace SaintsField.Samples.Scripts.SaintsEditor.Testing
{
    public class SaveRestoreTest : SaintsMonoBehaviour
    {
        public interface IMyInterface
        {

        }

        [Serializable]
        public struct MyStruct : IMyInterface
        {
            public string myStruct;
        }

        [Serializable]
        public class MyClass : IMyInterface
        {
            public string myClass;
        }

        [SerializeReference, ReferencePicker]
        public IMyInterface myInterface;

        public MyClass myClass;

        public string[] lis;
    }
}
