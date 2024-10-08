using UnityEngine;

namespace SaintsField.Samples.Scripts.IssueAndTesting.Issue.Issue88
{
    public interface IPhysicsRelayReceiver
    {
        void OnTriggerEnterRelayed(Collider other);
        void RegisterCollider(Collider col);
    }
}
