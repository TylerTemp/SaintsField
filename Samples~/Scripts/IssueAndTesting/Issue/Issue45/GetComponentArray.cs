using System.Collections.Generic;
using UnityEngine;

namespace SaintsField.Samples.Scripts.IssueAndTesting.Issue.Issue45
{
    public class GetComponentArray : MonoBehaviour
    {
        [GetComponent, PostFieldRichLabel("$" + nameof(DummyNumber))] public Dummy getComponent;
        [GetComponent, PostFieldRichLabel("$" + nameof(DummyNumber))] public Dummy[] getComponentArray;
        [GetComponent, PostFieldRichLabel("$" + nameof(DummyNumber))] public List<Dummy> getComponentList;

        private string DummyNumber(Dummy dummy)
        {
            return dummy? $"{dummy.comment}": "";
        }

        [GetByXPath("@{GetComponents(Dummy)}"), PostFieldRichLabel("$" + nameof(DummyNumber))] public Dummy[] getComponentArrayXPath;
    }
}
