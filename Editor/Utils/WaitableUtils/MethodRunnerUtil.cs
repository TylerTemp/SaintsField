using System;
using System.Collections;
using System.Reflection;
using System.Threading.Tasks;

#if SAINTSFIELD_UNITASK && !SAINTSFIELD_UNITASK_DISABLE
using Cysharp.Threading.Tasks;
#endif

namespace SaintsField.Editor.Utils.WaitableUtils
{
    public static class MethodRunnerUtil
    {
        public enum RunStatus
        {
            Pending,
            Completed,
            Faulted,
            Cancelled,
        }

        public class Payload
        {
            public readonly MethodInfo MethodInfo;
            public RunStatus Status;
            public Exception Exception;
            public Waiter Waiter;
            public Type ReturnType;
            public object ReturnValue;

            public float Progress => Waiter?.GetProgress() ?? -1f;

            public Payload(MethodInfo methodInfo)
            {
                MethodInfo = methodInfo;
                Status = RunStatus.Completed;
            }
        }

        public readonly struct ReturnValueInfo
        {
            public readonly Type Type;
            public readonly object Value;

            public ReturnValueInfo(Type type, object value)
            {
                Type = type;
                Value = value;
            }
        }

        public static Payload Invoke(MethodInfo methodInfo, object target, object[] parameters)
        {
            Payload payload = new Payload(methodInfo);
            try
            {
                SetResult(payload, methodInfo.Invoke(target, parameters));
            }
            catch (Exception exception)
            {
                payload.Status = RunStatus.Faulted;
                payload.Exception = exception;
            }

            return payload;
        }

        public static Payload FromResult(MethodInfo methodInfo, object result)
        {
            Payload payload = new Payload(methodInfo);
            SetResult(payload, result);
            return payload;
        }

        public static RunStatus Tick(Payload payload)
        {
            if (payload.Status != RunStatus.Pending)
            {
                return payload.Status;
            }

            Waiter waiter = payload.Waiter;
            waiter.Update();
            if (!waiter.SubWaiterDone())
            {
                return payload.Status;
            }

            Waiter.MoveNextResult moveNext = waiter.MoveNext();
            payload.Exception = moveNext.Exception;

            switch (moveNext.Status)
            {
                case Waiter.MoveNextStatus.Pending:
                    if (moveNext.Exception == null)
                    {
                        waiter.CheckCurrentNeedWaiter();
                    }
                    break;
                case Waiter.MoveNextStatus.Completed:
                    payload.Status = RunStatus.Completed;
                    payload.ReturnType = moveNext.ReturnType;
                    payload.ReturnValue = moveNext.ReturnValue;
                    break;
                case Waiter.MoveNextStatus.Faulted:
                    payload.Status = RunStatus.Faulted;
                    break;
                case Waiter.MoveNextStatus.Cancelled:
                    payload.Status = RunStatus.Cancelled;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            return payload.Status;
        }

        public static (bool hasReturnValue, ReturnValueInfo returnValueInfo) GetReturnValue(Payload payload)
        {
            if (payload.Status == RunStatus.Completed && payload.ReturnType != null)
            {
                return (true, new ReturnValueInfo(payload.ReturnType, payload.ReturnValue));
            }

            return (false, default);
        }

        public static bool IsWaitableReturnType(Type returnType)
        {
            if (typeof(IEnumerator).IsAssignableFrom(returnType)
                || typeof(Task).IsAssignableFrom(returnType))
            {
                return true;
            }

#if UNITY_6000_0_OR_NEWER
            if (typeof(UnityEngine.Awaitable).IsAssignableFrom(returnType)
                || GetGenericReturnType(returnType, typeof(UnityEngine.Awaitable<>)) != null)
            {
                return true;
            }
#endif

#if SAINTSFIELD_UNITASK && !SAINTSFIELD_UNITASK_DISABLE
            if (typeof(UniTask).IsAssignableFrom(returnType)
                || GetGenericReturnType(returnType, typeof(UniTask<>)) != null)
            {
                return true;
            }
#endif

            return false;
        }

        private static void SetResult(Payload payload, object result)
        {
#if UNITY_6000_0_OR_NEWER
            if (result is UnityEngine.Awaitable awaitable)
            {
                SetWaiter(payload, new Waiter(awaitable));
                return;
            }

            Type awaitableReturnType = GetGenericReturnType(payload.MethodInfo.ReturnType,
                typeof(UnityEngine.Awaitable<>));
            if (awaitableReturnType != null && result != null)
            {
                SetWaiter(payload, Waiter.AwaitableT(result, awaitableReturnType));
                return;
            }
#endif

            if (result is IEnumerator enumerator)
            {
                SetWaiter(payload, new Waiter(enumerator));
                return;
            }

            if (result is Task task)
            {
                SetWaiter(payload, new Waiter(task));
                return;
            }

#if SAINTSFIELD_UNITASK && !SAINTSFIELD_UNITASK_DISABLE
            if (result is UniTask uniTask)
            {
                SetWaiter(payload, new Waiter(uniTask));
                return;
            }

            Type uniTaskReturnType = GetGenericReturnType(payload.MethodInfo.ReturnType, typeof(UniTask<>));
            if (uniTaskReturnType != null && result != null)
            {
                SetWaiter(payload, Waiter.UniTaskWithValue(result, uniTaskReturnType));
                return;
            }
#endif

            payload.Status = RunStatus.Completed;
            if (payload.MethodInfo.ReturnType != typeof(void) && !IsWaitableReturnType(payload.MethodInfo.ReturnType))
            {
                payload.ReturnType = payload.MethodInfo.ReturnType;
                payload.ReturnValue = result;
            }
        }

        private static void SetWaiter(Payload payload, Waiter waiter)
        {
            payload.Waiter = waiter;
            payload.Status = RunStatus.Pending;
        }

        private static Type GetGenericReturnType(Type type, Type genericTypeDefinition)
        {
            for (Type current = type; current != null; current = current.BaseType)
            {
                if (current.IsGenericType && current.GetGenericTypeDefinition() == genericTypeDefinition)
                {
                    return current.GetGenericArguments()[0];
                }
            }

            return null;
        }
    }
}
