using System;
using System.Collections;
using System.Reflection;
using System.Threading.Tasks;
using UnityEngine;

#if SAINTSFIELD_UNITASK && !SAINTSFIELD_UNITASK_DISABLE
using Cysharp.Threading.Tasks;
#endif

namespace SaintsField.Editor.Utils.WaitableUtils
{
    public class Waiter
    {
        private IEnumerator _enumerator;
        private readonly Task _task;
        private Type _taskReturnType;

        private IWaitable _waitable;
        private Exception _overrideException;
        private object _overrideReturnValue;

        private Waiter()
        {
        }

        public Waiter(IEnumerator enumerator)
        {
            _enumerator = enumerator;
        }

        public Waiter(Task task)
        {
            _task = task;

            Type taskType = task.GetType();
            _taskReturnType = GetTaskReturnType(taskType);
        }
        public Waiter(Task task, Type taskReturnType)
        {
            _task = task;
            _taskReturnType = taskReturnType;
        }

#if UNITY_6000_0_OR_NEWER
        private Awaitable _awaitable;
        public Waiter(Awaitable awaitable)
        {
            _awaitable = awaitable;
        }

        private object _awaitableWithReturn;
        private object _awaitableWithReturnAwaiter;
        private PropertyInfo _awaitableWithReturnIsCompleted;
        private Delegate _awaitableWithReturnGetResultDelegate;

        // object awaitable must be Awaitable<T>, Type awaitableReturn is typeof(T)
        public static Waiter AwaitableT(object awaitable, Type awaitableReturn)
        {
            Type awaitableType = awaitable.GetType();
            // Type awaitableReturn = awaitableType.GetGenericArguments()[0];

            MethodInfo getAwaiter = awaitableType.GetMethod(
                "GetAwaiter",
                BindingFlags.Instance | BindingFlags.Public
            )!;

            object awaiter = getAwaiter.Invoke(awaitable, null)!;
            Type awaiterType = awaiter.GetType();
            PropertyInfo awaitableIsCompleted = awaiterType.GetProperty(
                "IsCompleted",
                BindingFlags.Instance | BindingFlags.Public
            )!;
            MethodInfo awaitableGetResult = awaiterType.GetMethod(
                "GetResult",
                BindingFlags.Instance | BindingFlags.Public
            )!;
            Delegate getResultDelegate = awaitableGetResult.CreateDelegate(
                typeof(Func<>).MakeGenericType(awaitableReturn),
                awaiter);

            return new Waiter
            {
                _awaitableWithReturn = awaitable,
                _taskReturnType = awaitableReturn,

                _awaitableWithReturnAwaiter = awaiter,
                _awaitableWithReturnIsCompleted = awaitableIsCompleted,
                _awaitableWithReturnGetResultDelegate = getResultDelegate,
            };
        }
#endif

        public static Type GetTaskReturnType(Type type)
        {
            Type taskType = type;

            while (taskType != null)
            {
                if (taskType.IsGenericType && taskType.GetGenericTypeDefinition() == typeof(Task<>))
                {
                    Type taskReturnType = taskType.GetGenericArguments()[0];
                    // ReSharper disable once ConvertIfStatementToConditionalTernaryExpression
                    if (taskReturnType.FullName == "System.Threading.Tasks.VoidTaskResult"
                        && taskReturnType.Assembly.GetName().Name == "mscorlib")
                    {
                        return null;
                    }

                    return taskReturnType;
                    // Debug.Log($"{task.GetType().FullName}/{_taskReturnType.FullName}");
                }

                taskType = taskType.BaseType;
            }

            return null;
        }

#if SAINTSFIELD_UNITASK && !SAINTSFIELD_UNITASK_DISABLE
        public Waiter(UniTask uniTask)
        {
            _enumerator = uniTask.ToCoroutine(error =>
            {
                _overrideException = error;
            });
        }

        // returnValue is ensured be passed in a UniTask<returnUniTaskValueType>, no need to check
        public static Waiter UniTaskWithValue(object returnValue, Type returnUniTaskValueType)
        {
            MethodInfo methodInfo = typeof(Waiter).GetMethod(nameof(UniTaskWithValueTyped),
                BindingFlags.Static | BindingFlags.NonPublic);
            return (Waiter) methodInfo!.MakeGenericMethod(returnUniTaskValueType).Invoke(null, new[] { returnValue });
        }

        private static Waiter UniTaskWithValueTyped<T>(UniTask<T> returnValue)
        {
            Waiter waiter = new Waiter
            {
                _taskReturnType = typeof(T),
            };
            waiter._enumerator = returnValue.ToCoroutine(
                result => waiter._overrideReturnValue = result,
                error => waiter._overrideException = error
            );
            return waiter;
        }
#endif

        public void CheckCurrentNeedWaiter()
        {
            if (_enumerator == null)
            {
                return;
            }
            switch (_enumerator.Current)
            {
                case null:
                    _waitable = null;
                    return;
                case WaitForSeconds ws:
                    _waitable = new WaitableWaitForSeconds(ws);
                    return;
                case WaitForSecondsRealtime ws:
                    _waitable = new WaitableWaitForSeconds(ws);
                    return;
                case WaitUntil wu:
                    _waitable = new WaitableWaitForCallback(wu);
                    return;
                case WaitWhile ww:
                    _waitable = new WaitableWaitForCallback(ww);
                    return;
                case AsyncOperation asyncOperation:
                    _waitable = new WaitableWaitForAsyncOperation(asyncOperation);
                    return;
                default:
                    _waitable = null;
                    return;
            }
        }

        public void Update()
        {
            if (_waitable != null && !SubWaiterDone())
            {
                _waitable.Update();
            }
        }

        public bool SubWaiterDone()
        {
            if (_task != null)
            {
                return _task.IsCompleted;
            }

#if UNITY_6000_0_OR_NEWER
            if (_awaitable != null)
            {
                return _awaitable.IsCompleted;
            }
            if (_awaitableWithReturn != null)
            {
                return (bool)_awaitableWithReturnIsCompleted.GetValue(_awaitableWithReturnAwaiter);
            }
#endif

            if (_overrideException != null)
            {
                return true;
            }

            // ReSharper disable once ConvertIfStatementToReturnStatement
            if (_waitable is null)
            {
                return true;
            }
            return _waitable.Done;
        }

        public enum MoveNextStatus
        {
            Pending,
            Completed,
            Faulted,
            Cancelled,
        }

        public readonly struct MoveNextResult
        {
            public readonly MoveNextStatus Status;
            public readonly Exception Exception;
            public readonly Type ReturnType;
            public readonly object ReturnValue;

            public MoveNextResult(MoveNextStatus status, Exception exception=null, Type taskReturnType=null, object taskReturnValue=null)
            {
                Status = status;
                Exception = exception;
                ReturnType = taskReturnType;
                ReturnValue = taskReturnValue;
            }
        }

        public MoveNextResult MoveNext()
        {
            if (_enumerator == null)
            {
#if UNITY_6000_0_OR_NEWER
                if (_awaitable != null)
                {
                    if (!_awaitable.IsCompleted)
                    {
                        return new MoveNextResult(MoveNextStatus.Pending);
                    }

                    try
                    {
                        _awaitable.GetAwaiter().GetResult();

                        _awaitable = null;
                        return new MoveNextResult(
                            MoveNextStatus.Completed,
                            taskReturnType: _taskReturnType);
                    }
                    catch (OperationCanceledException e)
                    {
                        _awaitable = null;
                        return new MoveNextResult(
                            MoveNextStatus.Cancelled,
                            exception: e,
                            taskReturnType: _taskReturnType);
                    }
                    catch (Exception e)
                    {
                        _awaitable = null;
                        return new MoveNextResult(
                            MoveNextStatus.Faulted,
                            exception: e,
                            taskReturnType: _taskReturnType);
                    }
                }
                if (_awaitableWithReturn != null)
                {
                    bool completed = (bool)_awaitableWithReturnIsCompleted.GetValue(_awaitableWithReturnAwaiter);
                    if (!completed)
                    {
                        return new MoveNextResult(MoveNextStatus.Pending);
                    }

                    try
                    {
                        object result = _awaitableWithReturnGetResultDelegate.DynamicInvoke();
                        _awaitableWithReturn = null;
                        return new MoveNextResult(
                            MoveNextStatus.Completed,
                            taskReturnType: _taskReturnType,
                            taskReturnValue: result);
                    }
                    catch (OperationCanceledException e)
                    {
                        _awaitableWithReturn = null;
                        return new MoveNextResult(
                            MoveNextStatus.Cancelled,
                            exception: e,
                            taskReturnType: _taskReturnType);
                    }
                    catch (Exception e)
                    {
                        _awaitableWithReturn = null;
                        return new MoveNextResult(
                            MoveNextStatus.Faulted,
                            exception: e,
                            taskReturnType: _taskReturnType);
                    }
                }
#endif

                if (!_task.IsCompleted)
                {
                    return new MoveNextResult(MoveNextStatus.Pending);
                }

                if (_task.IsFaulted)
                {
                    return new MoveNextResult(MoveNextStatus.Faulted, _task.Exception, taskReturnType: _taskReturnType);
                }

                if (_task.IsCanceled)
                {
                    return new MoveNextResult(MoveNextStatus.Cancelled, taskReturnType: _taskReturnType);
                }

                object taskReturnValue = null;
                if (_taskReturnType != null)
                {
                    taskReturnValue = _task.GetType().GetProperty(nameof(Task<object>.Result))?.GetValue(_task);
                }

                return new MoveNextResult(MoveNextStatus.Completed, taskReturnType: _taskReturnType, taskReturnValue: taskReturnValue);

            }

            if (_overrideException != null)
            {
                return new MoveNextResult(MoveNextStatus.Faulted, _overrideException, _taskReturnType);
            }

            try
            {
                bool pending = _enumerator.MoveNext();
                if (_overrideException != null)  // task -> ie can change the override exception
                {
                    return new MoveNextResult(MoveNextStatus.Faulted, _overrideException, _taskReturnType);
                }

                if (pending)
                {
                    return new MoveNextResult(MoveNextStatus.Pending);
                }

                return new MoveNextResult(MoveNextStatus.Completed,
                    taskReturnType: _taskReturnType,
                    taskReturnValue: _overrideReturnValue ?? _enumerator.Current);
            }
            catch (Exception e)
            {
                _waitable = null;
                return new MoveNextResult(MoveNextStatus.Faulted, e);
            }
        }

        public float GetProgress() => _waitable?.Progress ?? -1f;
    }
}
