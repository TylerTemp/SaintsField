using System;
using System.Reflection;
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

        [Button]
        private void RunPatch2RegressionTests()
        {
            _persistentInt = 0;
            _persistentString = null;
            _persistentFloat = 0f;

            TestSaintsEvent<int, string, float> testEvent = new TestSaintsEvent<int, string, float>();
            testEvent.SetPersistentCalls(new PersistentCall
            {
                callState = UnityEventCallState.EditorAndRuntime,
                methodName = nameof(PersistentPatch2Probe),
                target = this,
                persistentArguments = new[]
                {
                    new PersistentArgument
                    {
                        callType = PersistentArgument.CallType.Dynamic,
                        invokedParameterIndex = 0,
                    },
                    CreateSerializedStringArgument("serialized"),
                    new PersistentArgument
                    {
                        callType = PersistentArgument.CallType.OptionalDefault,
                    },
                },
            });

            SetArgumentType(testEvent.PersistentCalls[0].persistentArguments[0], typeof(int));
            SetArgumentType(testEvent.PersistentCalls[0].persistentArguments[2], typeof(float));

            testEvent.Invoke(1, "ignored", 99f);
            Assert.AreEqual(1, _persistentInt);
            Assert.AreEqual("serialized", _persistentString);
            Assert.AreEqual(2.5f, _persistentFloat);

            bool receivedDirectException = false;
            try
            {
                testEvent.Invoke(2, "ignored again", 100f);
            }
            catch (InvalidOperationException exception) when (exception.Message == Patch2ExceptionMessage)
            {
                receivedDirectException = true;
            }

            Assert.IsTrue(receivedDirectException,
                "Persistent listener still used MethodInfo.Invoke, which wraps the callback exception.");
            Debug.Log("SaintsEvent patch 2 typed persistent delegate test passed.", this);
        }

        [Button]
        private void RunPatch3RegressionTests()
        {
            string invocationOrder = "";
            UnityAction<int, string, float> selfRemovingListener = null;
            selfRemovingListener = (intValue, stringValue, floatValue) =>
            {
                invocationOrder += "A";
                _threeArguments.RemoveListener(selfRemovingListener);
            };
            UnityAction<int, string, float> remainingListener = (intValue, stringValue, floatValue) =>
            {
                invocationOrder += "B";
            };

            _threeArguments.AddListener(selfRemovingListener);
            _threeArguments.AddListener(remainingListener);
            try
            {
                _threeArguments.Invoke(1, "first", 1f);
                Assert.AreEqual("AB", invocationOrder,
                    "Runtime listeners did not use multicast snapshot semantics during removal.");

                _threeArguments.Invoke(2, "second", 2f);
                Assert.AreEqual("ABB", invocationOrder,
                    "The self-removing runtime listener was invoked more than once.");
            }
            finally
            {
                _threeArguments.RemoveListener(selfRemovingListener);
                _threeArguments.RemoveListener(remainingListener);
            }

            Debug.Log("SaintsEvent patch 3 direct multicast runtime listener test passed.", this);
        }

        [Button]
        private void RunPatch4RegressionTests()
        {
            const int invokeCount = 1000;
            SaintsEvent<int> testEvent = new SaintsEvent<int>();
            UnityAction<int> listener = Patch4ARuntimeListener;
            testEvent.AddListener(listener);
            testEvent.Invoke(0);
            _patch4AInvocationCount = 0;

            long allocatedBefore = GC.GetAllocatedBytesForCurrentThread();
            for (int index = 0; index < invokeCount; index++)
            {
                testEvent.Invoke(index);
            }
            long allocatedBytes = GC.GetAllocatedBytesForCurrentThread() - allocatedBefore;

            testEvent.RemoveListener(listener);
            Assert.AreEqual(invokeCount, _patch4AInvocationCount);
            Assert.AreEqual(0L, allocatedBytes,
                $"Runtime-only SaintsEvent<int> allocated {allocatedBytes} bytes during {invokeCount} invokes.");
            Debug.Log("SaintsEvent patch 4a zero-allocation runtime-only test passed.", this);
        }

        private int _patch4AInvocationCount;

        private void Patch4ARuntimeListener(int value)
        {
            _patch4AInvocationCount++;
        }

        private const string Patch2ExceptionMessage = "patch-2-direct-delegate";
        private int _persistentInt;
        private string _persistentString;
        private float _persistentFloat;

        private int PersistentPatch2Probe(int dynamicValue, string serializedValue, float optionalValue = 2.5f)
        {
            if (dynamicValue == 2)
            {
                throw new InvalidOperationException(Patch2ExceptionMessage);
            }

            _persistentInt = dynamicValue;
            _persistentString = serializedValue;
            _persistentFloat = optionalValue;
            return dynamicValue;
        }

        private static PersistentArgument CreateSerializedStringArgument(string value)
        {
            PersistentArgument argument = new PersistentArgument
            {
                callType = PersistentArgument.CallType.Serialized,
            };
            SetArgumentType(argument, typeof(string));
            argument.SerializeObject = value;
            return argument;
        }

        private static void SetArgumentType(PersistentArgument argument, Type type)
        {
            FieldInfo field = typeof(PersistentArgument).GetField("typeReference");
            Assert.IsNotNull(field);
            object typeReference = Activator.CreateInstance(field.FieldType, type);
            field.SetValue(argument, typeReference);
        }

        [Serializable]
        private sealed class TestSaintsEvent<T0, T1, T2>: SaintsEvent<T0, T1, T2>
        {
            public void SetPersistentCalls(params PersistentCall[] calls) => _persistentCalls = calls;
            public PersistentCall[] PersistentCalls => _persistentCalls;
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
