using SaintsField.Events;
using SaintsField.Playa;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Events;

namespace SaintsField.Samples.Scripts.SaintsEventExamples
{
    public class EventTestMatch : SaintsMonoBehaviour
    {
        [SerializeField] private SaintsEvent _noArguments = new SaintsEvent();
        [SerializeField] private SaintsEvent<int, string, float> _threeArguments = new SaintsEvent<int, string, float>();
        [SerializeField] private SaintsEvent<int, string, float, bool> _fourArguments = new SaintsEvent<int, string, float, bool>();

        [Button]
        private void RunPatch1RegressionTests()
        {
            TestNoArgumentRuntimeListeners();
            TestThreeArgumentRuntimeListener();
            TestFourArgumentRuntimeListener();
            Debug.Log("SaintsEvent patch 1 regression tests passed.", this);
        }

        private void TestNoArgumentRuntimeListeners()
        {
            int invocationCount = 0;
            UnityAction listener = () => invocationCount++;

            _noArguments.AddListener(listener);
            _noArguments.Invoke();
            Assert.AreEqual(1, invocationCount, "A zero-argument runtime listener was not invoked.");

            _noArguments.RemoveListener(listener);
            _noArguments.Invoke();
            Assert.AreEqual(1, invocationCount, "RemoveListener did not remove the zero-argument listener.");

            _noArguments.AddListener(listener);
            _noArguments.RemoveAllListeners();
            _noArguments.Invoke();
            Assert.AreEqual(1, invocationCount, "RemoveAllListeners did not remove the zero-argument listener.");
        }

        private void TestThreeArgumentRuntimeListener()
        {
            int actualInt = 0;
            string actualString = null;
            float actualFloat = 0f;
            UnityAction<int, string, float> listener = (intValue, stringValue, floatValue) =>
            {
                actualInt = intValue;
                actualString = stringValue;
                actualFloat = floatValue;
            };

            _threeArguments.AddListener(listener);
            try
            {
                _threeArguments.Invoke(3, "three", 3.5f);
            }
            finally
            {
                _threeArguments.RemoveListener(listener);
            }

            Assert.AreEqual(3, actualInt);
            Assert.AreEqual("three", actualString);
            Assert.AreEqual(3.5f, actualFloat);
        }

        private void TestFourArgumentRuntimeListener()
        {
            int actualInt = 0;
            string actualString = null;
            float actualFloat = 0f;
            bool actualBool = false;
            UnityAction<int, string, float, bool> listener = (intValue, stringValue, floatValue, boolValue) =>
            {
                actualInt = intValue;
                actualString = stringValue;
                actualFloat = floatValue;
                actualBool = boolValue;
            };

            _fourArguments.AddListener(listener);
            try
            {
                _fourArguments.Invoke(4, "four", 4.5f, true);
            }
            finally
            {
                _fourArguments.RemoveListener(listener);
            }

            Assert.AreEqual(4, actualInt);
            Assert.AreEqual("four", actualString);
            Assert.AreEqual(4.5f, actualFloat);
            Assert.IsTrue(actualBool);
        }
    }
}
