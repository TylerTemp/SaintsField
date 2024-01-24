using System;
using System.Linq;
using System.Reflection;
using SaintsField.Editor.Core;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace SaintsField.Editor.Drawers
{
    [CustomPropertyDrawer(typeof(ReferenceAttribute))]
    public class ReferenceAttributeDrawer: SaintsPropertyDrawer
    {
        private static string NamePropertyContainer(SerializedProperty property) => $"{property.propertyPath}__Reference_PropertyField_Container";
        private static string NamePropertyField(SerializedProperty property) => $"{property.propertyPath}__Reference_PropertyField";
        private static string NameButton(SerializedProperty property) => $"{property.propertyPath}__Reference_Button";

        protected override VisualElement CreateFieldUIToolKit(SerializedProperty property,
            ISaintsAttribute saintsAttribute, VisualElement container, Label fakeLabel, object parent)
        {
            VisualElement root = new VisualElement();

            VisualElement propertyContainer = new VisualElement
            {
                name = NamePropertyContainer(property),
            };

            PropertyField propertyField = new PropertyField(property, new string(' ', property.propertyPath.Length))
            {
                name = NamePropertyField(property),
            };
            propertyContainer.Add(propertyField);
            root.Add(propertyContainer);

            Button button = new Button
            {
                name = NameButton(property),
                text = "●",
                style =
                {
                    minHeight = SingleLineHeight,
                    maxHeight = SingleLineHeight,
                },
            };
            root.Add(button);

            return root;
        }

        protected override void OnAwakeUIToolkit(SerializedProperty property, ISaintsAttribute saintsAttribute, int index, VisualElement container,
            Action<object> onValueChangedCallback, object parent)
        {
            Button button = container.Q<Button>(NameButton(property));
            VisualElement propertyContainer = container.Q<VisualElement>(NamePropertyContainer(property));
            button.RegisterCallback<MouseUpEvent>(evt =>
            {
                string typename = property.managedReferenceFieldTypename;
                string[] typeSplitString = typename.Split(' ');
                string typeClassName = typeSplitString[1];
                string typeAssemblyName = typeSplitString[0];
                Type realType = Type.GetType($"{typeClassName}, {typeAssemblyName}");
                // Debug.Log(realType);

                GenericMenu menu = new GenericMenu();
                menu.AddItem(new GUIContent("[Null]"), false, () =>
                {
                    property.serializedObject.Update();
                    property.managedReferenceValue = null;
                    // property.managedReferenceId = -2L;
                    property.serializedObject.ApplyModifiedProperties();

                    onValueChangedCallback(null);
                });

                foreach (Type type in TypeCache.GetTypesDerivedFrom(realType)
                             .Where(each => !each.IsSubclassOf(typeof(UnityEngine.Object)))
                             .Where(each => !each.IsAbstract)  // abstract classes
                             .Where(each => !each.ContainsGenericParameters)  // generic classes
                             .Where(each => !each.IsClass || each.GetConstructor(Type.EmptyTypes) != null)  // no public empty constructors
                        )
                {
                    menu.AddItem(new GUIContent(type.ToString()), false, () =>
                    {
                        bool oldValueIsNull = property.managedReferenceValue == null;
                        Debug.Log($"oldValueIsNull={oldValueIsNull}");

                        object instance = Activator.CreateInstance(type);
                        property.managedReferenceValue = instance;
                        property.serializedObject.Update();
                        property.serializedObject.ApplyModifiedProperties();

                        onValueChangedCallback(instance);

                        if (oldValueIsNull)
                        {
                            BindingFlags BindAttr = BindingFlags.Instance | BindingFlags.Static | BindingFlags.NonPublic |
                                                    BindingFlags.Public | BindingFlags.DeclaredOnly;

                            Type parentType = parent.GetType();
                            FieldInfo thisField = parentType.GetField(property.name, BindAttr);
                            thisField.SetValue(parent, instance);

                            container.Q<PropertyField>(NamePropertyField(property)).RemoveFromHierarchy();

                            propertyContainer.Clear();

                            PropertyField newProp =
                                new PropertyField(property, new string(' ', property.propertyPath.Length))
                                {
                                    name = NamePropertyField(property),
                                };
                            newProp.BindProperty(property);
                            propertyContainer.Add(newProp);
                            // UnityEditorInternal.InternalEditorUtility.RepaintAllViews();
                        }
                    });
                }

                menu.DropDown(button.worldBound);
            });
        }
    }
}
