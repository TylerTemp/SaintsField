using System.Linq;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace SaintsField.Samples.Scripts.UIKit.Editor
{
    [CustomPropertyDrawer(typeof(UIKitPropFixAttribute))]
    public class UIKitPropFixAttributeDrawer: PropertyDrawer
    {
        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
// #if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_DRAW_PROCESS_CORE
//         Debug.Log($"Create property gui {property.serializedObject.targetObject} {property.propertyPath}/{this}");
// #endif

            // // Create property container element.
            // VisualElement container = new VisualElement();
            //
            // // Create property fields.
            // PropertyField amountField = new PropertyField(property);
            //
            // // Add fields to the container.
            // container.Add(amountField);
            //
            // return container;

            PropertyField propertyField = new PropertyField(property);

            Debug.Log(property.propertyPath);

            propertyField.schedule.Execute(() => FixUnityShit(property, propertyField)).StartingIn(200);

            return propertyField;
            // return new TextField(property.displayName);
        }

        private void FixUnityShit(SerializedProperty property, VisualElement root)
        {
            root.ClearClassList();
            foreach (VisualElement child in root.Children())
            {
                child.ClearClassList();
            }

            // root.style.flexDirection = FlexDirection.Row;
            // CleanClasses(root);

            // Debug.Log(propertyField.Children().Count());
            // root.AddToClassList("unity-base-field");
            // root.style.marginLeft = 0;
            // root.style.marginRight = 0;
            bool isNested = property.propertyPath.Contains(".");
            if (root.childCount == 0)
            {
                return;
            }

            VisualElement firstChild = root.Children().First();
            // firstChild.ClearClassList();
            firstChild.style.flexDirection = FlexDirection.Row;
            // firstChild.AddToClassList("unity-base-field");

            root.parent.Add(firstChild);
            root.RemoveFromHierarchy();

            // firstChild.style.flexGrow = 1;
            // firstChild.AddToClassList("unity-base-field");
            // firstChild.AddToClassList("unity-base-text-field");
            // firstChild.AddToClassList("unity-text-field");
            // firstChild.AddToClassList("unity-base-field__inspector-field");
            // firstChild.style.marginLeft = 3;
            // firstChild.style.marginRight = -2;
            // root.style.flexGrow = 1;
            Label label = root.Q<Label>();
            if (label == null)
            {
                return;
            }

            label.ClearClassList();
            label.style.borderLeftWidth = label.style.borderRightWidth = 1;
            label.style.borderLeftColor = label.style.borderRightColor = Color.blue;
            label.style.width = StyleKeyword.Null;
            // Debug.Log(label.text);
            // label.AddToClassList("unity-text-element");
            // label.AddToClassList("unity-label");
            // label.AddToClassList("unity-base-field__label");
            // label.AddToClassList("unity-base-text-field__label");
            // label.AddToClassList("unity-text-field__label");

            // label.AddToClassList("unity-base-field__label");
            // label.style.width = StyleKeyword.Null;
            // label.style.width = 1;

            VisualElement input = label.parent.Q<VisualElement>(className: "unity-base-text-field__input");
            input.ClearClassList();
            input.style.borderLeftWidth = input.style.borderRightWidth = 1;
            input.style.borderLeftColor = input.style.borderRightColor = Color.red;
            input.style.flexGrow = 1;
            // input.style.flexGrow = 1;
            // input.AddToClassList("unity-base-text-field__input");
            // input.AddToClassList("unity-base-text-field__input--single-line");
            // input.AddToClassList("unity-base-field__input");
            // input.AddToClassList("unity-text-field__input");
            // input.style.marginRight
        }

        // private void CleanClasses(VisualElement visualElement)
        // {
        //     if (visualElement == null)
        //     {
        //         return;
        //     }
        //
        //     visualElement.ClearClassList();
        //     foreach (VisualElement child in visualElement.Children())
        //     {
        //         CleanClasses(child);
        //     }
        // }
    }
}
