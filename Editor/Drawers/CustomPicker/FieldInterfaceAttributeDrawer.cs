#if UNITY_2021_3_OR_NEWER
using UnityEditor.UIElements;
using UnityEngine.UIElements;
#endif
using System;
using System.Linq;
using System.Reflection;
using SaintsField.Editor.Core;
using SaintsField.Editor.Utils;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace SaintsField.Editor.Drawers.CustomPicker
{
    [CustomPropertyDrawer(typeof(FieldInterfaceAttribute))]
    public class FieldInterfaceAttributeDrawer: SaintsPropertyDrawer
    {
        private class FieldInterfaceSelectWindow : ObjectSelectWindow
        {
            private Type _fieldType;
            private Type _interfaceType;
            private Action<Object> _onSelected;
            private EPick _editorPick;

            public static void Open(Object curValue, EPick editorPick, Type originType, Type swappedType, Action<Object> onSelected)
            {
                FieldInterfaceSelectWindow thisWindow = CreateInstance<FieldInterfaceSelectWindow>();
                thisWindow.titleContent = new GUIContent($"Select {swappedType.Name}");
                // thisWindow._expectedTypes = expectedTypes;
                thisWindow._fieldType = originType;
                thisWindow._interfaceType = swappedType;
                thisWindow._onSelected = onSelected;
                thisWindow._editorPick = editorPick;
                thisWindow.SetDefaultActive(curValue);
                // Debug.Log($"call show selector window");
                thisWindow.ShowAuxWindow();
            }

            protected override bool AllowScene =>
                // Debug.Log(_editorPick);
                _editorPick.HasFlag(EPick.Scene);

            protected override bool AllowAssets =>
                // Debug.Log(_editorPick);
                _editorPick.HasFlag(EPick.Assets);

            private string _error = "";
            protected override string Error => _error;

            protected override bool IsEqual(ItemInfo itemInfo, Object target)
            {
                Object itemObject = itemInfo.Object;
                if (!itemObject)
                {
                    return false;
                }

                int targetInstanceId = target.GetInstanceID();
                if (itemInfo.InstanceID == targetInstanceId)
                {
                    return true;
                }

                // target=originalType(Component, GameObject) e.g. Sprite, SpriteRenderer
                // itemObject: Scene.GameObject, Assets.?

                // lets get the originalValue from the checking target
                bool originalIsGameObject = _fieldType == typeof(GameObject);
                Object itemToOriginTypeValue;
                switch (itemObject)
                {
                    case GameObject go:
                        itemToOriginTypeValue = originalIsGameObject ? go : go.GetComponent(_fieldType);
                        break;
                    case Component compo:
                        itemToOriginTypeValue = originalIsGameObject ? compo.gameObject : compo.GetComponent(_fieldType);
                        break;
                    default:
                        return false;
                }

                // Debug.Log($"{itemObject} ?= {target} => {itemToOriginTypeValue.GetInstanceID() == targetInstanceId}");
                return itemToOriginTypeValue.GetInstanceID() == targetInstanceId;
            }

            protected override void OnSelect(ItemInfo itemInfo)
            {
                _error = "";
                Object obj = itemInfo.Object;

                if(!FetchFilter(itemInfo))
                {
                    // Debug.LogError($"Selected object {obj} has no component {expectedType}");
                    _error = $"{itemInfo.Label} is invalid";
                    return;
                }
                _onSelected(obj);
            }

            protected override bool FetchAllSceneObjectFilter(ItemInfo itemInfo) => FetchFilter(itemInfo);

            protected override bool FetchAllAssetsFilter(ItemInfo itemInfo) => FetchFilter(itemInfo);

            private bool FetchFilter(ItemInfo itemInfo)  // gameObject, Sprite, Texture2D, ...
            {
                if (itemInfo.Object == null)
                {
                    return true;
                }

                // if item is GameObject, then check any components with required interface
                // otherwise just check the item target
                if (itemInfo.Object is GameObject go)
                {
                    return go.GetComponent(_interfaceType) != null && go.GetComponent(_fieldType) != null;
                }

                Type itemType = itemInfo.Object.GetType();
                return itemType.IsInstanceOfType(_interfaceType) && itemType.IsInstanceOfType(_fieldType);
            }
        }

        #region IMGUI
        private string _error = "";

        protected override float GetFieldHeight(SerializedProperty property, GUIContent label,
            ISaintsAttribute saintsAttribute, bool hasLabelWidth) => EditorGUIUtility.singleLineHeight;

        protected override void DrawField(Rect position, SerializedProperty property, GUIContent label,
            ISaintsAttribute saintsAttribute, OnGUIPayload onGUIPayload, FieldInfo info, object parent)
        {
            FieldInterfaceAttribute fieldInterfaceAttribute = (FieldInterfaceAttribute)saintsAttribute;
            Type requiredInterface = fieldInterfaceAttribute.InterfaceType;
            Type fieldType = SerializedUtils.GetType(property);
            // Object requiredValue;
            // try
            // {
            //     requiredValue = GetValue(property, fieldType, requiredInterface);
            // }
            // catch (Exception e)
            // {
            //     Debug.LogException(e);
            //     _error = e.Message;
            //     DefaultDrawer(position, property, label, info);
            //     return;
            // }

            EPick editorPick = fieldInterfaceAttribute.EditorPick;
            bool customPicker = fieldInterfaceAttribute.CustomPicker;

            // ReSharper disable once ConvertToUsingDeclaration
            using (EditorGUI.ChangeCheckScope changed = new EditorGUI.ChangeCheckScope())
            {
                Rect fieldRect = customPicker
                    ? new Rect(position)
                    {
                        width = position.width - 20,
                    }
                    : position;

                Object previousValue = property.objectReferenceValue;

                EditorGUI.PropertyField(fieldRect, property, label);
                // ReSharper disable once InvertIf
                if (changed.changed)
                {
                    Object curValue = property.objectReferenceValue;
                    bool matched;
                    switch (curValue)
                    {
                        case null:
                            matched = true;
                            break;
                        case GameObject go:
                            matched = go.GetComponent(requiredInterface) != null;
                            break;
                        case Component comp:
                            matched = comp.GetComponent(requiredInterface) != null;
                            break;
                        default:
                            matched = curValue.GetType().IsInstanceOfType(requiredInterface);
                            break;
                    }

                    if (!matched)
                    {
                        property.objectReferenceValue = previousValue;
                    }
                    // report it anyway because SaintsPropertyDrawer will check the value to decide changes,
                    // which is earlier than this check
                    onGUIPayload.SetValue(property.objectReferenceValue);
                }
            }

            if(customPicker)
            {
                Rect overrideButtonRect = new Rect(position.x + position.width - 21, position.y, 21, position.height);
                if (GUI.Button(overrideButtonRect, "●"))
                {
                    // Type[] types = requiredComp  == fieldType
                    //     ? new []{requiredComp}
                    //     : new []{requiredComp, fieldType};
                    FieldInterfaceSelectWindow.Open(property.objectReferenceValue, editorPick, fieldType, requiredInterface, fieldResult =>
                    {
                        Object result = OnSelectWindowSelected(fieldResult, fieldType);
                        property.objectReferenceValue = result;
                        property.serializedObject.ApplyModifiedProperties();
                        onGUIPayload.SetValue(result);
                    });
                }
            }
        }

        private static Object OnSelectWindowSelected(Object fieldResult, Type fieldType)
        {
            Object result = null;
            switch (fieldResult)
            {
                case null:
                    // property.objectReferenceValue = null;
                    break;
                case GameObject go:
                    result = fieldType == typeof(GameObject) ? go : go.GetComponent(fieldType);
                    // Debug.Log($"isGo={fieldType == typeof(GameObject)},  fieldResult={fieldResult.GetType()} result={result.GetType()}");
                    break;
                case Component comp:
                    result = fieldType == typeof(GameObject)
                        ? comp.gameObject
                        : comp.GetComponent(fieldType);
                    break;
            }

            return result;
        }

        protected override bool WillDrawBelow(SerializedProperty property, ISaintsAttribute saintsAttribute,
            FieldInfo info,
            object parent) => _error != "";

        protected override float GetBelowExtraHeight(SerializedProperty property, GUIContent label, float width,
            ISaintsAttribute saintsAttribute, FieldInfo info, object parent) => _error == "" ? 0 : ImGuiHelpBox.GetHeight(_error, EditorGUIUtility.currentViewWidth, MessageType.Error);

        protected override Rect DrawBelow(Rect position, SerializedProperty property, GUIContent label,
            ISaintsAttribute saintsAttribute, FieldInfo info, object parent) => ImGuiHelpBox.Draw(position, _error, MessageType.Error);
        #endregion

        private static Object GetValue(SerializedProperty property, Type fieldType, Type requiredComp)
        {
            bool fieldTypeIsGameObject = fieldType == typeof(GameObject);
            bool requiredCompIsGameObject = requiredComp == typeof(GameObject);

            // ReSharper disable once ConvertIfStatementToSwitchStatement
            if (fieldTypeIsGameObject && requiredCompIsGameObject)
            {
                return property.objectReferenceValue;
            }

            if (!fieldTypeIsGameObject && !requiredCompIsGameObject)
            {
                return ((Component)property.objectReferenceValue)?.GetComponent(requiredComp);
            }

            if (fieldTypeIsGameObject)
            {
                return ((GameObject)property.objectReferenceValue)?.GetComponent(requiredComp);
            }

            return ((Component)property.objectReferenceValue)?.gameObject;
        }

        private static Object GetNewValue(Object fieldResult, Type fieldType, Type requiredComp)
        {
            bool requiredCompIsGameObject = requiredComp == typeof(GameObject);
            bool fieldTypeIsGameObject = fieldType == typeof(GameObject);

            // ReSharper disable once ConvertIfStatementToSwitchStatement
            if (requiredCompIsGameObject && fieldTypeIsGameObject)
            {
                return fieldResult;
            }

            if (!requiredCompIsGameObject && !fieldTypeIsGameObject)
            {
                return ((Component)fieldResult)?.GetComponent(fieldType);
            }

            if (requiredCompIsGameObject)
            {
                return ((GameObject)fieldResult)?.GetComponent(fieldType);
            }
            return ((Component)fieldResult)?.gameObject;
        }

#if UNITY_2021_3_OR_NEWER

        #region UIToolkit

        private static string NameObjectField(SerializedProperty property) => $"{property.propertyPath}__FieldType_ObjectField";
        private static string NameHelpBox(SerializedProperty property) => $"{property.propertyPath}__FieldType_HelpBox";
        private static string NameSelectorButton(SerializedProperty property) => $"{property.propertyPath}__FieldType_SelectorButton";

        protected override VisualElement CreateFieldUIToolKit(SerializedProperty property,
            ISaintsAttribute saintsAttribute,
            VisualElement container, Label fakeLabel, FieldInfo info, object parent)
        {
            FieldInterfaceAttribute fieldInterfaceAttribute = (FieldInterfaceAttribute)saintsAttribute;
            bool customPicker = fieldInterfaceAttribute.CustomPicker;
            Type requiredInterface = fieldInterfaceAttribute.InterfaceType;
            Type fieldType = SerializedUtils.GetType(property);
            // Object requiredValue;

            // Debug.Log($"property.Object={property.objectReferenceValue}");

            // try
            // {
            //     requiredValue = GetValue(property, fieldType, requiredComp);
            // }
            // catch (Exception e)
            // {
            //     Debug.LogException(e);
            //
            //     VisualElement root = new VisualElement();
            //     root.Add(SaintsFallbackUIToolkit(property));
            //     root.Add(new HelpBox(e.Message, HelpBoxMessageType.Error));
            //     return root;
            // }

            // Debug.Log($"requiredValue={requiredValue}");

            // PropertyField has some issue here for decoration etc. Need some test.
            ObjectField objectField = new ObjectField(new string(' ', property.displayName.Length))
            {
                name = NameObjectField(property),
                objectType = requiredInterface,
                allowSceneObjects = fieldInterfaceAttribute.EditorPick.HasFlag(EPick.Scene),
                value = property.objectReferenceValue,
                style =
                {
                    flexShrink = 1,
                },
            };

            objectField.Bind(property.serializedObject);

            if (customPicker)
            {
                StyleSheet hideStyle = Util.LoadResource<StyleSheet>("UIToolkit/PropertyFieldHideSelector.uss");
                objectField.styleSheets.Add(hideStyle);
                Button selectorButton = new Button
                {
                    text = "●",
                    style =
                    {
                        position = Position.Absolute,
                        right = 0,
                        width = 18,
                        marginLeft = 0,
                        marginRight = 0,
                    },
                    name = NameSelectorButton(property),
                };

                objectField.Add(selectorButton);
            }

            return objectField;
        }

        protected override void OnAwakeUIToolkit(SerializedProperty property, ISaintsAttribute saintsAttribute,
            int index, VisualElement container,
            Action<object> onValueChangedCallback, FieldInfo info, object parent)
        {
            FieldInterfaceAttribute fieldInterfaceAttribute = (FieldInterfaceAttribute)saintsAttribute;
            Type requiredComp = fieldInterfaceAttribute.InterfaceType;
            Type fieldType = SerializedUtils.GetType(property);
            EPick editorPick = fieldInterfaceAttribute.EditorPick;

            ObjectField objectField = container.Q<ObjectField>(NameObjectField(property));

            objectField.RegisterValueChangedCallback(v =>
            {
                Object result = GetNewValue(v.newValue, fieldType, requiredComp);
                property.objectReferenceValue = result;
                property.serializedObject.ApplyModifiedProperties();
                onValueChangedCallback.Invoke(result);

                HelpBox helpBox = container.Q<HelpBox>(NameHelpBox(property));
                if (v.newValue != null && result == null)
                {
                    helpBox.style.display = DisplayStyle.Flex;
                    helpBox.text = $"{v.newValue} has no component {fieldType}";
                }
                else
                {
                    helpBox.style.display = DisplayStyle.None;
                }
            });

            Button selectorButton = container.Q<Button>(NameSelectorButton(property));
            if (selectorButton != null)
            {
                selectorButton.clicked += () =>
                {
                    FieldInterfaceSelectWindow.Open(property.objectReferenceValue, editorPick, fieldType, requiredComp, fieldResult =>
                    {
                        Object result = OnSelectWindowSelected(fieldResult, fieldType);
                        property.objectReferenceValue = result;
                        property.serializedObject.ApplyModifiedProperties();
                        onValueChangedCallback.Invoke(result);
                    });
                };
            }
        }

        protected override VisualElement CreateBelowUIToolkit(SerializedProperty property,
            ISaintsAttribute saintsAttribute, int index,
            VisualElement container, FieldInfo info, object parent)
        {
            return new HelpBox("", HelpBoxMessageType.Error)
            {
                style =
                {
                    display = DisplayStyle.None,
                },
                name = NameHelpBox(property),
            };
        }

        protected override void ChangeFieldLabelToUIToolkit(SerializedProperty property,
            ISaintsAttribute saintsAttribute, int index, VisualElement container, string labelOrNull)
        {
            ObjectField target = container.Q<ObjectField>(NameObjectField(property));
            target.label = labelOrNull;
        }

        #endregion

#endif
    }
}
