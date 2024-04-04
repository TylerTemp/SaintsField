#if UNITY_EDITOR

using UnityEditor;
using UnityEngine;
#if UNITY_2022_2_OR_NEWER
using UnityEditor.UIElements;
using UnityEngine.UIElements;
#endif

namespace SaintsField.Samples.Scripts.IssueAndTesting.UIKit.Editor
{
    [CustomPropertyDrawer(typeof(IntFieldAttribute))]
    public class IntFieldAttributeDrawer : PropertyDrawer
    {
#if UNITY_2022_2_OR_NEWER
        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            // PropertyField propertyField = new PropertyField(property);
            // return propertyField;
            return new IntegerField(property.displayName);
        }
#endif
    }
}

#endif
