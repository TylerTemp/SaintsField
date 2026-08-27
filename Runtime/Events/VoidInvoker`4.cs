using System;
using System.Reflection;

// ReSharper disable once CheckNamespace
namespace SaintsField.Events
{
    public class VoidInvoker<T0, T1, T2, T3>: CachedInvoker
    {
        private readonly Action<T0, T1, T2, T3> _call;

        public VoidInvoker(MethodInfo method, object target, PersistentArgument[] args, object[] defaults)
            : base(args, defaults) => _call = (Action<T0, T1, T2, T3>)CreateDelegate(typeof(Action<T0, T1, T2, T3>), method, target);

        public override void Invoke(object[] e)
        {
            (bool found0, T0 a0) = TryGetArgument<T0>(0, e);
            if (!found0)
            {
                return;
            }

            (bool found1, T1 a1) = TryGetArgument<T1>(1, e);
            if (!found1)
            {
                return;
            }

            (bool found2, T2 a2) = TryGetArgument<T2>(2, e);
            if (!found2)
            {
                return;
            }

            (bool found3, T3 a3) = TryGetArgument<T3>(3, e);
            if (found3)
            {
                _call(a0, a1, a2, a3);
            }
        }
    }
}
