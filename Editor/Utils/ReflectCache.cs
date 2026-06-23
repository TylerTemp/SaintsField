using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using SaintsField.Interfaces;
using UnityEngine;

namespace SaintsField.Editor.Utils
{
    public static class ReflectCache
    {
        private static readonly Dictionary<AttributesKey, Attribute[]> CustomAttributes = new Dictionary<AttributesKey, Attribute[]>();

        private readonly struct AttributesKey : IEquatable<AttributesKey>
        {
            private readonly MemberInfo _memberInfo;
            private readonly Type _classType;
            private readonly bool _inherit;
            private readonly Type _type;

            public AttributesKey(MemberInfo memberInfo, bool inherit, Type type = null)
            {
                _memberInfo = memberInfo;
                _classType = null;
                _inherit = inherit;
                _type = type;
            }

            public AttributesKey(Type classType, bool inherit, Type type = null)
            {
                _memberInfo = null;
                _classType = classType;
                _inherit = inherit;
                _type = type;
            }

            public bool Equals(AttributesKey other) =>
                Equals(_memberInfo, other._memberInfo) && _classType == other._classType && _inherit == other._inherit && _type == other._type;

            public override bool Equals(object obj) => obj is AttributesKey other && Equals(other);

            public override int GetHashCode() => Util.CombineHashCode(_memberInfo, _classType, _inherit, _type);

            public override string ToString()
            {
                return $"<Attribute {(_classType != null? "class=" + _classType: $"member={_memberInfo.Name}-{_memberInfo.MemberType}")} type={_type} inherit={_inherit}/>";
            }
        }

        public static Attribute[] GetCustomAttributes(MemberInfo memberInfo, bool inherit = false)
        {
            AttributesKey key = new AttributesKey(memberInfo, inherit);
            if (CustomAttributes.TryGetValue(key, out Attribute[] attributes))
            {
                // Debug.Log($"cached fetch for {key} = {string.Join<Attribute>(", ", attributes)}");
                return attributes;
            }

            // Debug.Log($"refresh fetch for {key}");
            attributes = memberInfo.GetCustomAttributes().ToArray();
            attributes = EnsureSaintsAttributeFirst(attributes);
            CustomAttributes[key] = attributes;
            return attributes;
        }

        public static T[] GetCustomAttributes<T>(MemberInfo memberInfo, bool inherit = false)
        {
            AttributesKey key = new AttributesKey(memberInfo, inherit, typeof(T));
            if (CustomAttributes.TryGetValue(key, out Attribute[] attributes))
            {
                // return (T[])attributes;
                // Debug.Log($"cached fetch for {key} = {string.Join<Attribute>(", ", attributes)}");
                return attributes.OfType<T>().ToArray();
            }

            // Debug.Log($"refresh fetch for {key}");
            attributes = memberInfo.GetCustomAttributes().ToArray();
            attributes = EnsureSaintsAttributeFirst(attributes);
            CustomAttributes[key] = attributes;
            return attributes.OfType<T>().ToArray();
        }

        public static T[] GetTypeCustomAttributes<T>(Type attributeType, bool inherit)
        {
            AttributesKey key = new AttributesKey(attributeType, inherit, typeof(T));
            if (CustomAttributes.TryGetValue(key, out Attribute[] attributes))
            {
                // return (T[])attributes;
                // Debug.Log($"cached fetch for {key} = {string.Join<Attribute>(", ", attributes)}");
                return attributes.OfType<T>().ToArray();
            }

            // Debug.Log($"refresh fetch for {key}");
            attributes = attributeType.GetCustomAttributes(inherit: inherit).Cast<Attribute>().ToArray();
            attributes = EnsureSaintsAttributeFirst(attributes);
            CustomAttributes[key] = attributes;
            return attributes.OfType<T>().ToArray();
        }

        private static Attribute[] EnsureSaintsAttributeFirst(Attribute[] attributes)
        {
            if (attributes.Length == 0 || attributes[0] is ISaintsAttribute)
            {
                return attributes;
            }

            int saintsIndex = Array.FindIndex(attributes, each => each is PropertyAttribute and ISaintsAttribute);
            if (saintsIndex <= 0)
            {
                return attributes;
            }

            Attribute saintsAttribute = attributes[saintsIndex];
            Array.Copy(attributes, 0, attributes, 1, saintsIndex);
            attributes[0] = saintsAttribute;
            return attributes;
        }

        // public static void ReplaceCustomAttributes(MemberInfo memberInfo, Attribute[] attributes, bool inherit = false)
        // {
        //     AttributesKey key = new AttributesKey(memberInfo, inherit);
        //     Debug.Log($"replace cache for {key}");
        //     CustomAttributes[key] = attributes;
        // }
    }
}
