#if UNITY_EDITOR
using SaintsField.Editor.Utils;
using UnityEditor;

#if UNITY_2022_2_OR_NEWER
using UnityEditor.UIElements;
using UnityEngine.UIElements;
#endif

namespace SaintsField.Samples.Scripts.IssueAndTesting.UIKit.Editor
{
    [CustomPropertyDrawer(typeof(FixLabelAttribute))]
    public class FixLabelAttributeDrawer : PropertyDrawer
    {
#if UNITY_2022_2_OR_NEWER
        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            PropertyField propertyField = new PropertyField(property);

            propertyField.schedule.Execute(() =>
            {
                Label label = propertyField.Q<Label>(className: "unity-label");
                if (label != null)
                {
                    UIToolkitUtils.FixLabelWidthLoopUIToolkit(label);
                }
            });

            return propertyField;
        }
#endif
    }
}
#endif
