using System;
using UnityEngine;

// ReSharper disable once CheckNamespace
namespace SaintsField
{
    [Serializable]
    public class SaintsInterface<TObject, TInterface>: IWrapProp where TObject: UnityEngine.Object where TInterface: class
    {
        [field: SerializeField] public TObject V { get; private set; }
        [field: SerializeReference] public object VRef { get; private set; }

        [field: SerializeField] public bool IsVRef { get; private set; }

        public virtual TInterface I => IsVRef? VRef as TInterface: V as TInterface;

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

            if (a?.IsVRef != b?.IsVRef)
            {
                return false;
            }

            if (a?.IsVRef ?? false)
            {
                return a.VRef == b.VRef;
            }

            // ReSharper disable once Unity.NoNullPropagation
            return a?.V
                   // ReSharper disable once Unity.NoNullPropagation
                   == b?.V;
        }

        public static bool operator !=(SaintsInterface<TObject, TInterface> a, SaintsInterface<TObject, TInterface> b)
        {
            if (a?.IsVRef != b?.IsVRef)
            {
                return true;
            }

            if (a?.IsVRef ?? false)
            {
                return a.VRef != b.VRef;
            }

            // ReSharper disable once Unity.NoNullPropagation
            return a?.V
                   // ReSharper disable once Unity.NoNullPropagation
                   != b?.V;
        }

        // ReSharper disable once NonReadonlyMemberInGetHashCode
        // ReSharper disable once Unity.NoNullPropagation
        public override int GetHashCode()
        {
            if (IsVRef)
            {
                // ReSharper disable once NonReadonlyMemberInGetHashCode
                return VRef?.GetHashCode() ?? 0;
            }
            // ReSharper disable once NonReadonlyMemberInGetHashCode
            return V?.GetHashCode() ?? 0;
        }
    }

    [Serializable]
    public class SaintsInterface<TInterface>: SaintsInterface<UnityEngine.Object, TInterface> where TInterface: class
    {
        public SaintsInterface(UnityEngine.Object obj) : base(obj)
        {
        }
    }
}
