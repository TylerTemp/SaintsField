using System;
using System.Collections.Generic;
using SaintsField.Playa;

namespace SaintsField.Samples.Scripts.SaintsEditor.Testing
{
    public class ListRemoveTest : SaintsMonoBehaviour
    {
        [Serializable]
        public struct MyStruct
        {
            public int myInt;
        }

        public MyStruct[] myStructs;

        [GetScriptableObject]
        public Scriptable[] mySoListRef;

        [Ordered, Expandable]
        public List<Scriptable> mySoList;

        [Button]
        private void SetSo()
        {
            mySoList.RemoveAt(1);
        }
    }
}
