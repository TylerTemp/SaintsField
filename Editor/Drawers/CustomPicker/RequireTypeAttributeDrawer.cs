#if UNITY_2021_3_OR_NEWER
// using UnityEditor.UIElements;
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
    [CustomPropertyDrawer(typeof(RequireTypeAttribute))]
    public class RequireTypeAttributeDrawer: SaintsPropertyDrawer
    {
        private class FieldInterfaceSelectWindow : ObjectSelectWindow
        {
            private Type _fieldType;
            private IReadOnlyList<Type> _requiredTypes;
            // private Type _interfaceType;
            private Action<Object> _onSelected;
            private EPick _editorPick;

            public static void Open(Object curValue, EPick editorPick, Type originType, IEnumerable<Type> requiredTypes, Action<Object> onSelected)
            {
                FieldInterfaceSelectWindow thisWindow = CreateInstance<FieldInterfaceSelectWindow>();
                thisWindow._requiredTypes = requiredTypes.ToArray();
                thisWindow.titleContent = new GUIContent($"Select {originType.Name} with {string.Join(", ", thisWindow._requiredTypes.Select(t => t.Name))}");
                // thisWindow._expectedTypes = expectedTypes;
                thisWindow._fieldType = originType;
                // thisWindow._interfaceType = swappedType;
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
                Debug.Assert(itemObject, itemObject);

                int targetInstanceId = target.GetInstanceID();
                if (itemInfo.InstanceID == targetInstanceId)
                {
                    return true;
                }

                // target=originalType(Component, GameObject) e.g. Sprite, SpriteRenderer
                // itemObject: Scene.GameObject, Assets.?

                // lets get the originalValue from the checking target
                // bool originalIsGameObject = _fieldType == typeof(GameObject);
                Object itemToOriginTypeValue = Util.GetTypeFromObj(itemObject, _fieldType);
                // switch (itemObject)
                // {
                //     case GameObject go:
                //         itemToOriginTypeValue = originalIsGameObject ? (Object)go : go.GetComponent(_fieldType);
                //         break;
                //     case Component compo:
                //         itemToOriginTypeValue = originalIsGameObject ? (Object)compo.gameObject : compo.GetComponent(_fieldType);
                //         break;
                //     default:
                //         return false;
                // }

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

                IEnumerable<Type> checkTypes = typeof(Component).IsAssignableFrom(_fieldType)
                    ? _requiredTypes.Append(_fieldType)
                    : _requiredTypes;
                if (itemInfo.Object is GameObject go)
                {
                    return checkTypes.All(requiredType => go.GetComponent(requiredType) != null);
                }

                Type itemType = itemInfo.Object.GetType();
                return checkTypes.All(requiredType => itemType.IsInstanceOfType(requiredType));
            }
        }

        private static IReadOnlyList<string> GetMissingTypeNames(Object curValue, IEnumerable<Type> requiredTypes)
        {
            return requiredTypes.Where(eachType => Util.GetTypeFromObj(curValue, eachType) == null)
                .Select(eachType => eachType.Name)
                .ToArray();
            // switch (curValue)
            // {
            //     case GameObject go:
            //         return requiredTypes
            //             .Where(requiredType => go.GetComponent(requiredType) == null)
            //             .Select(requiredType => requiredType.Name)
            //             .ToArray();
            //     case Component comp:
            //         return requiredTypes
            //             .Where(requiredType => comp.GetComponent(requiredType) == null)
            //             .Select(requiredType => requiredType.Name)
            //             .ToArray();
            //     default:
            //     {
            //         Type curType = curValue.GetType();
            //         return requiredTypes
            //             .Where(requiredType => !curType.IsInstanceOfType(requiredType))
            //             .Select(requiredType => requiredType.Name)
            //             .ToArray();
            //     }
            // }
        }

        #region IMGUI

        // ReSharper disable once InconsistentNaming
        protected string _error { private get; set; } = "";
        protected bool ImGuiFirstChecked { get; private set; }

        private Object _previousValue;

        protected override float DrawPreLabelImGui(Rect position, SerializedProperty property,
            ISaintsAttribute saintsAttribute, FieldInfo info, object parent)
        {
            _previousValue = property.objectReferenceValue;
            return base.DrawPreLabelImGui(position, property, saintsAttribute, info, parent);
        }

        protected override float GetPostFieldWidth(Rect position, SerializedProperty property, GUIContent label,
            ISaintsAttribute saintsAttribute, int index, OnGUIPayload onGuiPayload, FieldInfo info, object parent)
        {
            RequireTypeAttribute requireTypeAttribute = (RequireTypeAttribute)saintsAttribute;
            return requireTypeAttribute.CustomPicker ? 20 : 0;
        }

        private GUIStyle _imGuiButtonStyle;

        protected override bool DrawPostFieldImGui(Rect position, SerializedProperty property, GUIContent label,
            ISaintsAttribute saintsAttribute, int index, OnGUIPayload onGUIPayload, FieldInfo info, object parent)
        {
            RequireTypeAttribute requireTypeAttribute = (RequireTypeAttribute)saintsAttribute;
            IReadOnlyList<Type> requiredTypes = requireTypeAttribute.RequiredTypes;

            bool customPicker = requireTypeAttribute.CustomPicker;
            if(customPicker)
            {
                // ReSharper disable once ConvertIfStatementToNullCoalescingAssignment
                if (_imGuiButtonStyle == null)
                {
                    _imGuiButtonStyle = new GUIStyle(GUI.skin.button)
                    {
                        // margin = new RectOffset(0, 0, 0, 0),
                        padding = new RectOffset(0, 0, 0, 0),
                    };
                }

                if (GUI.Button(position, "●", _imGuiButtonStyle))
                {
                    OpenSelectorWindow(property, requireTypeAttribute, info, onGUIPayload.SetValue, parent);
                }
            }

            if (!ImGuiFirstChecked || onGUIPayload.changed)
            {
                // Debug.Log($"onGUIPayload.changed={onGUIPayload.changed}/_imGuiFirstChecked={_imGuiFirstChecked}");
                _error = "";
                // bool isFirstCheck = !_imGuiFirstChecked;
                // Debug.Log($"_imGuiFirstChecked={_imGuiFirstChecked}/freeSign={fieldInterfaceAttribute.FreeSign}");


                Object curValue = GetCurFieldValue(property, requireTypeAttribute);
                if (curValue is null)
                {
                    return customPicker;
                }

                IReadOnlyList<string> missingTypeNames = GetMissingTypeNames(curValue, requiredTypes);

                // Debug.Log($"missingTypeNames={string.Join(",", missingTypeNames)}, _imGuiFirstChecked={_imGuiFirstChecked}");

                if (missingTypeNames.Count > 0)  // if has errors
                {
                    string errorMessage = $"{curValue} has no component{(missingTypeNames.Count > 1? "s": "")} {string.Join(", ", missingTypeNames)}.";
                    // freeSign will always give error information
                    // but if you never passed the first check, then sign as you want and it'll always just show error
                    if (!ImGuiFirstChecked || requireTypeAttribute.FreeSign)
                    {
                        // Debug.Log($"isFirstCheck={isFirstCheck}/freeSign={fieldInterfaceAttribute.FreeSign}");
                        _error = errorMessage;
                    }
                    else  // it's not freeSign, and you've already got a correct answer. So revert to the old value.
                    {
                        // property.objectReferenceValue = _previousValue;
                        RestorePreviousValue(property, info, parent);
                        onGUIPayload.SetValue(GetPreviousValue());
                        Debug.LogWarning($"{errorMessage} Change reverted to {(_previousValue==null? "null": _previousValue.ToString())}.");
                    }
                }
                else
                {
                    ImGuiFirstChecked = true;
                }
            }

            return customPicker;
        }

        protected virtual Object GetCurFieldValue(SerializedProperty property, RequireTypeAttribute _) => property.objectReferenceValue;

        protected virtual void OpenSelectorWindow(SerializedProperty property, RequireTypeAttribute requireTypeAttribute, FieldInfo info, Action<object> onChangeCallback, object parent)
        {
            FieldInterfaceSelectWindow.Open(property.objectReferenceValue, requireTypeAttribute.EditorPick,
                ReflectUtils.GetElementType(info.FieldType), requireTypeAttribute.RequiredTypes, fieldResult =>
            {
                Object result = OnSelectWindowSelected(fieldResult, ReflectUtils.GetElementType(info.FieldType));
                property.objectReferenceValue = result;
                property.serializedObject.ApplyModifiedProperties();
                // onGUIPayload.SetValue(result);
                onChangeCallback(result);
            });
        }

        protected virtual void RestorePreviousValue(SerializedProperty property, FieldInfo info, object parent)
        {
            property.objectReferenceValue = _previousValue;
            ReflectUtils.SetValue(property.propertyPath, property.serializedObject.targetObject, info, parent, _previousValue);
        }

        protected virtual object GetPreviousValue() => _previousValue;

        private static Object OnSelectWindowSelected(Object fieldResult, Type fieldType)
        {
            Object result = null;
            switch (fieldResult)
            {
                case null:
                    // property.objectReferenceValue = null;
                    break;
                case GameObject go:
                    // ReSharper disable once RedundantCast
                    result = fieldType == typeof(GameObject) ? (Object)go : go.GetComponent(fieldType);
                    // Debug.Log($"isGo={fieldType == typeof(GameObject)},  fieldResult={fieldResult.GetType()} result={result.GetType()}");
                    break;
                case Component comp:
                    result = fieldType == typeof(GameObject)
                        // ReSharper disable once RedundantCast
                        ? (Object)comp.gameObject
                        : comp.GetComponent(fieldType);
                    break;
            }

            return result;
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

#if UNITY_2021_3_OR_NEWER

        #region UIToolkit
        protected static string NameHelpBox(SerializedProperty property) => $"{property.propertyPath}__RequireType_HelpBox";
        protected static string NameSelectorButton(SerializedProperty property) => $"{property.propertyPath}__RequireType_SelectorButton";

        protected class Payload
        {
            public bool hasCorrectValue;
            public Object correctValue;
        }

        protected override VisualElement CreatePostFieldUIToolkit(SerializedProperty property,
            ISaintsAttribute saintsAttribute, int index, VisualElement container, FieldInfo info, object parent)
        {
            RequireTypeAttribute requireTypeAttribute = (RequireTypeAttribute)saintsAttribute;
            bool customPicker = requireTypeAttribute.CustomPicker;

            if (!customPicker)
            {
                return null;
            }

            Button button = new Button
            {
                text = "●",
                style =
                {
                    // position = Position.Absolute,
                    // right = 0,
                    width = 18,
                    marginLeft = 0,
                    marginRight = 0,
                },
                name = NameSelectorButton(property),
            };

            button.AddToClassList(ClassAllowDisable);
            return button;
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
                userData = new Payload
                {
                    hasCorrectValue = false,
                    correctValue = null,
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
            RequireTypeAttribute requireTypeAttribute = (RequireTypeAttribute)saintsAttribute;
            IReadOnlyList<Type> requiredTypes = requireTypeAttribute.RequiredTypes;

            if(requireTypeAttribute.CustomPicker)
            {
                container.Q<Button>(NameSelectorButton(property)).clicked += () =>
                {
                    OpenSelectorWindow(property, requireTypeAttribute, info, onValueChangedCallback, parent);
                };
            }

            HelpBox helpBox = container.Q<HelpBox>(NameHelpBox(property));
            Payload payload = (Payload)helpBox.userData;
            Object curValue = property.objectReferenceValue;
            IReadOnlyList<string> missingTypeNames = curValue == null
                ? Array.Empty<string>()
                : GetMissingTypeNames(curValue, requiredTypes);
            if (missingTypeNames.Count > 0)
            {
                helpBox.text = $"{curValue} has no component{(missingTypeNames.Count > 1? "s": "")} {string.Join(", ", missingTypeNames)}";
                helpBox.style.display = DisplayStyle.Flex;
            }
            else
            {
                payload.hasCorrectValue = true;
                payload.correctValue = curValue;
            }


        }

        protected override void OnValueChanged(SerializedProperty property, ISaintsAttribute saintsAttribute, int index,
            VisualElement container,
            FieldInfo info, object parent, Action<object> onValueChangedCallback, object newValue)
        {
            Object newObjectValue = (Object)newValue;
            RequireTypeAttribute requireTypeAttribute = (RequireTypeAttribute)saintsAttribute;
            IReadOnlyList<Type> requiredTypes = requireTypeAttribute.RequiredTypes;

            HelpBox helpBox = container.Q<HelpBox>(NameHelpBox(property));
            Payload payload = (Payload)helpBox.userData;

            IReadOnlyList<string> missingTypeNames = newObjectValue == null
                ? Array.Empty<string>()
                : GetMissingTypeNames(newObjectValue, requiredTypes);

            if (missingTypeNames.Count == 0)
            {
                helpBox.style.display = DisplayStyle.None;
                payload.hasCorrectValue = true;
                payload.correctValue = newObjectValue;
            }
            else
            {
                string errorMessage = $"{newObjectValue} has no component{(missingTypeNames.Count > 1? "s": "")} {string.Join(", ", missingTypeNames)}.";
                if(requireTypeAttribute.FreeSign || !payload.hasCorrectValue)
                {
                    helpBox.text = errorMessage;
                    helpBox.style.display = DisplayStyle.Flex;
                }
                else
                {
                    Debug.Assert(!requireTypeAttribute.FreeSign && payload.hasCorrectValue,
                          "Code should not be here. This is a BUG.");
                    property.objectReferenceValue = payload.correctValue;
                    property.serializedObject.ApplyModifiedProperties();
                    Debug.LogWarning($"{errorMessage} Change reverted to {(payload.correctValue == null ? "null" : payload.correctValue.ToString())}.");
                    // careful for infinite loop!
                    onValueChangedCallback(payload.correctValue);
                }
            }

        }

        #endregion

#endif
    }
}
