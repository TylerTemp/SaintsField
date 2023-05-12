using UnityEditor;
using UnityEngine;
using Color = UnityEngine.Color;

namespace ExtInspector.Samples.Editor
{
    [CustomPropertyDrawer(typeof(IconAttribute))]
    public class IconAttributeDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            Texture2D texture = (Texture2D)EditorGUIUtility.Load("ExtInspector/magnifying-glass-solid.png");
            Texture2D coloredT = Standalone.Icon.ApplyTextureColor(texture, Color.magenta);
            Standalone.Icon.ResizeTexture(coloredT, 12, 12);
            label.image = coloredT;
            EditorGUI.PropertyField(position, property, label);
        }
    }
}
