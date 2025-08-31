using System;
using System.Reflection;
using UnityEngine.Events;

namespace SaintsField.Events
{
    internal class InvokableCall<T0, T1> : BaseInvokableCall
    {
        protected event UnityAction<T0, T1> Delegate;

        public InvokableCall(UnityAction<T0, T1> action) => Delegate += action;

        public override void Invoke(object[] args)
        {
            if (args.Length != 2)
                throw new ArgumentException("Passed argument 'args' is invalid size. Expected size is 1");
            ThrowOnInvalidArg<T0>(args[0]);
            ThrowOnInvalidArg<T1>(args[1]);
            if (!AllowInvoke(Delegate))
                return;
            Delegate((T0) args[0], (T1) args[1]);
        }

        public override bool Find(object targetObj, MethodInfo method)
        {
            return Delegate.Target == targetObj && Delegate.Method.Equals(method);
        }
    }
}
