// using UnityEditor;
// using UnityEngine;
//
// namespace SaintsField.Editor
// {
//     [CustomPropertyDrawer(typeof(TestIMGUIAttribute))]
//     public class TestIMGUIAttributeDrawer: PropertyDrawer
//     {
//         public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
//         {
//             return EditorGUIUtility.singleLineHeight;
//         }
//
//         private bool _testToggle;
//
//         public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
//         {
//             _testToggle = EditorGUI.Toggle(position, label, _testToggle);
//         }
//     }
// }
