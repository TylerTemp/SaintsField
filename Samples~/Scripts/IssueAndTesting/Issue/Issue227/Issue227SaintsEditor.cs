using SaintsField.Samples.Scripts.SaintsEditor;
using UnityEngine;

namespace SaintsField.Samples.Scripts.IssueAndTesting.Issue.Issue227
{
    public class Issue227SaintsEditor : SaintsMonoBehaviour
    {
        [Range(0, 1), ReadOnly]
        public float delay;

        public bool randomizeOffset = true;
    }
}
