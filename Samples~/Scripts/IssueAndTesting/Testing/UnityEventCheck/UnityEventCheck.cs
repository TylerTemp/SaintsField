using SaintsField.Playa;
using UnityEngine;
using UnityEngine.Events;

namespace SaintsField.Samples.Scripts.IssueAndTesting.Testing.UnityEventCheck
{
    public class UnityEventCheck : SaintsMonoBehaviour
    {
        [ShowInInspector]
        [SerializeField] private UnityEvent _event = new UnityEvent();

        [ShowInInspector]
        [Separator(80)]
        [SerializeField] private UnityEvent<int, GameObject> _eventWithParameters = new UnityEvent<int, GameObject>();

        [Separator(80)]

        [Button]
        private void AddInstanceListener()
        {
            _event.AddListener(InstanceCallback);
            Debug.Log("Added instance listener.", this);
        }

        [Button]
        private void AddStaticListener()
        {
            _event.AddListener(StaticCallback);
            Debug.Log("Added static listener.", this);
        }

        private int _lambdaCaptureValue;

        [Button]
        private void AddCapturedLambdaListener()
        {
            int captureValue = ++_lambdaCaptureValue;
            _event.AddListener(CapturedLambdaListener);
            Debug.Log($"Added captured lambda. Capture value={captureValue}.", this);
            return;

            void CapturedLambdaListener() => Debug.Log($"Captured lambda invoked. Captured value={captureValue}.", this);
        }

        [Button]
        private void AddParameterizedListener()
        {
            _eventWithParameters.AddListener(ParameterizedCallback);
            Debug.Log("Added parameterized listener.", this);
        }

        private void InstanceCallback()
        {
            Debug.Log($"Instance callback invoked on {name}.", this);
        }

        private void ParameterizedCallback(int intValue, GameObject go)
        {
            Debug.Log($"invoked: intValue={intValue}, GameObject={go}.", this);
        }

        private static void StaticCallback()
        {
            Debug.Log("Static callback invoked.");
        }

        [OnInspectorInit]
        private void EditorInit()
        {
            AddInstanceListener();
            AddStaticListener();
            AddCapturedLambdaListener();
            AddParameterizedListener();
        }
    }
}
