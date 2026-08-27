using System;
using System.Reflection;

// ReSharper disable once CheckNamespace
namespace SaintsField.Events
{
    public class VoidInvoker: CachedInvoker
    {
        private readonly Action _call;

        public VoidInvoker(MethodInfo method, object target, PersistentArgument[] args, object[] defaults)
            : base(args, defaults) => _call = (Action)CreateDelegate(typeof(Action), method, target);

        public override void Invoke(object[] eventArguments)
        {
            _call();
        }
    }
}
