using System;
using UnityEditor;
using UnityEngine;

namespace SaintsField.Editor.Core
{
    public class InsideSaintsFieldScoop: IDisposable
    {
        public struct PropertyKey
        {
            public int ObjectHash;
            public string PropertyPath;

            public override string ToString()
            {
                return $"{ObjectHash}.{PropertyPath}";
            }
        }

        private readonly PropertyKey _property;

        public static PropertyKey MakeKey(SerializedProperty property) => new PropertyKey
        {
            ObjectHash = property.serializedObject.targetObject.GetInstanceID(),
            PropertyPath = property.propertyPath,
        };

        public InsideSaintsFieldScoop(PropertyKey key)
        {
            // SaintsPropertyDrawer.IsSubDrawer = true;
            if (!SaintsPropertyDrawer.SubCounter.TryGetValue(key, out int count))
            {
                count = 0;
            }

            // Debug.Log($"subCount {key} {count}+1");
            SaintsPropertyDrawer.SubCounter[key] = count + 1;

            _property = key;
        }

        public void Dispose()
        {
            // SaintsPropertyDrawer.IsSubDrawer = false;
            if (SaintsPropertyDrawer.SubCounter.TryGetValue(_property, out int count))
            {
                // Debug.Log($"subCount {_property} {count}-1");
                SaintsPropertyDrawer.SubCounter[_property] = count - 1;
            }
        }
    }
}
