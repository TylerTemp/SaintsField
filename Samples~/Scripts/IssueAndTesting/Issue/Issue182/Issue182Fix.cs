using SaintsField.Playa;
using SaintsField.Samples.Scripts.SaintsEditor;
using UnityEngine;

namespace SaintsField.Samples.Scripts.IssueAndTesting.Issue.Issue182
{
    public class Issue182Fix : SaintsMonoBehaviour
    {
        [Comment2("remove this armor from this parent's Damage Handler when detaching")]
        public Transform unitParent;

        [LayoutStart("On Detach", ELayout.TitleBox)]
        public bool destroySelfUnit,
            destroyThisComponent,
            unparent;
    }
}
