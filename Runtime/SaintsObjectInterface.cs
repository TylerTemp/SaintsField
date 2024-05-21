using UnityEngine;

namespace SaintsField
{
    public class SaintsObjectInterface<TInterface>: ISaintsInterface<UnityEngine.Object, TInterface> where TInterface: class
    {
        [field: SerializeField]
        public Object V { get; private set; }

        public TInterface I => V as TInterface;

#if UNITY_EDITOR
        public string EditorValuePropertyName => nameof(V);
#endif
    }
}
