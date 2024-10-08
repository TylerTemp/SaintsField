using UnityEngine;

namespace SaintsField.Samples.Scripts.IssueAndTesting.Issue.Issue88
{
    public struct ExampleStructOfInterface : IPhysicsRelayReceiver
    {
        public string DisplayName;

        public void OnTriggerEnterRelayed(Collider other)
        {
        }

        public void RegisterCollider(Collider col)
        {
        }
    }

    public class MCPhysicsRelay : MonoBehaviour, IPhysicsRelayReceiver
    {
#if UNITY_2021_3_OR_NEWER
        // example of non-UnityEngine.Object
        [SerializeReference, ReferencePicker]
        public IPhysicsRelayReceiver nonUnityObject;
#endif

        // example of UnityEngine.Object
        public SaintsInterface<UnityEngine.Object, IPhysicsRelayReceiver> unityObject;

        public void OnTriggerEnterRelayed(Collider other)
        {
        }

        public void RegisterCollider(Collider col)
        {
        }
    }
}
