#if UNITY_EDITOR
using UnityEditor;
#endif

namespace SaintsField
{
    public static class SaintsContext
    {
#if UNITY_EDITOR
        public static SerializedProperty SerializedProperty;
#endif
    }
}
