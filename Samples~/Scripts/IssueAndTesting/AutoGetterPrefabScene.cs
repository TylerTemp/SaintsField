using UnityEngine;

namespace SaintsField.Samples.Scripts.IssueAndTesting
{
    public class AutoGetterPrefabScene : MonoBehaviour
    {
        [GetComponentInScene]
        public Dummy sceneDummy;
    }
}
