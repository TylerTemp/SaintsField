using System;
using System.Reflection;

// ReSharper disable once CheckNamespace
namespace SaintsField.Events
{
    public class ResultInvoker<T0, T1, TResult>: CachedInvoker
    {
        private readonly Func<T0, T1, TResult> _call;

        public ResultInvoker(MethodInfo method, object target, PersistentArgument[] args, object[] defaults)
            : base(args, defaults) => _call = (Func<T0, T1, TResult>)CreateDelegate(typeof(Func<T0, T1, TResult>), method, target);

        public override void Invoke(object[] e)
        {
            (bool found0, T0 a0) = TryGetArgument<T0>(0, e);
            if (!found0)
            {
                return;
            }

            (bool found1, T1 a1) = TryGetArgument<T1>(1, e);
            if (found1)
            {
                _call(a0, a1);
            }
        }
    }
}
