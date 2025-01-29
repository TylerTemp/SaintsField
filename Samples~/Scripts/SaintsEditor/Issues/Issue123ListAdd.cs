using System.Collections.Generic;
using SaintsField.Playa;
using UnityEngine;

namespace SaintsField.Samples.Scripts.SaintsEditor.Issues
{
    public class Issue123ListAdd: SaintsMonoBehaviour
    {
        [ListDrawerSettings] public List<string> contents;

        [Button]
        private void AddItem() => contents.Add($"{contents.Count}");

        [Button]
        private void RemoveItem() => contents.RemoveAt(Random.Range(0, contents.Count));
    }
}
