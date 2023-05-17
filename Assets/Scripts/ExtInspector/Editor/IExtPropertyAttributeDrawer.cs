using UnityEditor;
using UnityEngine;

namespace ExtInspector.Editor
{
    public interface IExtPropertyAttributeDrawer
    {
        public float GetPropertyHeight(SerializedProperty property, GUIContent label, IPostDecorator postDecorator);

        public void OnGUI(Rect position, SerializedProperty property, GUIContent label, IPostDecorator postDecorator);
    }
}
