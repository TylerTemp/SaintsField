using System;
using System.Reflection;

namespace SaintsField.Events
{
    internal abstract class BaseInvokableCall
    {
        protected BaseInvokableCall()
        {
        }

        protected BaseInvokableCall(object target, MethodInfo function)
        {
            if (function == null)
                throw new ArgumentNullException(nameof (function));
            if (function.IsStatic)
            {
                if (target != null)
                    throw new ArgumentException("target must be null");
            }
            else if (target == null)
                throw new ArgumentNullException(nameof (target));
        }

        public abstract void Invoke(object[] args);

        protected static void ThrowOnInvalidArg<T>(object arg)
        {
            if (arg != null && !(arg is T))
            {
                throw new ArgumentException(
                    $"Passed argument 'args[0]' is of the wrong type. Type:{(object)arg.GetType()} Expected:{(object)typeof(T)}");
            }
        }

        protected static bool AllowInvoke(Delegate @delegate)
        {
            object target = @delegate.Target;
            return target == null || !(target is UnityEngine.Object @object) || @object != null;
        }

        public abstract bool Find(object targetObj, MethodInfo method);
    }
}
