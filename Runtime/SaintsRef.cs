using System;
using UnityEngine;

namespace SaintsField
{
    [Serializable]
    public class SaintsRef<TObject, TInterface>: ISaintsRef<TObject, TInterface> where TObject: UnityEngine.Object where TInterface: class
    {
        [field: SerializeField]
        public TObject V { get; set; }

        public TInterface I => V as TInterface;

#if UNITY_EDITOR
        public string EditorValuePropertyName => nameof(V);
#endif
    }
}
