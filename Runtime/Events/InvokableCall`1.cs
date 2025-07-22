using System;
using System.Reflection;
using UnityEngine.Events;

namespace SaintsField.Events
{
    internal class InvokableCall<T0> : BaseInvokableCall
    {
        protected event UnityAction<T0> Delegate;

        // public InvokableCall(object target, MethodInfo theFunction)
        //     : base(target, theFunction)
        // {
        //     Delegate += (UnityAction<T1>)System.Delegate.CreateDelegate(typeof(UnityAction<T1>), target, theFunction);
        // }

        public InvokableCall(UnityAction<T0> action) => Delegate += action;

        public override void Invoke(object[] args)
        {
            if (args.Length != 1)
                throw new ArgumentException("Passed argument 'args' is invalid size. Expected size is 1");
            ThrowOnInvalidArg<T0>(args[0]);
            if (!AllowInvoke(Delegate))
                return;
            Delegate((T0)args[0]);
        }

        // public virtual void Invoke(T1 args0)
        // {
        //     if (!AllowInvoke(Delegate))
        //         return;
        //     Delegate(args0);
        // }

        public override bool Find(object targetObj, MethodInfo method)
        {
            return Delegate.Target == targetObj && Delegate.Method.Equals(method);
        }
    }
}
