using SaintsField.Playa;
using SaintsField.Samples.Scripts.SaintsEditor;
using UnityEngine;

namespace SaintsField.Samples.Scripts.IssueAndTesting.Issue.Issue182
{
    public class Issue182Main : SaintsMonoBehaviour
    {
        [Comment("remove this armor from this parent's Damage Handler when detaching")]
        public Transform unitParent;

        [LayoutStart("On Detach", ELayout.Background|ELayout.Title)]
        public bool destroySelfUnit,
            destroyThisComponent,
            unparent;
    }
}
