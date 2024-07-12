using System.Collections.Generic;
using SaintsField.Samples.Scripts;
using UnityEngine;

namespace SaintsField.Samples.IssueAndTesting.Issue.Issue45
{
    public class GetComponentInChildrenArray : MonoBehaviour
    {
        [GetComponentInChildren, PostFieldRichLabel(nameof(DummyNumber), isCallback: true)] public Dummy[] getComponentInChildrenArray;
        [GetComponentInChildren, PostFieldRichLabel(nameof(DummyNumber), isCallback: true)] public List<Dummy> getComponentInChildrenList;

        private string DummyNumber(Dummy dummy)
        {
            return dummy? $"{dummy.comment}": "";
        }
    }
}
