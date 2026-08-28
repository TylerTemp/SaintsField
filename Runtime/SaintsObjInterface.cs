using System;

namespace SaintsField
{
    [Serializable, Obsolete("Please use SaintsInterface<T>")]
    public class SaintsObjInterface<TInterface>: SaintsInterface<UnityEngine.Object, TInterface> where TInterface: class
    {
        public SaintsObjInterface(UnityEngine.Object obj) : base(obj)
        {
        }
    }
}
