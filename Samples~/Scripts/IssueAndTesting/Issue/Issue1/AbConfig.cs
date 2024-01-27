using UnityEngine;

namespace SaintsField.Samples.Scripts.IssueAndTesting.Issue.Issue1
{
    [CreateAssetMenu(fileName = "AbConfig", menuName = "ScriptableObjects/Issue/Issue1", order = 1)]
    public class AbConfig : ScriptableObject
    {
        public string ABManifestName;
        public string ABExtensionName;
    }
}
