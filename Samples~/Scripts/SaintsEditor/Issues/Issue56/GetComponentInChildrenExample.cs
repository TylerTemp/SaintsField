using UnityEngine;

namespace SaintsField.Samples.Scripts.SaintsEditor.Issues.Issue56
{
    public class GetComponentInChildrenExample : MonoBehaviour
    {
        [GetComponentInChildren, EndText(nameof(DummyNumber), isCallback: true)] public Dummy[] childrenWithSelf;
        [GetComponentInChildren(excludeSelf: true), EndText(nameof(DummyNumber), isCallback: true)] public Dummy[] childrenNoSelf;

        private string DummyNumber(Dummy dummy)
        {
            return dummy? $"{dummy.comment}": "";
        }
    }
}
