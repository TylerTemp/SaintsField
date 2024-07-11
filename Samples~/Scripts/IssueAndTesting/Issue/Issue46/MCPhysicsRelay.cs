using System;
using UnityEngine;

namespace SaintsField.Samples.Scripts.IssueAndTesting.Issue46
{
    public class MCPhysicsRelay: MonoBehaviour
    {
        // [SerializeReference] public IPhysicsReceiver receiver;

        public SaintsInterface<UnityEngine.Object, IPhysicsReceiver> receiver1;
        
        // For old Unity, use this instead
        [Serializable]
        public class PhysicsReceiverInterface : SaintsInterface<UnityEngine.Object, IPhysicsReceiver> { }

        public PhysicsReceiverInterface receiver2;
        
        // this also works with the GetComponent* decorators
        [GetComponentInChildren] public PhysicsReceiverInterface receiver3;
        
        // at this point it does not work with `RequireType` but I'm working on it

        private void Awake()
        {
            Debug.Log(receiver3.I);  // get the actual interface
        }
    }
}