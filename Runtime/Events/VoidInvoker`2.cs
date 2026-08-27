using System;
using System.Reflection;

// ReSharper disable once CheckNamespace
namespace SaintsField.Events
{
    public class VoidInvoker<T0, T1>: CachedInvoker
    {
        private readonly Action<T0, T1> _call;

        public VoidInvoker(MethodInfo method, object target, PersistentArgument[] args, object[] defaults)
            : base(args, defaults) => _call = (Action<T0, T1>)CreateDelegate(typeof(Action<T0, T1>), method, target);

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
