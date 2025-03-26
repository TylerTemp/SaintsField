using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace SaintsField.Samples.Scripts.IssueAndTesting.Issue.Issue182
{

    public class Comment2Attribute : BelowRichLabelAttribute
    {
        public Comment2Attribute(string comment) : base($"  <color=gray><size=10%>{comment}</size></color>") { }
    }

    public class CommentAttribute:PropertyAttribute
    {
        public readonly string comment;

        public CommentAttribute(string comment)
        {
            this.comment = comment;
        }
    }
#if UNITY_EDITOR
    [CustomPropertyDrawer(typeof(CommentAttribute))]
    public class CommentPropertyDrawer:PropertyDrawer
    {
        CommentAttribute commentAttribute => (CommentAttribute) attribute;

        public override float GetPropertyHeight(SerializedProperty prop,GUIContent label)
        {
            float baseHeight    = EditorGUI.GetPropertyHeight(prop,label,true);
            float commentHeight = EditorStyles.wordWrappedMiniLabel.CalcHeight(new GUIContent(commentAttribute.comment),Screen.width-20)-3;
            if(prop.propertyPath.Contains("Array.data[")) {
                // Array element
                if(IsLastArrayElement(prop)) { return baseHeight+commentHeight; }
                return baseHeight;
            }
            // Regular property or array header
            return baseHeight+commentHeight;
        }

        public override void OnGUI(Rect position,SerializedProperty prop,GUIContent label)
        {
            EditorGUI.PropertyField(position,prop,label,true);
            bool isArrayElement    = prop.propertyPath.Contains("Array.data[");
            bool shouldDrawComment = false;
            if(isArrayElement) {
                // For array elements
                shouldDrawComment = IsLastArrayElement(prop);
            } else
            if(prop.isArray) {
                // For array header
                shouldDrawComment = !prop.isExpanded;
            } else {
                // For regular properties
                shouldDrawComment = true;
            }
            if(shouldDrawComment) {
                float baseHeight;
                if(prop.isArray && !prop.isExpanded) { baseHeight = EditorGUIUtility.singleLineHeight; } else { baseHeight = EditorGUI.GetPropertyHeight(prop,label,true); }
                Rect commentPosition = position;
                commentPosition.y += baseHeight;
                commentPosition = EditorGUI.IndentedRect(commentPosition);
                commentPosition.x += 10;
                commentPosition.y -= 4;
                commentPosition.width -= 20;
                Color originalColor = GUI.contentColor;
                Color fadedColor    = originalColor;
                fadedColor.a = 0.5f;
                GUI.contentColor = fadedColor;
                GUI.Label(commentPosition,commentAttribute.comment,EditorStyles.wordWrappedMiniLabel);
                GUI.contentColor = originalColor;
            }
        }

        bool IsLastArrayElement(SerializedProperty property)
        {
            string propertyPath = property.propertyPath;
            if(!propertyPath.Contains("Array.data[")) return false;
            // Extract array path and current index
            int    arrayIndex   = propertyPath.IndexOf("Array.data[");
            string arrayPath    = propertyPath.Substring(0,arrayIndex+5);
            int    startIndex   = propertyPath.IndexOf('[')+1;
            int    endIndex     = propertyPath.IndexOf(']');
            int    currentIndex = int.Parse(propertyPath.Substring(startIndex,endIndex-startIndex));
            // Get array property and check if this is the last element
            SerializedProperty arrayProp = property.serializedObject.FindProperty(arrayPath);
            return currentIndex==arrayProp.arraySize-1;
        }
    }
#endif
}
