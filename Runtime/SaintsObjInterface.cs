using System;

namespace SaintsField
{
    [Serializable]
    public class SaintsObjInterface<TInterface>: SaintsInterface<UnityEngine.Object, TInterface> where TInterface: class
    {

    }
}
