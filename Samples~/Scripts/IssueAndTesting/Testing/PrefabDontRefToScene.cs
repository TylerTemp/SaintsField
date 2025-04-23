using SaintsField.Samples.Scripts.SaintsEditor;
using UnityEngine;

namespace SaintsField.Samples.Scripts.IssueAndTesting.Testing
{
    public class PrefabDontRefToScene : SaintsMonoBehaviour
    {
        [GetComponentInScene] public Transform dontRefToSceneWhenInPrefab;

        [GetComponentInParents] public Transform[] dontRefToSceneParentsWhenInPrefab;
    }
}
