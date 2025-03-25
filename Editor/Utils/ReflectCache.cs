using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace SaintsField.Editor.Utils
{
    public static class ReflectCache
    {
        private static readonly Dictionary<AttributesKey, object[]> CustomAttributes = new Dictionary<AttributesKey, object[]>();

        private readonly struct AttributesKey : IEquatable<AttributesKey>
        {
            readonly MemberInfo memberInfo;
            readonly bool inherit;
            readonly Type type;

            public AttributesKey(MemberInfo memberInfo, bool inherit, Type type = null)
            {
                this.memberInfo = memberInfo;
                this.inherit = inherit;
                this.type = type;
            }

            public bool Equals(AttributesKey other) =>
                Equals(memberInfo, other.memberInfo) && inherit == other.inherit && type == other.type;

            public override bool Equals(object obj) => obj is AttributesKey other && Equals(other);

            public override int GetHashCode() => Util.CombineHashCode(memberInfo, inherit, type);
        }

        public static Attribute[] GetCustomAttributes(MemberInfo memberInfo, bool inherit = false)
        {
            AttributesKey key = new AttributesKey(memberInfo, inherit);
            if (CustomAttributes.TryGetValue(key, out object[] attributes))
            {
                return (Attribute[])attributes;
            }

            // ReSharper disable once CoVariantArrayConversion
            attributes = memberInfo.GetCustomAttributes().ToArray();
            CustomAttributes[key] = attributes;
            return (Attribute[])attributes;
        }

        public static T[] GetCustomAttributes<T>(MemberInfo memberInfo, bool inherit = false) where T : class
        {
            AttributesKey key = new AttributesKey(memberInfo, inherit, typeof(T));
            if (CustomAttributes.TryGetValue(key, out object[] attributes))
            {
                return (T[])attributes;
            }

            // ReSharper disable once CoVariantArrayConversion
            attributes = memberInfo.GetCustomAttributes().OfType<T>().ToArray();
            CustomAttributes[key] = attributes;
            return (T[])attributes;
        }
    }
}
