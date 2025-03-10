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

        [ArrayDefaultExpand, ListDrawerSettings]
        public List<string> listDrawer;

        [Serializable]
        public struct TableStruct
        {
            public string name;
            public int value;
        }

        [ArrayDefaultExpand, Table] public TableStruct[] table;
    }
}
