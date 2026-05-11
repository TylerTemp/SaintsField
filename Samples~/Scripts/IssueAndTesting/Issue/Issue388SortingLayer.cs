using UnityEngine;

namespace SaintsField.Samples.Scripts.IssueAndTesting.Issue
{
    public class Issue388SortingLayer : MonoBehaviour
    {
        [SerializeField, SortingLayer]
        private int _staticSortingLayer = 0;
    }
}
