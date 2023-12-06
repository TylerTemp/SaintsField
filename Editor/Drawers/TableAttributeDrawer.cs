using UnityEditor;
using UnityEngine;

namespace SaintsField.Editor.Drawers
{
    [CustomPropertyDrawer(typeof(TableAttribute))]
    public class TableAttributeDrawer: PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            Debug.Log("called!");
            // base.OnGUI(position, property, label);
            GUI.Label(position, "TableAttribute");
        }
    }
}
