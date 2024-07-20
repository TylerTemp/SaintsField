using System.Collections.Generic;
using UnityEngine;

namespace SaintsField.Samples.Scripts.SaintsEditor.Issues.Issue56
{
    public class GetComponentInParentExample : SaintsMonoBehaviour
    {
        [GetComponentInParents, PostFieldRichLabel(nameof(DummyNumber), isCallback: true)] public Dummy[] parentsWithSelf;
        [GetComponentInParents(excludeSelf: true), PostFieldRichLabel(nameof(DummyNumber), isCallback: true)] public Dummy[] parentsNoSelf;
        [GetComponentInParents(includeInactive: true), PostFieldRichLabel(nameof(DummyNumber), isCallback: true)] public Dummy[] parentsWithSelfInactive;
        [GetComponentInParents(includeInactive: true, excludeSelf: true), PostFieldRichLabel(nameof(DummyNumber), isCallback: true)] public Dummy[] parentsNoSelfInactive;

        [Space]
        [GetComponentInParent, PostFieldRichLabel(nameof(DummyNumber), isCallback: true)] public Dummy[] parentWithSelf;
        [GetComponentInParent(excludeSelf: true), PostFieldRichLabel(nameof(DummyNumber), isCallback: true)] public Dummy[] parentNoSelf;

        private string DummyNumber(Dummy dummy)
        {
            return dummy? $"{dummy.comment}": "";
        }
    }
}
