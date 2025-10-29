using System.Collections.Generic;
using SaintsField.Samples.Scripts;
using UnityEngine;

namespace SaintsField.Samples.IssueAndTesting.Issue.Issue45
{
    public class GetComponentByPathArray : MonoBehaviour
    {
        [GetComponentByPath("*"), EndText(nameof(DummyNumber), isCallback: true)] public Dummy getComponentByPath;
        [GetComponentByPath("*"), EndText(nameof(DummyNumber), isCallback: true)] public Dummy[] getComponentByPathArray;
        [GetComponentByPath("*[1]"), EndText(nameof(DummyNumber), isCallback: true)] public List<Dummy> getComponentByPathList;

        private string DummyNumber(Dummy dummy)
        {
            return dummy? $"{dummy.comment}": "";
        }
    }
}
