
using System.Linq;
#if UNITY_EDITOR
using System.Collections.Generic;
using SaintsField.Utils;
using UnityEditor;
#endif

namespace SaintsField
{
    public static class SaintsContext
    {
#if UNITY_EDITOR
        public static SerializedProperty SerializedProperty;

        public static SerializedProperty FindPropertyRelateTo(string propertyName)
        {
            string curPath = SerializedProperty.propertyPath;

            foreach (string eachPart in propertyName.Split('/'))
            {
                int lastDot = curPath.LastIndexOf('.');
                if (eachPart == "..")
                {
                    string upPath = curPath[..lastDot];
                    if (upPath.EndsWith(']'))  // .Array.data[Number]
                    {
                        upPath = string.Join('.', upPath.Split('.').SkipLast(2));
                    }

                    curPath = upPath;
                }
                else
                {
                    curPath = curPath[..(lastDot + 1)] + eachPart;
                }
            }

            return SerializedProperty.serializedObject.FindProperty(curPath);

            // int lastDot = curPath.LastIndexOf('.');
            //
            // SerializedProperty result = SerializedProperty.serializedObject.FindProperty(MergePath(curPath, lastDot, propertyName));
            // // ReSharper disable once ConvertIfStatementToNullCoalescingExpression
            // if (result == null)
            // {
            //     result = SerializedProperty.serializedObject.FindProperty(MergePath(curPath, lastDot, RuntimeUtil.GetAutoPropertyName(propertyName)));
            // }
            //
            // return result;
        }

        private static string MergePath(string curPath, int lastDot, string propertyName)
        {
            if (lastDot == -1)
            {
                return propertyName;
            }

            if (propertyName == "..")
            {
                return curPath[..lastDot];
            }

            string parentPath = curPath[..(lastDot + 1)];
            return parentPath + propertyName;

        }
#endif
    }
}
