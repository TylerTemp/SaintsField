using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using SaintsField.Editor.Core;
using SaintsField.Editor.Utils;
using SaintsField.Editor.Utils.SaintsObjectPickerWindow;
using SaintsField.Utils;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace SaintsField.Editor.Drawers.CustomPicker.RequireTypeDrawer
{
#if ODIN_INSPECTOR
    [Sirenix.OdinInspector.Editor.DrawerPriority(Sirenix.OdinInspector.Editor.DrawerPriorityLevel.WrapperPriority)]
#endif
    [CustomPropertyDrawer(typeof(RequireTypeAttribute), true)]
    public partial class RequireTypeAttributeDrawer: SaintsPropertyDrawer
    {
        private class FieldInterfaceSelectWindow : SaintsObjectPickerWindowIMGUI
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
                _editorPick.HasFlagFast(EPick.Scene);

            protected override bool AllowAssets =>
                // Debug.Log(_editorPick);
                _editorPick.HasFlagFast(EPick.Assets);

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
                return checkTypes.All(requiredType => itemType.IsAssignableFrom(requiredType));
            }
        }

        private static IReadOnlyList<string> GetMissingTypeNames(Object curValue, IEnumerable<Type> requiredTypes)
        {
            return requiredTypes.Where(eachType => Util.GetTypeFromObj(curValue, eachType) == null)
                .Select(eachType => eachType.Name)
                .ToArray();
        }

        protected virtual void OpenSelectorWindowIMGUI(SerializedProperty property, RequireTypeAttribute requireTypeAttribute, FieldInfo info, Action<object> onChangeCallback, object parent)
        {
            Type fieldType = SerializedUtils.IsArrayOrDirectlyInsideArray(property)? ReflectUtils.GetElementType(info.FieldType): info.FieldType;
            FieldInterfaceSelectWindow.Open(property.objectReferenceValue, requireTypeAttribute.EditorPick,
                fieldType, requireTypeAttribute.RequiredTypes, fieldResult =>
                {
                    // Type fieldType = ReflectUtils.GetElementType(info.FieldType);
                    Object result = OnSelectWindowSelected(fieldResult, fieldType, requireTypeAttribute.RequiredTypes);
                    if(requireTypeAttribute.FreeSign && RuntimeUtil.IsNull(result))
                    {
                        result = NoInterfaceGetResult(fieldResult, fieldType);
                    }
                    property.objectReferenceValue = result;
                    property.serializedObject.ApplyModifiedProperties();
                    // onGUIPayload.SetValue(result);
                    onChangeCallback(result);
                });
        }

        private static Object OnSelectWindowSelected(Object fieldResult, Type fieldType, IReadOnlyList<Type> requiredTypes)
        {
            if (RuntimeUtil.IsNull(fieldResult))
            {
                return null;
            }

            List<Type> interfaceTypes = new List<Type>();
            List<Type> normalTypes = new List<Type>
            {
                fieldType,
            };

            foreach (Type requiredType in requiredTypes)
            {
                if (requiredType.IsInterface)
                {
                    interfaceTypes.Add(requiredType);
                }
                else
                {
                    normalTypes.Add(requiredType);
                }
            }

            if (interfaceTypes.Count == 0)
            {
                return NoInterfaceGetResult(fieldResult, fieldType);
            }

            List<Object> toCheckTargets = new List<Object>();
            switch (fieldResult)
            {
                case GameObject go:
                    toCheckTargets.AddRange(go.GetComponents<Component>());
                    break;
                case Component comp:
                    toCheckTargets.AddRange(comp.GetComponents<Component>());
                    break;
                case ScriptableObject so:
                    toCheckTargets.Add(so);
                    break;
            }

            IReadOnlyList<Object> processingTargets = toCheckTargets;

            if (interfaceTypes.Count > 0)
            {
                processingTargets = GetQualifiedInterfaces(processingTargets, interfaceTypes).ToArray();
            }

            processingTargets = GetQualifiedComponent(processingTargets, normalTypes).ToArray();

            foreach (Object processedResult in processingTargets)
            {
#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_REQUIRE_TYPE
                Debug.Log($"checking {processedResult} for {fieldType}");
#endif
                switch (processedResult)
                {
                    case null:
                        // property.objectReferenceValue = null;
                        continue;
                    case GameObject go:
                        // ReSharper disable once RedundantCast
                    {
                        Object r = fieldType == typeof(GameObject) ? (Object)go : go.GetComponent(fieldType);
                        if (r != null)
                        {
#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_REQUIRE_TYPE
                            Debug.Log($"Return {r} for {fieldType}");
#endif
                            return r;
                        }
                    }
                        break;
                        // Debug.Log($"isGo={fieldType == typeof(GameObject)},  fieldResult={fieldResult.GetType()} result={result.GetType()}");
                    case Component comp:
                    {
                        if (fieldType.IsAssignableFrom(comp.GetType()))
                        {
#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_REQUIRE_TYPE
                            Debug.Log($"Return {comp} for {fieldType}");
#endif
                            return comp;
                        }
                    }
                        break;
                    case ScriptableObject so:
                    {
                        if (fieldType.IsAssignableFrom(so.GetType()))
                        {
                            return so;
                        }
                    }
                        break;
                }
            }

            return null;
        }

        private static Object NoInterfaceGetResult(Object fieldResult, Type fieldType)
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
    }
}
