using System;
using UnityEngine;

namespace SaintsField
{
    [Serializable]
    public class SaintsInterface<TObject, TInterface>: ISaintsInterface<TObject, TInterface> where TObject: UnityEngine.Object where TInterface: class
    {
        [field: SerializeField]
        public TObject V { get; private set; }

        public TInterface I => V as TInterface;

#if UNITY_EDITOR
        public string EditorValuePropertyName => nameof(V);
#endif
    }
}
