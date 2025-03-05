using UnityEngine;

namespace SaintsField.Samples.Scripts.IssueAndTesting.Issue.Issue158
{
    public class Issue158Main : MonoBehaviour
    {
        [GetComponentInScene] public HVRTeleporter hvrTeleporter;
        [GetComponentInScene(includeInactive: true)] public HVRTeleporter hvrTeleporterInactive;
    }
}
