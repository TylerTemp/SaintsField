using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using SaintsField.Editor.Core;
using SaintsField.Editor.Utils;
using UnityEditor;
#if UNITY_2021_3_OR_NEWER
using UnityEditor.UIElements;
using UnityEngine.UIElements;
#endif
using UnityEngine;
using Object = UnityEngine.Object;

namespace SaintsField.Editor.Drawers
{
    [CustomPropertyDrawer(typeof(ExpandableAttribute))]
    public class ExpandableAttributeDrawer: SaintsPropertyDrawer
    {
        #region IMGUI

        private string _error = "";

        // private bool _expanded;

        // There is a issue that, unity will randomly change _expanded value for IMGUI when clicking.
        // Plus Unity array uses same drawer instance for every element in an array,
        // so just use EditorPrefs here.

        // private static string KeyExpanded(SerializedProperty property) => $"{property.serializedObject.targetObject.GetInstanceID()}_{property.propertyPath}__Expandable_Expanded";
        //
        // private static bool GetExpand(SerializedProperty property)
        // {
        //     // bool isArray = SerializedUtils.PropertyPathIndex(property.propertyPath) != -1;
        //     // return isArray
        //     //     ? EditorPrefs.GetBool(KeyExpanded(property))
        //     //     : _expanded;
        //     return inMemoryStorage.TryGetValue(KeyExpanded(property), out object value) && (bool)value;
        // }

        // private static void SetExpand(SerializedProperty property, bool value) {
        //     // bool isArray = SerializedUtils.PropertyPathIndex(property.propertyPath) != -1;
        //     // if(isArray)
        //     // {
        //     //     EditorPrefs.SetBool(KeyExpanded(property), value);
        //     // }
        //     // else
        //     // {
        //     //     _expanded = value;
        //     // }
        //     // EditorPrefs.SetBool(KeyExpanded(property), value);
        //     inMemoryStorage[KeyExpanded(property)] = value;
        // }

        protected override float DrawPreLabelImGui(Rect position, SerializedProperty property,
            ISaintsAttribute saintsAttribute, FieldInfo info, object parent)
        {
            if(property.objectReferenceValue == null)
            {
                return -1;
            }

            // EditorGUI.DrawRect(position, Color.yellow);

            // bool isArray = SerializedUtils.PropertyPathIndex(property.propertyPath) != -1;

            // Rect drawPos = new Rect(position)
            // {
            //     x = position.x - 13,
            // };

            bool curExpanded = property.isExpanded;
#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_DRAW_PROCESS_EXPANDABLE
            Debug.Log($"cur expand {curExpanded}/{KeyExpanded(property)}");
#endif
            // ReSharper disable once ConvertToUsingDeclaration
            using(EditorGUI.ChangeCheckScope changed = new EditorGUI.ChangeCheckScope())
            {
                using(new GUIEnabledScoop(true))
                {
                    bool newExpanded = EditorGUI.Foldout(position, curExpanded,
                        new GUIContent(new string(' ', property.displayName.Length)), true);
                    if (changed.changed)
                    {
                        // SetExpand(property, newExpanded);
                        property.isExpanded = newExpanded;
                    }
                }
            }

            return 13;
        }

        protected override bool WillDrawBelow(SerializedProperty property, ISaintsAttribute saintsAttribute,
            FieldInfo info,
            object parent)
        {
            return true;
        }

        protected override float GetBelowExtraHeight(SerializedProperty property, GUIContent label, float width,
            ISaintsAttribute saintsAttribute, FieldInfo info, object parent)
        {
            float basicHeight = _error == "" ? 0 : ImGuiHelpBox.GetHeight(_error, width, MessageType.Error);

            if (!property.isExpanded || property.objectReferenceValue == null)
            {
                return basicHeight;
            }

            // ScriptableObject scriptableObject = property.objectReferenceValue as ScriptableObject;
            SerializedObject serializedObject = new SerializedObject(property.objectReferenceValue);

            // foreach (SerializedProperty serializedProperty in GetAllField(serializedObject))
            // {
            //     Debug.Log(serializedProperty);
            // }

            // SerializedProperty childProperty = GetAllField(serializedObject).First();
            // Debug.Log(childProperty);
            // Debug.Log(childProperty.displayName);
            // Debug.Log(EditorGUI.GetPropertyHeight(childProperty, true));
            //
            // return basicHeight;

            float expandedHeight = GetAllField(serializedObject).Select(childProperty =>
                EditorGUI.GetPropertyHeight(childProperty, true)).Sum();
            // float expandedHeight = 0;

            return basicHeight + expandedHeight;
        }

        // private UnityEditor.Editor _editor;

        protected override Rect DrawBelow(Rect position, SerializedProperty property, GUIContent label,
            ISaintsAttribute saintsAttribute, FieldInfo info, object parent)
        {
            Object scriptableObject = property.objectReferenceValue;
            _error = property.propertyType != SerializedPropertyType.ObjectReference
                ? $"Expected ScriptableObject type, get {property.propertyType}"
                : "";

            Rect leftRect = position;

            if (_error != "")
            {
                leftRect = ImGuiHelpBox.Draw(position, _error, MessageType.Error);
            }

            bool isExpand = property.isExpanded;
            // Debug.Log($"below expand = {isExpand}");
            if (!isExpand || scriptableObject == null)
            {
                return leftRect;
            }

            SerializedObject serializedObject = new SerializedObject(scriptableObject);
            serializedObject.Update();

            float usedHeight = 0f;

            Rect indentedRect;
            using (new EditorGUI.IndentLevelScope(1))
            {
                indentedRect = EditorGUI.IndentedRect(leftRect);
            }

            // _editor ??= UnityEditor.Editor.CreateEditor(scriptableObject);
            // _editor.OnInspectorGUI();

            List<(Rect, SerializedProperty)> propertyRects = new List<(Rect, SerializedProperty)>();

            using(new EditorGUI.IndentLevelScope(1))
            using(new AdaptLabelWidth())
            using(new ResetIndentScoop())
            {
                foreach (SerializedProperty childProperty in GetAllField(serializedObject))
                {
                    float childHeight = EditorGUI.GetPropertyHeight(childProperty, true);
                    Rect childRect = new Rect
                    {
                        x = indentedRect.x,
                        y = indentedRect.y + usedHeight,
                        width = indentedRect.width,
                        height = childHeight,
                    };

                    // EditorGUI.PropertyField(childRect, childProperty, true);
                    propertyRects.Add((childRect, childProperty));

                    usedHeight += childHeight;
                }
            }

            GUI.Box(new Rect(leftRect)
            {
                height = usedHeight,
            }, GUIContent.none);

            foreach ((Rect childRect, SerializedProperty childProperty) in propertyRects)
            {
                EditorGUI.PropertyField(childRect, childProperty, true);
                // EditorGUI.DrawRect(childRect, Color.blue);
            }

            serializedObject.ApplyModifiedProperties();

            return new Rect(leftRect)
            {
                y = leftRect.y + usedHeight,
                height = leftRect.height - usedHeight,
            };
        }

        #endregion

        private static IEnumerable<SerializedProperty> GetAllField(SerializedObject serializedScriptableObject)
        {
            // ReSharper disable once ConvertToUsingDeclaration
            using (SerializedProperty iterator = serializedScriptableObject.GetIterator())
            {
                if (!iterator.NextVisible(true))
                {
                    yield break;
                }

                do
                {
                    SerializedProperty childProperty = serializedScriptableObject.FindProperty(iterator.name);
                    if (childProperty.name.Equals("m_Script", System.StringComparison.Ordinal))
                    {
                        continue;
                    }

                    yield return childProperty;
                } while (iterator.NextVisible(false));
            }
        }

#if UNITY_2021_3_OR_NEWER

        #region UIToolkit

        private static string NameFoldout(SerializedProperty property) => $"{property.propertyPath}__ExpandableAttributeDrawer_Foldout";
        private static string NameProps(SerializedProperty property) => $"{property.propertyPath}__ExpandableAttributeDrawer_Props";

        protected override VisualElement CreatePostOverlayUIKit(SerializedProperty property,
            ISaintsAttribute saintsAttribute, int index,
            VisualElement container, object parent)
        {
            Foldout foldOut = new Foldout
            {
                style =
                {
                    // backgroundColor = Color.green,
                    // left = -5,
                    position = Position.Absolute,
                    width = LabelBaseWidth - IndentWidth,
                },
                name = NameFoldout(property),
                value = false,
            };

            foldOut.RegisterValueChangedCallback(v =>
            {
                container.Q<VisualElement>(NameProps(property)).style.display = v.newValue ? DisplayStyle.Flex : DisplayStyle.None;
            });

            return foldOut;
        }

        protected override VisualElement CreateBelowUIToolkit(SerializedProperty property,
            ISaintsAttribute saintsAttribute, int index,
            VisualElement container, FieldInfo info, object parent)
        {
            VisualElement visualElement = new VisualElement
            {
                style =
                {
                    display = DisplayStyle.None,
                    backgroundColor = EColor.CharcoalGray.GetColor(),
                },
                name = NameProps(property),
                userData = null,
            };

            visualElement.AddToClassList(ClassAllowDisable);

            return visualElement;
        }

        protected override void OnUpdateUIToolkit(SerializedProperty property, ISaintsAttribute saintsAttribute,
            int index,
            VisualElement container, Action<object> onValueChangedCallback, FieldInfo info, object parent)
        {
            Foldout foldOut = container.Q<Foldout>(NameFoldout(property));

            VisualElement propsElement = container.Q<VisualElement>(NameProps(property));
            Object curObject = (Object) propsElement.userData;

            if (ReferenceEquals(property.objectReferenceValue, curObject))
            {
                return;
            }

            DisplayStyle foldoutDisplay = property.objectReferenceValue == null ? DisplayStyle.None : DisplayStyle.Flex;
            if(foldOut.style.display != foldoutDisplay)
            {
                foldOut.style.display = foldoutDisplay;
            }

            propsElement.userData = property.objectReferenceValue;
            propsElement.Clear();
            if (property.objectReferenceValue == null)
            {
                return;
            }

            InspectorElement inspectorElement = new InspectorElement(property.objectReferenceValue)
            {
                // style =
                // {
                //     width = Length.Percent(100),
                // },
            };

            propsElement.Add(inspectorElement);

            // foreach (PropertyField propertyField in GetPropertyFields(property, property.objectReferenceValue))
            // {
            //     propsElement.Add(propertyField);
            // }
        }

        #endregion

#endif
    }
}
