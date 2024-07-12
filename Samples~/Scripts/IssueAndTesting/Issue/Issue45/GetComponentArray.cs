using System.Collections.Generic;
using SaintsField.Samples.Scripts;
using UnityEngine;

namespace SaintsField.Samples.IssueAndTesting.Issue.Issue45
{
    public class GetComponentArray : MonoBehaviour
    {
        [GetComponent, PostFieldRichLabel(nameof(DummyNumber), isCallback: true)] public Dummy[] getComponentArray;
        [GetComponent, PostFieldRichLabel(nameof(DummyNumber), isCallback: true)] public List<Dummy> getComponentList;

        private string DummyNumber(Dummy dummy)
        {
            return dummy? $"{dummy.comment}": "";
        }
    }
}
