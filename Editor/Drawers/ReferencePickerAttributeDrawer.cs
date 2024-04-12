#if UNITY_2021_3_OR_NEWER
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using SaintsField.Editor.Core;
using UnityEditor;
using UnityEngine;
#if UNITY_2021_3_OR_NEWER
using UnityEditor.UIElements;
using UnityEngine.UIElements;
#endif

namespace SaintsField.Editor.Drawers
{
    [CustomPropertyDrawer(typeof(ReferencePickerAttribute))]
    public class ReferencePickerAttributeDrawer: SaintsPropertyDrawer
    {
        private static IEnumerable<Type> GetTypes(SerializedProperty property)
        {
            string typename = property.managedReferenceFieldTypename;
            // Debug.Log(typename);
            string[] typeSplitString = typename.Split(' ');
            string typeAssemblyName = typeSplitString[0];
            string typeContainerSlashClass = typeSplitString[1];
            Type realType = Type.GetType($"{typeContainerSlashClass}, {typeAssemblyName}");
            // Debug.Log(realType);

            return TypeCache.GetTypesDerivedFrom(realType)
                .Prepend(realType)
                .Where(each => !each.IsSubclassOf(typeof(UnityEngine.Object)))
                .Where(each => !each.IsAbstract) // abstract classes
                .Where(each => !each.ContainsGenericParameters) // generic classes
                .Where(each => !each.IsClass || each.GetConstructor(Type.EmptyTypes) != null);
        }

        private static object CopyObj(object oldObj, object newObj)
        {
            if (newObj == null || oldObj == null)
            {
                return newObj;
            }
            // MyObject copyObject = ...
            Type type = oldObj.GetType();
            while (type != null)
            {
                UpdateForType(type, oldObj, newObj);
                type = type.BaseType;
            }

            return newObj;
        }

        private static void UpdateForType(Type type, object source, object destination)
        {
            FieldInfo[] myObjectFields = type.GetFields(
                BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);

            foreach (FieldInfo fi in myObjectFields)
            {
                try
                {
                    fi.SetValue(destination, fi.GetValue(source));
                }
                catch (Exception)
                {
                    // do nothing
                    // Debug.LogException(e);
                }
            }
        }

        #region IMGUI

        private const float ImGuiButtonWidth = 20f;

        protected override bool DrawPostFieldImGui(Rect position, SerializedProperty property, GUIContent label,
            ISaintsAttribute saintsAttribute,
            int index,
            OnGUIPayload onGUIPayload, FieldInfo info, object parent)
        {
            object managedReferenceValue = property.managedReferenceValue;

            string displayLabel = managedReferenceValue == null
                ? ""
                : managedReferenceValue.GetType().Name;

            GUIContent fullLabel = new GUIContent(displayLabel);
            GUIStyle textStyle = new GUIStyle(EditorStyles.label)
            {
                richText = true,
            };
            float width = textStyle.CalcSize(fullLabel).x;
            GUI.Label(new Rect(position)
            {
                x = position.x - width,
                width = width,
                height = SingleLineHeight,
            }, fullLabel, textStyle);

            // Rect newPos = new Rect(position)
            // {
            //     height = SingleLineHeight,
            //     width = width,
            //     x = position.x + position.width - width,
            // };

            // (Rect labelRect, Rect dropdownRect) = Utils.RectUtils.SplitWidthRect(new Rect(position)
            // {
            //     height = SingleLineHeight,
            // }, position.width - 20);
            //
            // EditorGUI.DrawRect(labelRect, Color.yellow);
            // EditorGUI.DrawRect(dropdownRect, Color.blue);
            //
            // GUI.Label(labelRect, displayLabel);

            Rect dropdownRect = new Rect(position)
            {
                height = SingleLineHeight,
            };

            // ReSharper disable once InvertIf
            if (EditorGUI.DropdownButton(dropdownRect, new GUIContent(" "), FocusType.Keyboard))
            {
                GenericMenu genericDropdownMenu = new GenericMenu();
                genericDropdownMenu.AddItem(new GUIContent("[Null]"), managedReferenceValue == null, () =>
                {
                    property.managedReferenceValue = null;
                    property.serializedObject.ApplyModifiedProperties();
                    onGUIPayload.SetValue(null);
                });
                genericDropdownMenu.AddSeparator("");

                foreach (Type type in GetTypes(property))
                {
                    // string assemblyName =  type.Assembly.ToString().Split('(', ',')[0];
                    string displayName = $"{type.Name}: {type.Namespace}";

                    genericDropdownMenu.AddItem(new GUIContent(displayName), managedReferenceValue != null && managedReferenceValue.GetType() == type, () =>
                    {
                        object instance = Activator.CreateInstance(type);
                        property.managedReferenceValue = instance;
                        property.serializedObject.ApplyModifiedProperties();
                        // property.serializedObject.SetIsDifferentCacheDirty();
                        onGUIPayload.SetValue(instance);
                    });
                }
                genericDropdownMenu.DropDown(new Rect(position)
                {
                    x = 0,
                    width = EditorGUIUtility.currentViewWidth,
                    height = SingleLineHeight,
                });
            }

            // EditorGUI.DrawRect(new Rect(position)
            // {
            //     x = 0,
            //     width = EditorGUIUtility.currentViewWidth,
            // }, Color.red);

            return true;
        }

        protected override float GetPostFieldWidth(Rect position, SerializedProperty property, GUIContent label,
            ISaintsAttribute saintsAttribute, FieldInfo info, object parent)
        {
            return ImGuiButtonWidth;
        }

        #endregion

#if UNITY_2021_3_OR_NEWER

        #region UI Toolkit
        // private static string NamePropertyContainer(SerializedProperty property) => $"{property.propertyPath}__Reference_PropertyField_Container";
        // private static string NamePropertyField(SerializedProperty property) => $"{property.propertyPath}__Reference_PropertyField";
        private static string NameButton(SerializedProperty property) => $"{property.propertyPath}__Reference_Button";
        private static string NameLabel(SerializedProperty property) => $"{property.propertyPath}__Reference_Label";

        protected override VisualElement CreatePostFieldUIToolkit(SerializedProperty property,
            ISaintsAttribute saintsAttribute, int index, VisualElement container, FieldInfo info, object parent)
        {
            // VisualElement root = new VisualElement
            // {
            //     style =
            //     {
            //         width = 40,
            //     },
            // };

            Button button = new Button
            {
                name = NameButton(property),
                text = "▼",
                style =
                {
                    height = SingleLineHeight - 2,
                    // maxHeight = SingleLineHeight,
                    width = SingleLineHeight - 2,
                    borderLeftWidth = 0,
                    borderRightWidth = 0,
                    borderTopWidth = 0,
                    borderBottomWidth = 0,
                    paddingLeft = 0,
                    paddingRight = 0,
                    paddingTop = 0,
                    paddingBottom = 0,
                    // marginTop = 5,
                    // borderTopLeftRadius = 0,
                    // borderTopRightRadius = 0,
                    // borderBottomLeftRadius = 0,
                    // borderBottomRightRadius = 0,
                    // backgroundColor = Color.clear,

                    flexDirection = FlexDirection.Row,
                    // justifyContent = Justify.FlexEnd,
                    overflow = Overflow.Visible,
                },
            };

            object curValue = property.managedReferenceValue;
            string labelName = curValue == null
                ? ""
                : curValue.GetType().Name;

            Label label = new Label(labelName)
            {
                name = NameLabel(property),
                // style =
                // {
                //     minWidth = 0,
                // },
                style =
                {
                    position = Position.Absolute,
                    right = SingleLineHeight,
                    // translate = new Translate(Length.Percent(-100), Length.Auto()),
                    // paddingRight = 1,
                },
                pickingMode = PickingMode.Ignore,
            };
            button.Add(label);

            // button.Add(new Label("▼")
            // {
            //     // style =
            //     // {
            //     //     flexShrink = 0,
            //     // },
            // });

            // root.Add(button);

            button.AddToClassList(ClassAllowDisable);

            return button;
        }

        protected override void OnAwakeUIToolkit(SerializedProperty property, ISaintsAttribute saintsAttribute,
            int index, VisualElement container,
            Action<object> onValueChangedCallback, FieldInfo info, object parent)
        {
            Button button = container.Q<Button>(NameButton(property));
            // VisualElement propertyContainer = container.Q<VisualElement>(NamePropertyContainer(property));
            button.clicked += () =>
            {
                // string typename = property.managedReferenceFieldTypename;
                // string[] typeSplitString = typename.Split(' ');
                // string typeClassName = typeSplitString[1];
                // string typeAssemblyName = typeSplitString[0];
                // Type realType = Type.GetType($"{typeClassName}, {typeAssemblyName}");
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

                foreach (Type type in GetTypes(property))
                {
                    // string assemblyName =  type.Assembly.ToString().Split('(', ',')[0];
                    string displayName = $"{type.Name}: {type.Namespace}";

                    genericDropdownMenu.AddItem(displayName, managedReferenceValue != null && managedReferenceValue.GetType() == type, () =>
                    {
                        object instance = CopyObj(managedReferenceValue, Activator.CreateInstance(type));
                        PropSetValue(container, property, instance);

                        onValueChangedCallback(instance);
                    });
                }

                Rect fakePos = container.worldBound;
                fakePos.height = SingleLineHeight;

                genericDropdownMenu.DropDown(fakePos, container, true);
            };
        }

        protected override void OnValueChanged(SerializedProperty property, ISaintsAttribute saintsAttribute, int index,
            VisualElement container,
            FieldInfo info,
            object parent, Action<object> onValueChangedCallback, object newValue)
        {
            UpdateLabel(property, container, newValue);
        }

        protected override void OnUpdateUIToolkit(SerializedProperty property, ISaintsAttribute saintsAttribute,
            int index,
            VisualElement container, Action<object> onValueChanged, FieldInfo info, object parent)
        {
            UpdateLabel(property, container, property.managedReferenceValue);
        }

        private static void PropSetValue(VisualElement container, SerializedProperty property, object newValue)
        {
            property.serializedObject.Update();
            // property.managedReferenceValue = null;
            property.managedReferenceValue = newValue;
            // property.managedReferenceId = -2L;
            property.serializedObject.ApplyModifiedProperties();
            // property.serializedObject.SetIsDifferentCacheDirty();

            container.Query<PropertyField>(className: SaintsFieldFallbackClass).ForEach(each => each.BindProperty(property));
        }

        private static void UpdateLabel(SerializedProperty property, VisualElement container, object newValue)
        {
            Label label = container.Q<Label>(NameLabel(property));
            string newLabel = newValue == null
                ? ""
                : newValue.GetType().Name;

            if(label.text != newLabel)
            {
                label.text = newLabel;
            }
        }
        #endregion

#endif
    }
}
#endif
