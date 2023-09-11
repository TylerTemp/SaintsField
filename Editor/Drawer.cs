using UnityEditor;

namespace ExtInspector.Editor
{
    public static class Drawer
    {
        public static void PropertyField(SerializedProperty property)
        {
            string displayName = property.displayName;
        }
    }
}
