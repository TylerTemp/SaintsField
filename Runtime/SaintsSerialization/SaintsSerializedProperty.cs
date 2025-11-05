using System;
using UnityEngine;

// ReSharper disable once CheckNamespace
namespace SaintsField.SaintsSerialization
{
    [Serializable]
    public struct SaintsSerializedProperty: IWrapProp
    {
        public SaintsPropertyType propertyType;
        public string propertyPath;

        public long longValue;
        public ulong uLongValue;
        public string stringValue;

        #region General Serialization
        // ReSharper disable once InconsistentNaming
        public UnityEngine.Object V;
        // ReSharper disable once InconsistentNaming
        [SerializeReference] public object VRef;
        // ReSharper disable once InconsistentNaming
        public bool IsVRef;
        #endregion

#if UNITY_EDITOR
        // ReSharper disable once StaticMemberInGenericType
        public static readonly string EditorPropertyName = nameof(V);
#endif
    }
}
