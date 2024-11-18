using System.Collections.Generic;
using SaintsField.Samples.Scripts.SaintsEditor;
using UnityEngine;

namespace SaintsField.Samples.Scripts.IssueAndTesting.Issue.Issue102
{
    public class Issue102 : MonoBehaviour
    {
        [SerializeField, GetComponentInChildren(false, typeof(MudCurvePoint), true)]
        private List<Transform> mPoints = new List<Transform>();
    }
}
