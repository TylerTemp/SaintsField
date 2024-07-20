using UnityEngine;

namespace SaintsField.Samples.Scripts
{
    public class GetComponentInParentsExample: MonoBehaviour
    {
        [GetComponentInParent] public SpriteRenderer directParent;
        [GetComponentInParent(typeof(SpriteRenderer))] public GameObject directParentDifferentType;
        [GetComponentInParent] public BoxCollider directNoSuch;

        [GetComponentInParents] public SpriteRenderer searchParent;
        [GetComponentInParents(true, typeof(SpriteRenderer))] public GameObject searchParentDifferentType;
        [GetComponentInParents] public BoxCollider searchNoSuch;
    }
}
