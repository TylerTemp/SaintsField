using SaintsField.Samples.Scripts;
using UnityEngine;

namespace SaintsField.Samples.IssueAndTesting.Issue.Issue42
{
    public class GetComponentError : MonoBehaviour
    {
        [GetComponent, GetComponentInChildren, GetComponentInParent, GetComponentInParents, GetComponentByPath("."), GetPrefabWithComponent, GetScriptableObject] public float getComponentNotSupport;

        [GetComponentInParents, GetComponentInParent] public Dummy noSuchParent;
    }
}
