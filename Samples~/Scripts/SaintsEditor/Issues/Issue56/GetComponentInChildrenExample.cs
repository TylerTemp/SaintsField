using UnityEngine;

namespace SaintsField.Samples.Scripts.SaintsEditor.Issues.Issue56
{
    public class GetComponentInChildrenExample : MonoBehaviour
    {
        [GetComponentInChildren, PostFieldRichLabel(nameof(DummyNumber), isCallback: true)] public Dummy[] childrenWithSelf;
        [GetComponentInChildren(excludeSelf: true), PostFieldRichLabel(nameof(DummyNumber), isCallback: true)] public Dummy[] childrenNoSelf;

        private string DummyNumber(Dummy dummy)
        {
            return dummy? $"{dummy.comment}": "";
        }
    }
}
