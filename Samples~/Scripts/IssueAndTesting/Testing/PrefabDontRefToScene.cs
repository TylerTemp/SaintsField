using UnityEngine;

namespace SaintsField.Samples.Scripts.IssueAndTesting.Testing
{
    public class PrefabDontRefToScene : MonoBehaviour
    {
        [GetComponentInScene] public Transform dontRefToSceneWhenInPrefab;
    }
}
