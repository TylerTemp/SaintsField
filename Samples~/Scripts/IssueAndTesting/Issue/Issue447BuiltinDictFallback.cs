using System;
using System.Collections.Generic;
using UnityEngine;

namespace SaintsField.Samples.Scripts.IssueAndTesting.Issue
{
    public class Issue447BuiltinDictFallback : SaintsMonoBehaviour
    {
        [Serializable]
        class Test
        {
            public string A;
            public int B;
        }

        [SerializeField] Dictionary<int, string> dictA; // Works
        [SerializeField] Dictionary<string, string> dictB; // Works
        [SerializeField] Dictionary<Vector2, string> dictC; // Works
        [SerializeField] Dictionary<Test, string> dictD; // Works
        [SerializeField] Dictionary<ScriptableObject, string> dictE; // Fails
    }
}
