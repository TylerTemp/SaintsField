using System;
using System.Linq;
using SaintsField.Editor.Core;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace SaintsField.Editor.Drawers
{
    [CustomPropertyDrawer(typeof(ReferencePickerAttribute))]
    public class ReferencePickerAttributeDrawer: SaintsPropertyDrawer
    {
        #region UI Toolkit
        // private static string NamePropertyContainer(SerializedProperty property) => $"{property.propertyPath}__Reference_PropertyField_Container";
        // private static string NamePropertyField(SerializedProperty property) => $"{property.propertyPath}__Reference_PropertyField";
        private static string NameButton(SerializedProperty property) => $"{property.propertyPath}__Reference_Button";

        protected override VisualElement CreatePostFieldUIToolkit(SerializedProperty property,
            ISaintsAttribute saintsAttribute, int index, VisualElement container, object parent)
        {
            return new Button
            {
                name = NameButton(property),
                text = "▼",
                style =
                {
                    minHeight = SingleLineHeight,
                    maxHeight = SingleLineHeight,
                    borderLeftWidth = 0,
                    borderRightWidth = 0,
                    borderTopWidth = 0,
                    borderBottomWidth = 0,
                    paddingLeft = 0,
                    paddingRight = 0,
                    paddingTop = 0,
                    paddingBottom = 0,
                    borderTopLeftRadius = 0,
                    borderTopRightRadius = 0,
                    borderBottomLeftRadius = 0,
                    borderBottomRightRadius = 0,
                    backgroundColor = Color.clear,
                },
            };
        }

        protected override void OnAwakeUIToolkit(SerializedProperty property, ISaintsAttribute saintsAttribute, int index, VisualElement container,
            Action<object> onValueChangedCallback, object parent)
        {
            Button button = container.Q<Button>(NameButton(property));
            // VisualElement propertyContainer = container.Q<VisualElement>(NamePropertyContainer(property));
            button.clicked += () =>
            {
                string typename = property.managedReferenceFieldTypename;
                string[] typeSplitString = typename.Split(' ');
                string typeClassName = typeSplitString[1];
                string typeAssemblyName = typeSplitString[0];
                Type realType = Type.GetType($"{typeClassName}, {typeAssemblyName}");
                // Debug.Log(realType);

                // GenericMenu menu = new GenericMenu();
                object managedReferenceValue = property.managedReferenceValue;
                GenericDropdownMenu genericDropdownMenu = new GenericDropdownMenu();
                genericDropdownMenu.AddItem("[Null]", managedReferenceValue == null, () =>
                {
                    PropSetValue(container, property, null);
                    onValueChangedCallback(null);
                });
                genericDropdownMenu.AddSeparator("");

                foreach (Type type in TypeCache.GetTypesDerivedFrom(realType)
                             .Where(each => !each.IsSubclassOf(typeof(UnityEngine.Object)))
                             .Where(each => !each.IsAbstract)  // abstract classes
                             .Where(each => !each.ContainsGenericParameters)  // generic classes
                             .Where(each => !each.IsClass || each.GetConstructor(Type.EmptyTypes) != null)  // no public empty constructors
                        )
                {
                    // string assemblyName =  type.Assembly.ToString().Split('(', ',')[0];
                    string displayName = $"{type.Name}: {type.Namespace}";

                    genericDropdownMenu.AddItem(displayName, managedReferenceValue != null && managedReferenceValue.GetType() == type, () =>
                    {
                        object instance = Activator.CreateInstance(type);
                        PropSetValue(container, property, instance);

                        onValueChangedCallback(instance);
                    });
                }

                Rect fakePos = container.worldBound;
                fakePos.height = SingleLineHeight;

                genericDropdownMenu.DropDown(fakePos, container, true);
            };
        }

        private static void PropSetValue(VisualElement container, SerializedProperty property, object newValue)
        {
            property.serializedObject.Update();
            property.managedReferenceValue = null;
            property.managedReferenceValue = newValue;
            // property.managedReferenceId = -2L;
            property.serializedObject.ApplyModifiedProperties();
            // property.serializedObject.SetIsDifferentCacheDirty();

            container.Query<PropertyField>(className: SaintsFieldFallbackClass).ForEach(each => each.BindProperty(property));

            // PropertyField propertyField = container.Q<PropertyField>();
            // propertyField.Unbind();
            // propertyField.BindProperty(property);
            // property.serializedObject.Update();
        }
        #endregion
    }
}
