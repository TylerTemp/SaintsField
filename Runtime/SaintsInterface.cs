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

        public static implicit operator TInterface(SaintsInterface<TObject, TInterface> saintsInterface) => saintsInterface.I;
        public static implicit operator TObject(SaintsInterface<TObject, TInterface> saintsInterface) => saintsInterface.V;

        public override string ToString() => $"<Interface I={I} V={V}/>";

        public override bool Equals(object obj)
        {
            // Debug.Log($"Call Equal: {obj} vs {this}");
            return obj is SaintsInterface<TObject, TInterface> other && ReferenceEquals(other.V, V);
        }

        public static bool operator ==(SaintsInterface<TObject, TInterface> a, SaintsInterface<TObject, TInterface> b)
        {
            // Debug.Log($"Call ==: {a} vs {b}");
            // ReSharper disable once Unity.NoNullPropagation
            return a?.V
                   // ReSharper disable once Unity.NoNullPropagation
                   == b?.V;
        }

        public static bool operator !=(SaintsInterface<TObject, TInterface> a, SaintsInterface<TObject, TInterface> b) =>
            // ReSharper disable once Unity.NoNullPropagation
            a?.V
            // ReSharper disable once Unity.NoNullPropagation
            != b?.V;

        // ReSharper disable once NonReadonlyMemberInGetHashCode
        // ReSharper disable once Unity.NoNullPropagation
        public override int GetHashCode() => V?.GetHashCode() ?? 0;
    }
}
