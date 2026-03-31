using System;
using System.Collections.Generic;
using SaintsField.Editor.Utils;
using UnityEditor;
using UnityEngine;

namespace SaintsField.Editor.Core
{
    public readonly struct InsideSaintsFieldScoop: IDisposable
    {
        public readonly struct PropertyKey : IEquatable<PropertyKey>
        {
#if UNITY_6000_5_OR_NEWER
            public readonly ulong ObjectHash;
#else
            public readonly int ObjectHash;
#endif
            public readonly string PropertyPath;

#if UNITY_6000_5_OR_NEWER
            public PropertyKey(EntityId entityId, string propertyPath)
#else
            public PropertyKey(int objectHash, string propertyPath)
#endif
            {
#if UNITY_6000_5_OR_NEWER
                ObjectHash = EntityId.ToULong(entityId);
#else
                ObjectHash = objectHash;
#endif
                PropertyPath = propertyPath;
            }

            public override string ToString()
            {
                return $"{ObjectHash}.{PropertyPath}";
            }

            public bool Equals(PropertyKey other)
            {
                return ObjectHash == other.ObjectHash && PropertyPath == other.PropertyPath;
            }

            public override bool Equals(object obj)
            {
                return obj is PropertyKey other && Equals(other);
            }

            public override int GetHashCode()
            {
                return Util.CombineHashCode(ObjectHash, PropertyPath);
            }
        }

        private readonly PropertyKey _property;

        public static PropertyKey MakeKey(SerializedProperty property) => new PropertyKey(
#if UNITY_6000_5_OR_NEWER
            property.serializedObject.targetObject.GetEntityId(),
#else
            property.serializedObject.targetObject.GetInstanceID(),
#endif
            property.propertyPath
        );

        private readonly Dictionary<InsideSaintsFieldScoop.PropertyKey, int> Counter;

        public InsideSaintsFieldScoop(Dictionary<InsideSaintsFieldScoop.PropertyKey, int> counter, PropertyKey key)
        {
            Counter = counter;
            _property = key;

            if (!Counter.TryGetValue(key, out int count))
            {
                count = 0;
            }

            // Debug.Log($"subCount {key} {count}+1");
            Counter[key] = count + 1;
        }

        public void Dispose()
        {
            // SaintsPropertyDrawer.IsSubDrawer = false;
            if (Counter.TryGetValue(_property, out int count))
            {
                // Debug.Log($"subCount {_property} {count}-1");
                Counter[_property] = count - 1;
            }
        }
    }
}
