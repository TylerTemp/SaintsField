using System;
using SaintsField.Samples.Scripts.SaintsEditor;
using UnityEngine;

namespace SaintsField.Samples.Scripts.IssueAndTesting.Issue.Issud136
{
    public class Issue136Table : SaintsMonoBehaviour
    {
        [Serializable]
        public struct Level
        {
            public int price;
            public float change;
            public Scriptable indicator;
        }

        [Table]
        public Level[] levels;
    }
}
