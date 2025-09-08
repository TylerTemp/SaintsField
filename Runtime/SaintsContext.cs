#if UNITY_EDITOR
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
            int lastDot = curPath.LastIndexOf('.');

            SerializedProperty result = SerializedProperty.serializedObject.FindProperty(MergePath(curPath, lastDot, propertyName));
            // ReSharper disable once ConvertIfStatementToNullCoalescingExpression
            if (result == null)
            {
                result = SerializedProperty.serializedObject.FindProperty(MergePath(curPath, lastDot, RuntimeUtil.GetAutoPropertyName(propertyName)));
            }

            return result;
        }

        private static string MergePath(string curPath, int lastDot, string propertyName)
        {
            if (lastDot == -1)
            {
                return propertyName;
            }

            // ReSharper disable once ReplaceSubstringWithRangeIndexer
            string parentPath = curPath.Substring(0, lastDot + 1);
            return parentPath + propertyName;

        }
#endif
    }
}
