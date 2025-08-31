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

        public SaintsInterface(TObject obj)
        {
            V = obj;
        }

#if UNITY_EDITOR
        // ReSharper disable once StaticMemberInGenericType
        public static readonly string EditorPropertyName = nameof(V);
        public virtual bool EditorCustomPicker => true;

        /// <summary>
        /// Set the actual value, must have implemented the `TInterface`
        /// </summary>
        /// <param name="obj">The value to sign</param>
        public void EditorSetValue(TObject obj)
        {
            V = obj;
        }
#endif

        // public static implicit operator TInterface(SaintsInterface<TObject, TInterface> saintsInterface) => saintsInterface.I;
        // public static implicit operator TObject(SaintsInterface<TObject, TInterface> saintsInterface) => saintsInterface.V;

        public static explicit operator TInterface(SaintsInterface<TObject, TInterface> saintsInterface) => saintsInterface.I;
        public static explicit operator TObject(SaintsInterface<TObject, TInterface> saintsInterface) => saintsInterface.V;

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
