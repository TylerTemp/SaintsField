using System.Collections.Generic;
using SaintsField.Samples.Scripts;
using UnityEngine;

namespace SaintsField.Samples.IssueAndTesting.Issue.Issue45
{
    public class GetComponentInSceneArray : MonoBehaviour
    {
        [GetComponentInScene, PostFieldRichLabel(nameof(DummyNumber), isCallback: true)] public Dummy[] getComponentInParentArray;
        [GetComponentInScene, PostFieldRichLabel(nameof(DummyNumber), isCallback: true)] public List<Dummy> getComponentInParentsList;

        private string DummyNumber(Dummy dummy)
        {
            return dummy? $"{dummy.comment}": "";
        }
    }
}
