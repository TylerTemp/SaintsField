using System;

namespace SaintsField.Editor.Drawers.SaintsEventBaseTypeDrawer
{
    public static class SaintsEventUtils
    {
        public static string StringifyType(Type type)
        {
            if (type == typeof(string))
            {
                return "string";
            }

            if (type == typeof(int))
            {
                return "int";
            }

            if (type == typeof(long))
            {
                return "long";
            }

            if (type == typeof(float))
            {
                return "float";
            }

            if (type == typeof(double))
            {
                return "double";
            }

            if (type == typeof(bool))
            {
                return "bool";
            }

            if (type == typeof(object))
            {
                return "object";
            }

            string s = type.ToString();
            return s.StartsWith("UnityEngine.")
                ? s.Substring("UnityEngine.".Length)
                : s;
        }
    }
}
