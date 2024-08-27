using System;
using System.Collections.Generic;
using UnityEngine;

namespace SaintsField.Samples.Scripts.IssueAndTesting.Issue.Issue23
{
    public class ImGuiFallback : MonoBehaviour
    {
        public bool toggle;

        [Serializable]
        public class Container<T1, T2>
        {
            [Serializable]
            protected sealed class Entry
            {
                public T1 Name;
                public T2 Object;
            }

            [SerializeField]
            protected List<Entry> Entries;
        }

        [Serializable]
        public class ContainerChild<T> : Container<string, T>
        {
            public T GetByName(string name)
            {
                var entry = Entries.Find(e => e.Name == name);
                return entry == default ? default : entry.Object;
            }
        }

        [Serializable]
        public  class GameObjectChild : ContainerChild<GameObject>
        {
        }

        public GameObjectChild normal;
        [HideIf(nameof(toggle)), InfoBox("Inherent Fallback")] public GameObjectChild inherent;
        // won't work on old Unity
        [HideIf(nameof(toggle)), InfoBox("Direct Fallback")] public ContainerChild<GameObject> direct;

        // public SaintsArray<string> plain;

        // [ShowIf(nameof(toggle)), InfoBox("Type CustomDrawer fallback", above: true)]
        // public SaintsArray<string> dec;
    }
}
