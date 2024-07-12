using System.Collections;
using System.Collections.Generic;
using SaintsField.Samples.Scripts;
using UnityEngine;

namespace SaintsField.Samples.IssueAndTesting.Issue.Issue45
{
    public class GetComponentArray : MonoBehaviour
    {
        [GetComponent, PostFieldRichLabel(nameof(DummyNumber), isCallback: true)] public Dummy[] getComponentArray;

        private string DummyNumber(Dummy dummy)
        {
            return dummy? $"{dummy.number}": "";
        }
    }
}
