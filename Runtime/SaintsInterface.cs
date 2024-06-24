using System;
using UnityEngine;

namespace SaintsField
{
    [Serializable]
    public class SaintsInterface<TObject, TInterface>: IWrapProp where TObject: UnityEngine.Object where TInterface: class
    {
        [field: SerializeField]
        public TObject V { get; private set; }

        public TInterface I => V as TInterface;

#if UNITY_EDITOR
        public virtual string EditorPropertyName => nameof(V);
        public virtual bool EditorCustomPicker => true;
#endif
    }
}
