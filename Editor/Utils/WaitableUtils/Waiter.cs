using System;
using System.Collections;
using System.Threading.Tasks;
using UnityEngine;

namespace SaintsField.Editor.Utils.WaitableUtils
{
    public class Waiter
    {
        private readonly IEnumerator Enumerator;
        private readonly Task Task;
        private readonly Type TaskReturnType;

        private IWaitable Waitable;

        public Waiter(IEnumerator enumerator)
        {
            Enumerator = enumerator;
        }

        public Waiter(Task task)
        {
            Task = task;

            Type taskType = task.GetType();
            while (taskType != null)
            {
                if (taskType.IsGenericType && taskType.GetGenericTypeDefinition() == typeof(Task<>))
                {
                    TaskReturnType = taskType.GetGenericArguments()[0];
                    break;
                }

                taskType = taskType.BaseType;
            }
        }

        public void CheckCurrentNeedWaiter()
        {
            if (Enumerator == null)
            {
                return;
            }
            switch (Enumerator.Current)
            {
                case null:
                    Waitable = null;
                    return;
                case WaitForSeconds ws:
                    Waitable = new WaitableWaitForSeconds(ws);
                    return;
                case WaitForSecondsRealtime ws:
                    Waitable = new WaitableWaitForSeconds(ws);
                    return;
                case WaitUntil wu:
                    Waitable = new WaitableWaitForCallback(wu);
                    return;
                case WaitWhile ww:
                    Waitable = new WaitableWaitForCallback(ww);
                    return;
                case AsyncOperation asyncOperation:
                    Waitable = new WaitableWaitForAsyncOperation(asyncOperation);
                    return;
                default:
                    Waitable = null;
                    return;
            }
        }

        public void Update()
        {
            if (!Done())
            {
                Waitable?.Update();
            }
        }

        public bool Done()
        {
            if (Task != null)
            {
                return Task.IsCompleted;
            }

            // ReSharper disable once ConvertIfStatementToReturnStatement
            if (Waitable is null)
            {
                return true;
            }
            return Waitable.Done;
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
            if (Enumerator == null)
            {
                if (Task.IsCompleted)
                {
                    if (Task.IsFaulted)
                    {
                        return new MoveNextResult(MoveNextStatus.Faulted, Task.Exception, taskReturnType: TaskReturnType);
                    }
                    else if (Task.IsCanceled)
                    {
                        return new MoveNextResult(MoveNextStatus.Cancelled, taskReturnType: TaskReturnType);
                    }

                    object taskReturnValue = null;
                    if (TaskReturnType != null)
                    {
                        taskReturnValue = Task.GetType().GetProperty(nameof(Task<object>.Result))?.GetValue(Task);
                    }

                    return new MoveNextResult(MoveNextStatus.Completed, taskReturnType: TaskReturnType, taskReturnValue: taskReturnValue);
                }
                else
                {
                    return new MoveNextResult(MoveNextStatus.Pending);
                }
            }
            else
            {
                try
                {
                    if (Enumerator.MoveNext())
                    {
                        return new MoveNextResult(MoveNextStatus.Pending);
                    }
                    else
                    {
                        return new MoveNextResult(MoveNextStatus.Completed);
                    }
                }
                catch (Exception e)
                {
                    Waitable = null;
                    return new MoveNextResult(MoveNextStatus.Faulted, e);
                }
            }
        }

        public float GetProgress() => Waitable?.Progress ?? -1f;
    }
}
