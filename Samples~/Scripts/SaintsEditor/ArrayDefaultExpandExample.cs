using System;
using System.Collections.Generic;
using SaintsField.Playa;

namespace SaintsField.Samples.Scripts.SaintsEditor
{
    public class ArrayDefaultExpandExample : SaintsMonoBehaviour
    {
        [ArrayDefaultExpand]
        public string[] arrayDefault;

        [ArrayDefaultExpand]
        public List<string> listDefault;

        [ArrayDefaultExpand, ListDrawerSettings]
        public string[] arrayDrawer;

        [Serializable]
        public struct TableStruct
        {
            public string name;
            public int value;
        }

        [ArrayDefaultExpand, Table] public TableStruct[] table;

        private int[] _myInts = new []{1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15};
    }
}
