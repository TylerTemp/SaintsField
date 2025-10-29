using System.Collections.Generic;
using UnityEngine;

namespace SaintsField.Samples.Scripts.SaintsEditor.Issues.Issue56
{
    public class GetComponentInParentExample : SaintsMonoBehaviour
    {
        [GetComponentInParents, EndText(nameof(DummyNumber), isCallback: true)] public Dummy[] parentsWithSelf;
        [GetComponentInParents(excludeSelf: true), EndText(nameof(DummyNumber), isCallback: true)] public Dummy[] parentsNoSelf;
        [GetComponentInParents(includeInactive: true), EndText(nameof(DummyNumber), isCallback: true)] public Dummy[] parentsWithSelfInactive;
        [GetComponentInParents(includeInactive: true, excludeSelf: true), EndText(nameof(DummyNumber), isCallback: true)] public Dummy[] parentsNoSelfInactive;

        [Space]
        [GetComponentInParent, EndText(nameof(DummyNumber), isCallback: true)] public Dummy[] parentWithSelf;
        [GetComponentInParent(excludeSelf: true), EndText(nameof(DummyNumber), isCallback: true)] public Dummy[] parentNoSelf;

        private string DummyNumber(Dummy dummy)
        {
            return dummy? $"{dummy.comment}": "";
        }
    }
}
