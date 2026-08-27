using System;
using System.Reflection;

// ReSharper disable once CheckNamespace
namespace SaintsField.Events
{
    public class ResultInvoker<TResult>: CachedInvoker
    {
        private readonly Func<TResult> _call;

        public ResultInvoker(MethodInfo method, object target, PersistentArgument[] args, object[] defaults)
            : base(args, defaults) => _call = (Func<TResult>)CreateDelegate(typeof(Func<TResult>), method, target);

        public override void Invoke(object[] eventArguments)
        {
            _call();
        }
    }
}
