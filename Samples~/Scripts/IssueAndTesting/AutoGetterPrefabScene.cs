using UnityEngine;

namespace SaintsField.Samples.Scripts.IssueAndTesting
{
    public class AutoGetterPrefabScene : MonoBehaviour
    {
        [FindObjectsByType]
        public Dummy sceneDummy;
    }
}
