using System;
using System.Reflection;

// ReSharper disable once CheckNamespace
namespace SaintsField.Events
{
    public class ReflectionInvoker: CachedInvoker
    {
        private readonly MethodInfo _method;
        private readonly object _target;

        public ReflectionInvoker(MethodInfo method, object target, PersistentArgument[] args, object[] defaults)
            : base(args, defaults)
        {
            _method = method;
            _target = target;
        }

        public override void Invoke(object[] eventArguments)
        {
            object[] values = new object[Arguments.Length];
            for (int index = 0; index < values.Length; index++)
            {
                PersistentArgument argument = Arguments[index];
                switch (argument.callType)
                {
                    case PersistentArgument.CallType.Dynamic:
                        int eventIndex = argument.invokedParameterIndex;
                        if (eventIndex < 0 || eventIndex >= eventArguments.Length)
                        {
                            return;
                        }
                        values[index] = eventArguments[eventIndex];
                        break;
                    case PersistentArgument.CallType.Serialized:
                        values[index] = argument.isUnityObject ? argument.unityObject : argument.SerializeObject;
                        break;
                    case PersistentArgument.CallType.OptionalDefault:
                        values[index] = OptionalDefaults[index];
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(argument.callType), argument.callType, null);
                }
            }

            _method.Invoke(_target, values);
        }
    }
}
