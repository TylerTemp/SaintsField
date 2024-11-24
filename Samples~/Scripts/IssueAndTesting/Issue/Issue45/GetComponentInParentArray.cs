using System.Collections.Generic;
using UnityEngine;

namespace SaintsField.Samples.Scripts.IssueAndTesting.Issue.Issue45
{
    public class GetComponentInParentArray : MonoBehaviour
    {
        [GetComponentInParent, PostFieldRichLabel("$" + nameof(DummyNumber)), OnValueChanged(nameof(OnChange))] public Dummy[] getComponentInParentArray;
        [GetComponentInParents, PostFieldRichLabel("$" + nameof(DummyNumber)), OnValueChanged(nameof(OnChange))] public List<Dummy> getComponentInParentsList;

        [GetComponentInParents] public Dummy getComponentInParents;

        private string DummyNumber(Dummy dummy)
        {
            return dummy? $"{dummy.comment}": "Null";
        }

        public void OnChange(object value, int index = -1)
        {
            Debug.Log($"change[{index}]: {value}");
        }
    }
}
