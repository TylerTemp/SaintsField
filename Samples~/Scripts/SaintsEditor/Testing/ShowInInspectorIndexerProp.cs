using System;
using SaintsField.Playa;
using UnityEngine;

namespace SaintsField.Samples.Scripts.SaintsEditor.Testing
{
    public class ShowInInspectorIndexerProp : SaintsMonoBehaviour
    {
        public struct Indexer
        {
            // 1. Define an indexer. This acts as a property that requires a parameter.
            public string this[int index]
            {
                get { return $"Item {index}"; }
            }

            public string this[string keyIndex]
            {
                get { return $"Key {keyIndex}"; }
            }
        }

        [ShowInInspector] private Indexer _indexer;
    }
}
