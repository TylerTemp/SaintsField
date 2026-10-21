using System;
using System.Collections.Generic;
using UnityEngine;

namespace SaintsField.Samples.Scripts.IssueAndTesting.Issue
{
    // https://github.com/codewriter-packages/Tri-Inspector/issues/248
    // Expand each container, then Items, and open Option on a TestItem.
    // All three paths should offer Option 1 (0), Option 2 (1), Option 3 (2).
    public class TriInspector248DictionaryDropdown : MonoBehaviour
    {
        [Serializable]
        public class Container
        {
            [SerializeReference, ReferencePicker]
            private Item[] items = { new TestItem() };
        }

        [Serializable]
        public abstract class Item
        {
        }

        [Serializable]
        public class TestItem : Item
        {
            [SerializeField, Dropdown(nameof(GetOptions))]
            private int option;

            private Dropdown<int> GetOptions()
            {
                return new Dropdown<int>
                {
                    { "Option 1", 0 },
                    { "Option 2", 1 },
                    { "Option 3", 2 },
                };
            }
        }

        [SerializeField]
        private Container control = new Container();

        [SerializeField]
        private Dictionary<string, Container> containers = new Dictionary<string, Container>
        {
            { "First", new Container() },
            { "Second", new Container() },
        };

        [SerializeField]
        private SaintsDictionary<string, Container> saintsContainers = new SaintsDictionary<string, Container>
        {
            { "First", new Container() },
            { "Second", new Container() },
        };
    }
}
