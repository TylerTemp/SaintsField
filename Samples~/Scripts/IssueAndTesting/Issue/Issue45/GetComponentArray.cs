using System.Collections.Generic;
using SaintsField.Playa;
using SaintsField.Samples.Scripts.SaintsEditor;
using UnityEngine;

namespace SaintsField.Samples.Scripts.IssueAndTesting.Issue.Issue45
{
    public class GetComponentArray : SaintsMonoBehaviour
    {
        [
            GetComponent,
            // PostFieldRichLabel("$" + nameof(DummyNumber))
        ]
        public Dummy getComponent;
        [GetComponent, EndText("$" + nameof(DummyNumber))] public Dummy[] getComponentArray;
        [GetComponent, EndText("$" + nameof(DummyNumber))] public List<Dummy> getComponentList;

        private string DummyNumber(Dummy dummy)
        {
            return dummy? $"{dummy.comment}": "NULL";
        }

        [GetByXPath("@{GetComponents(Dummy)}"), EndText("$" + nameof(DummyNumber))] public Dummy[] getComponentArrayXPath;
        [Button]
        private void DebugButton()
        {
            Debug.Log(getComponent);
            Debug.Log(getComponentArray.Length);
            Debug.Log(getComponentList.Count);
        }
    }
}
