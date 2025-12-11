using SaintsField.Samples.Scripts.SaintsEditor;
using UnityEngine;

namespace SaintsField.Samples.Scripts.IssueAndTesting.Testing
{
    public class PrefabDontRefToScene : SaintsMonoBehaviour
    {
        [FindObjectsByType] public Transform dontRefToSceneWhenInPrefab;

        [GetComponentInParents] public Transform[] dontRefToSceneParentsWhenInPrefab;
    }
}
