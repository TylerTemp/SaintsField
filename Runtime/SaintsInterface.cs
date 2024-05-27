using System;
using SaintsField.Utils;
using UnityEngine;

namespace SaintsField
{
    [Serializable]
    public class SaintsInterface<TObject, TInterface>: ISaintsInterface where TObject: UnityEngine.Object where TInterface: class
    {
        [field: SerializeField]
        public TObject V { get; private set; }

        public TInterface I => V as TInterface;

#if UNITY_EDITOR
        public virtual string EditorValuePropertyName => nameof(V);
        public virtual bool EditorCustomPicker => true;
#endif
    }
}
