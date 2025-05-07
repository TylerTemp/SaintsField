using System;
using SaintsField.Samples.Scripts.IssueAndTesting.Issue46;
using UnityEngine;
using Object = UnityEngine.Object;

namespace SaintsField.Samples.Scripts.IssueAndTesting.Issue.Issue46
{
    public class MCPhysicsRelay: MonoBehaviour
    {
        // [SerializeReference] public IPhysicsReceiver receiver;

        public SaintsInterface<Object, IPhysicsReceiver> receiver1;

        // For old Unity, use this instead
        [Serializable]
        public class PhysicsReceiverInterface : SaintsInterface<Object, IPhysicsReceiver>
        {
            public PhysicsReceiverInterface(Object obj) : base(obj)
            {
            }
        }

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
