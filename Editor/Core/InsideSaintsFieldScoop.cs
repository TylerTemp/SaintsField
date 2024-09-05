using System;
using System.Collections.Generic;
using SaintsField.Editor.Utils;
using UnityEditor;
using UnityEngine;

namespace SaintsField.Editor.Core
{
    public class InsideSaintsFieldScoop: IDisposable
    {
        public struct PropertyKey : IEquatable<PropertyKey>
        {
            public int ObjectHash;
            public string PropertyPath;

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

        public static PropertyKey MakeKey(SerializedProperty property) => new PropertyKey
        {
            ObjectHash = property.serializedObject.targetObject.GetInstanceID(),
            PropertyPath = property.propertyPath,
        };

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
