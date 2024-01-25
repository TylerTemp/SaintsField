using System;
using System.Collections.Generic;
using System.Linq;
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

        private bool _expanded;

        private static string KeyExpanded(SerializedProperty property) => $"{property.serializedObject.targetObject.GetInstanceID()}_{property.propertyPath}__Expandable_Expanded";

        private bool GetExpand(SerializedProperty property)
        {
            bool isArray = SerializedUtils.PropertyPathIndex(property.propertyPath) != -1;
            return isArray
                ? EditorPrefs.GetBool(KeyExpanded(property))
                : _expanded;
        }

        private void SetExpand(SerializedProperty property, bool value) {
            bool isArray = SerializedUtils.PropertyPathIndex(property.propertyPath) != -1;
            if(isArray)
            {
                EditorPrefs.SetBool(KeyExpanded(property), value);
            }
            else
            {
                _expanded = value;
            }
        }

        protected override (bool isActive, Rect position) DrawPreLabelImGui(Rect position, SerializedProperty property, ISaintsAttribute saintsAttribute)
        {
            if(property.objectReferenceValue == null)
            {
                return (false, position);
            }

            bool isArray = SerializedUtils.PropertyPathIndex(property.propertyPath) != -1;

            Rect drawPos = isArray
                ? new Rect(position)
                {
                    x = position.x - 12,
                }
                : position;

            if(isArray)
            {
                using(EditorGUI.ChangeCheckScope changed = new EditorGUI.ChangeCheckScope())
                {
                    bool newExpanded = EditorGUI.Foldout(drawPos, GetExpand(property), GUIContent.none, true);
                    if (changed.changed)
                    {
                        SetExpand(property, newExpanded);
                    }
                }
            }
            else
            {
                _expanded = EditorGUI.Foldout(drawPos, _expanded, GUIContent.none, true);
            }
            return (true, position);
        }

        protected override bool WillDrawBelow(SerializedProperty property, ISaintsAttribute saintsAttribute)
        {
            return true;
        }

        protected override float GetBelowExtraHeight(SerializedProperty property, GUIContent label, float width, ISaintsAttribute saintsAttribute)
        {
            float basicHeight = _error == "" ? 0 : ImGuiHelpBox.GetHeight(_error, width, MessageType.Error);

            if (!GetExpand(property) || property.objectReferenceValue == null)
            {
                return basicHeight;
            }

            // ScriptableObject scriptableObject = property.objectReferenceValue as ScriptableObject;
            SerializedObject serializedObject = new SerializedObject(property.objectReferenceValue);
            float expandedHeight = GetAllField(serializedObject).Select(childProperty =>
                GetPropertyHeight(childProperty, new GUIContent(childProperty.displayName))).Sum();

            return basicHeight + expandedHeight;
        }

        // private UnityEditor.Editor _editor;

        protected override Rect DrawBelow(Rect position, SerializedProperty property, GUIContent label, ISaintsAttribute saintsAttribute)
        {
            Object scriptableObject = property.objectReferenceValue;
            _error = property.propertyType != SerializedPropertyType.ObjectReference
                ? $"Expected ScriptableObject type, get {property.propertyType}"
                : "";
            // else if (!(property.objectReferenceValue is ScriptableObject))
            // {
            //     _error = $"Expected ScriptableObject type, get {property.objectReferenceValue.GetType()}";
            // }
            //
            //
            // if (_editor == null)
            // {
            //     _editor = UnityEditor.Editor.CreateEditor(scriptableObject);
            // }
            //
            // _editor.DrawDefaultInspector();
            //
            // return position;

            Rect leftRect = position;

            if (_error != "")
            {
                leftRect = ImGuiHelpBox.Draw(position, _error, MessageType.Error);
            }

            if (!GetExpand(property) || scriptableObject == null)
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
                    float childHeight = GetPropertyHeight(childProperty, new GUIContent(childProperty.displayName));
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
                    // height = EditorGUIUtility.singleLineHeight,
                    // width = 20,
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

        protected override VisualElement CreateBelowUIToolkit(SerializedProperty property, ISaintsAttribute saintsAttribute, int index,
            VisualElement container, object parent)
        {
            // InspectorElement visualElement = new InspectorElement
            // {
            //     style =
            //     {
            //         width = Length.Percent(100),
            //         display = DisplayStyle.None,
            //     },
            //     name = NameProps(property),
            //     userData = null,
            // };

            VisualElement visualElement = new VisualElement
            {
                style =
                {
                    display = DisplayStyle.None,
                },
                name = NameProps(property),
                userData = null,
            };

            return visualElement;
        }

        // private static IEnumerable<PropertyField> GetPropertyFields(SerializedProperty property, Object obj)
        // {
        //     SerializedObject serializedObject = new SerializedObject(obj);
        //     serializedObject.Update();
        //
        //     foreach (SerializedProperty childProperty in GetAllField(serializedObject))
        //     {
        //         PropertyField prop = new PropertyField(childProperty)
        //         {
        //             style =
        //             {
        //                 paddingLeft = IndentWidth,
        //             },
        //         };
        //         prop.AddToClassList($"{property.propertyPath}__ExpandableAttributeDrawer_Prop");
        //         prop.Bind(serializedObject);
        //         // visualElement.Add(prop);
        //         yield return prop;
        //     }
        // }

        protected override void OnUpdateUIToolkit(SerializedProperty property, ISaintsAttribute saintsAttribute,
            int index,
            VisualElement container, Action<object> onValueChangedCallback, object parent)
        {
            Foldout foldOut = container.Q<Foldout>(NameFoldout(property));

            VisualElement propsElement = container.Q<VisualElement>(NameProps(property));
            Object curObject = (Object) propsElement.userData;

            if (ReferenceEquals(property.objectReferenceValue, curObject))
            {
                return;
            }
            foldOut.style.display = property.objectReferenceValue == null? DisplayStyle.None : DisplayStyle.Flex;

            propsElement.userData = property.objectReferenceValue;
            propsElement.Clear();
            if (property.objectReferenceValue == null)
            {
                return;
            }

            propsElement.Add(new InspectorElement(property.objectReferenceValue)
            {
                // style =
                // {
                //     width = Length.Percent(100),
                // },
            });

            // foreach (PropertyField propertyField in GetPropertyFields(property, property.objectReferenceValue))
            // {
            //     propsElement.Add(propertyField);
            // }
        }

        #endregion

#endif
    }
}
