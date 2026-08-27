using System;
using System.Reflection;

// ReSharper disable once CheckNamespace
namespace SaintsField.Events
{
    public abstract class CachedInvoker
    {
        protected readonly PersistentArgument[] Arguments;
        protected readonly object[] OptionalDefaults;

        protected CachedInvoker(PersistentArgument[] arguments, object[] optionalDefaults)
        {
            Arguments = arguments;
            OptionalDefaults = optionalDefaults;
        }

        public abstract void Invoke(object[] eventArguments);

        protected static Delegate CreateDelegate(Type delegateType, MethodInfo methodInfo, object invokeTarget)
        {
            return methodInfo.IsStatic
                ? Delegate.CreateDelegate(delegateType, methodInfo)
                : Delegate.CreateDelegate(delegateType, invokeTarget, methodInfo);
        }

        protected (bool found, T value) TryGetArgument<T>(int index, object[] eventArguments)
        {
            PersistentArgument argument = Arguments[index];
            object rawValue;
            switch (argument.callType)
            {
                case PersistentArgument.CallType.Dynamic:
                    int eventIndex = argument.invokedParameterIndex;
                    if (eventIndex < 0 || eventIndex >= eventArguments.Length)
                    {
                        return (false, default);
                    }
                    rawValue = eventArguments[eventIndex];
                    break;
                case PersistentArgument.CallType.Serialized:
                    rawValue = argument.isUnityObject ? argument.unityObject : argument.SerializeObject;
                    break;
                case PersistentArgument.CallType.OptionalDefault:
                    rawValue = OptionalDefaults[index];
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(argument.callType), argument.callType, null);
            }

            return (true, (T)rawValue);
        }
    }
}
