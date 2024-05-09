using System;
using UnityEngine;

namespace SaintsField.Samples.Scripts.IssueAndTesting.Issue.Issue23
{
    public class ImGuiFallback : MonoBehaviour
    {
        public bool toggle;

        [Serializable]
        public struct Source
        {
            [SerializeField] private string[] serializedEntries;
        }

        // public Source normal;
        [HideIf(nameof(toggle)), InfoBox("Type CustomDrawer fallback", above: true)] public Source withIf;

        // public SaintsArray<string> plain;

        [ShowIf(nameof(toggle)), InfoBox("Type CustomDrawer fallback", above: true)]
        public SaintsArray<string> dec;
    }
}
