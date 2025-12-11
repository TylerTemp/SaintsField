using UnityEngine;

namespace SaintsField.Samples.Scripts.IssueAndTesting.Issue.Issue158
{
    public class Issue158Main : MonoBehaviour
    {
        [FindObjectsByType] public HVRTeleporter hvrTeleporter;
        [FindObjectsByType(findObjectsInactive: true)] public HVRTeleporter hvrTeleporterInactive;
    }
}
