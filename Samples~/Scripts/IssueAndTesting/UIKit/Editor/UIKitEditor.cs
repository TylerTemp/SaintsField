#if SAINTSFIELD_SAMPLE_ENABLE_UIKIT_EDITOR
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

// this gives: StackOverflowException: The requested operation caused a stack overflow.
// no idea why
// see: https://forum.unity.com/threads/property-drawers.595369/

namespace SaintsField.Samples.Scripts.UIKit.Editor
{
    [CustomEditor(typeof(Object), true, isFallback = true)]
    public class UIKitEditor : UnityEditor.Editor
    {
        public override VisualElement CreateInspectorGUI()
        {
            VisualElement container = new VisualElement();

            SerializedProperty iterator = serializedObject.GetIterator();
            if (iterator.NextVisible(true))
            {
                do
                {
                    PropertyField propertyField = new PropertyField(iterator.Copy()) { name = "PropertyField:" + iterator.propertyPath };

                    if (iterator.propertyPath == "m_Script" && serializedObject.targetObject != null)
                        propertyField.SetEnabled(value: false);

                    container.Add(propertyField);
                }
                while (iterator.NextVisible(false));
            }

            return container;
        }
    }
}
#endif
