using System;
using UnityEngine;

namespace SaintsField.Samples.Scripts.SaintsEditor.Issues
{
    public class Issue325Table : SaintsMonoBehaviour
    {
        [Serializable]
        public struct Nest4
        {
            public string n4;
        }
        [Serializable]
        public struct Nest3
        {
            public Nest4 nest4;
            public string n3;
        }

        [Serializable]
        public struct Nest2
        {
            public Nest3 nest3;
            public string n2;
        }

        [Serializable]
        public struct Nest1
        {
            public Nest2 nest2;
            public string n1;
        }

        [Serializable]
        public struct TableContainer
        {
            public Nest1 nest1;
            public string table;
        }

        [Table] public TableContainer[] table;
    }
}
