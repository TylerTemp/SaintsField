#if UNITY_2021_3_OR_NEWER
using UnityEditor.UIElements;
using UnityEngine.UIElements;
#endif
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using SaintsField.Editor.Core;
using SaintsField.Editor.Utils;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace SaintsField.Editor.Drawers.CustomPicker
{
    [CustomPropertyDrawer(typeof(FieldTypeAttribute))]
    public class FieldTypeAttributeDrawer: SaintsPropertyDrawer
    {
        private class FieldTypeSelectWindow : ObjectSelectWindow
        {
            // private Type[] _expectedTypes;
            private Type _originType;
            private Type _swappedType;
            private Action<Object> _onSelected;
            private EPick _editorPick;

            public static void Open(Object curValue, EPick editorPick, Type originType, Type swappedType, Action<Object> onSelected)
            {
                FieldTypeSelectWindow thisWindow = CreateInstance<FieldTypeSelectWindow>();
                thisWindow.titleContent = new GUIContent($"Select {swappedType.Name}");
                // thisWindow._expectedTypes = expectedTypes;
                thisWindow._originType = originType;
                thisWindow._swappedType = swappedType;
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
                bool originalIsGameObject = _originType == typeof(GameObject);
                Object itemToOriginTypeValue;
                switch (itemObject)
                {
                    case GameObject go:
                        itemToOriginTypeValue = originalIsGameObject ? (Object)go : go.GetComponent(_originType);
                        break;
                    case Component compo:
                        itemToOriginTypeValue = originalIsGameObject ? (Object)compo.gameObject : compo.GetComponent(_originType);
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

            private bool FetchFilter(ItemInfo itemInfo)
            {
                if (itemInfo.Object == null)
                {
                    return true;
                }
                Type[] expectedTypes = _originType == _swappedType
                    ? new[] {_originType}
                    : new[] {_originType, _swappedType};
                // return  || _expectedTypes.All(each => CanSign(itemInfo.Object, each));
                return expectedTypes.All(each => CanSign(itemInfo.Object, each));
            }
        }

        private static bool CanSign(Object target, Type type)
        {
            if(type.IsInstanceOfType(target))
            {
                return true;
            }

            bool expectedIsGameObject = type == typeof(GameObject);
            switch (target)
            {
                case GameObject go:
                    if (expectedIsGameObject)
                    {
                        return true;
                    }
                    return go.GetComponent(type) != null;
                case Component comp:
                    return comp.GetComponent(type) != null;
                default:
                    return false;
            }
        }

        #region IMGUI
        private string _error = "";

        protected override float GetFieldHeight(SerializedProperty property, GUIContent label,
            ISaintsAttribute saintsAttribute, FieldInfo info, bool hasLabelWidth, object parent) => EditorGUIUtility.singleLineHeight;

        protected override void DrawField(Rect position, SerializedProperty property, GUIContent label,
            ISaintsAttribute saintsAttribute, OnGUIPayload onGUIPayload, FieldInfo info, object parent)
        {
            FieldTypeAttribute fieldTypeAttribute = (FieldTypeAttribute)saintsAttribute;
            Type fieldType = ReflectUtils.GetElementType(info.FieldType);
            Type requiredComp = fieldTypeAttribute.CompType ?? fieldType;
            Object requiredValue;
            try
            {
                requiredValue = GetValue(property, fieldType, requiredComp);
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                _error = e.Message;
                DefaultDrawer(position, property, label, info);
                return;
            }

            EPick editorPick = fieldTypeAttribute.EditorPick;
            bool customPicker = fieldTypeAttribute.CustomPicker;

            // ReSharper disable once ConvertToUsingDeclaration
            using (EditorGUI.ChangeCheckScope changed = new EditorGUI.ChangeCheckScope())
            {
                Rect fieldRect = customPicker
                    ? new Rect(position)
                    {
                        width = position.width - 20,
                    }
                    : position;

                Object fieldResult =
                    EditorGUI.ObjectField(fieldRect, label, requiredValue, requiredComp, editorPick.HasFlag(EPick.Scene));
                // ReSharper disable once InvertIf
                if (changed.changed)
                {
                    Object result = GetNewValue(fieldResult, fieldType, requiredComp);
                    property.objectReferenceValue = result;

                    if (fieldResult != null && result == null)
                    {
                        _error = $"{fieldResult} has no component {fieldType}";
                    }
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
                    FieldTypeSelectWindow.Open(property.objectReferenceValue, editorPick, fieldType, requiredComp, fieldResult =>
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
            return Util.GetTypeFromObj(fieldResult, fieldType);
            // Object result = null;
            // switch (fieldResult)
            // {
            //     case null:
            //         // property.objectReferenceValue = null;
            //         break;
            //     case GameObject go:
            //         result = fieldType == typeof(GameObject) ? (Object)go : go.GetComponent(fieldType);
            //         // Debug.Log($"isGo={fieldType == typeof(GameObject)},  fieldResult={fieldResult.GetType()} result={result.GetType()}");
            //         break;
            //     case Component comp:
            //         result = fieldType == typeof(GameObject)
            //             ? (Object)comp.gameObject
            //             : comp.GetComponent(fieldType);
            //         break;
            // }
            //
            // return result;
        }

        protected override bool WillDrawBelow(SerializedProperty property, ISaintsAttribute saintsAttribute,
            int index,
            FieldInfo info,
            object parent) => _error != "";

        protected override float GetBelowExtraHeight(SerializedProperty property, GUIContent label, float width,
            ISaintsAttribute saintsAttribute, int index, FieldInfo info, object parent) => _error == "" ? 0 : ImGuiHelpBox.GetHeight(_error, EditorGUIUtility.currentViewWidth, MessageType.Error);

        protected override Rect DrawBelow(Rect position, SerializedProperty property, GUIContent label,
            ISaintsAttribute saintsAttribute, int index, FieldInfo info, object parent) => ImGuiHelpBox.Draw(position, _error, MessageType.Error);
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

            // Debug.Log($"fieldResult={fieldResult}, fieldType={fieldType}, requiredComp={requiredComp}; requiredCompIsGameObject={requiredCompIsGameObject}; fieldTypeIsGameObject={fieldTypeIsGameObject}");

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

            if (fieldResult is GameObject go)
            {
                return go;
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
            VisualElement container, FieldInfo info, object parent)
        {
            FieldTypeAttribute fieldTypeAttribute = (FieldTypeAttribute)saintsAttribute;
            bool customPicker = fieldTypeAttribute.CustomPicker;
            Type fieldType = ReflectUtils.GetElementType(info.FieldType);
            Type requiredComp = fieldTypeAttribute.CompType ?? fieldType;
            Object requiredValue;

            // Debug.Log($"property.Object={property.objectReferenceValue}");

            try
            {
                requiredValue = GetValue(property, fieldType, requiredComp);
            }
            catch (Exception e)
            {
                Debug.LogException(e);

                VisualElement root = new VisualElement();
                root.Add(PropertyFieldFallbackUIToolkit(property));
                root.Add(new HelpBox(e.Message, HelpBoxMessageType.Error));
                root.AddToClassList(ClassAllowDisable);
                return root;
            }

            // Debug.Log($"requiredValue={requiredValue}");

            ObjectField objectField = new ObjectField(property.displayName)
            {
                name = NameObjectField(property),
                objectType = requiredComp,
                allowSceneObjects = true,
                value = requiredValue,
                style =
                {
                    flexShrink = 1,
                },
            };

            objectField.Bind(property.serializedObject);
            objectField.AddToClassList(BaseField<Object>.alignedFieldUssClassName);

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

            objectField.AddToClassList(ClassAllowDisable);

            return objectField;
        }

        protected override VisualElement CreateBelowUIToolkit(SerializedProperty property,
            ISaintsAttribute saintsAttribute, int index,
            VisualElement container, FieldInfo info, object parent)
        {
            HelpBox helpBox = new HelpBox("", HelpBoxMessageType.Error)
            {
                style =
                {
                    display = DisplayStyle.None,
                },
                name = NameHelpBox(property),
            };
            helpBox.AddToClassList(ClassAllowDisable);
            return helpBox;
        }

        protected override void OnAwakeUIToolkit(SerializedProperty property, ISaintsAttribute saintsAttribute,
            int index, VisualElement container,
            Action<object> onValueChangedCallback, FieldInfo info, object parent)
        {
            FieldTypeAttribute fieldTypeAttribute = (FieldTypeAttribute)saintsAttribute;
            Type fieldType = ReflectUtils.GetElementType(info.FieldType);
            Type requiredComp = fieldTypeAttribute.CompType ?? fieldType;
            EPick editorPick = fieldTypeAttribute.EditorPick;

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
                    FieldTypeSelectWindow.Open(property.objectReferenceValue, editorPick, fieldType, requiredComp, fieldResult =>
                    {
                        Object result = OnSelectWindowSelected(fieldResult, fieldType);
                        // Debug.Log($"fieldType={fieldType} fieldResult={fieldResult}, result={result}");
                        property.objectReferenceValue = result;
                        property.serializedObject.ApplyModifiedProperties();
                        objectField.SetValueWithoutNotify(result);
                        // objectField.Unbind();
                        // objectField.BindProperty(property);
                        onValueChangedCallback.Invoke(result);

                        // Debug.Log($"property new value = {property.objectReferenceValue}, objectField={objectField.value}");
                    });
                };
            }
        }

        protected override void OnValueChanged(SerializedProperty property, ISaintsAttribute saintsAttribute, int index, VisualElement container,
            FieldInfo info, object parent, Action<object> onValueChangedCallback, object newValue)
        {
            FieldTypeAttribute fieldTypeAttribute = (FieldTypeAttribute)saintsAttribute;
            Type fieldType = ReflectUtils.GetElementType(info.FieldType);
            Type requiredComp = fieldTypeAttribute.CompType ?? fieldType;
            HelpBox helpBox = container.Q<HelpBox>(NameHelpBox(property));

            // ReSharper disable once UseNegatedPatternInIsExpression
            if (!(newValue is Object) && newValue != null)
            {
                helpBox.style.display = DisplayStyle.Flex;
                helpBox.text = $"Value `{newValue}` is not a UnityEngine.Object";
                return;
            }

            Object uObjectValue = (Object) newValue;
            Object result = GetNewValue(uObjectValue, info.FieldType, requiredComp);

            ObjectField objectField = container.Q<ObjectField>(NameObjectField(property));
            if(objectField.value != result)
            {
                objectField.SetValueWithoutNotify(result);
            }

            if (newValue != null && result == null)
            {
                helpBox.style.display = DisplayStyle.Flex;
                helpBox.text = $"{newValue} has no component {fieldType}";
            }
            else
            {
                helpBox.style.display = DisplayStyle.None;
            }
        }


        // protected override void ChangeFieldLabelToUIToolkit(SerializedProperty property,
        //     ISaintsAttribute saintsAttribute, int index, VisualElement container, string labelOrNull,
        //     IReadOnlyList<RichTextDrawer.RichTextChunk> richTextChunks, bool tried, RichTextDrawer richTextDrawer)
        // {
        //     ObjectField target = container.Q<ObjectField>(NameObjectField(property));
        //     target.label = labelOrNull;
        // }

        #endregion

#endif
    }
}
