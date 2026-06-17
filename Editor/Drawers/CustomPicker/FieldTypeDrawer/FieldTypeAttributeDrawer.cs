using System;
using System.Collections.Generic;
using System.Linq;
using SaintsField.Editor.Core;
using SaintsField.Editor.Linq;
using SaintsField.Editor.Utils;
using SaintsField.Editor.Utils.SaintsObjectPickerWindow;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace SaintsField.Editor.Drawers.CustomPicker.FieldTypeDrawer
{
#if ODIN_INSPECTOR
    [Sirenix.OdinInspector.Editor.DrawerPriority(Sirenix.OdinInspector.Editor.DrawerPriorityLevel.WrapperPriority)]
#endif
    [CustomPropertyDrawer(typeof(FieldTypeAttribute), true)]
    public partial class FieldTypeAttributeDrawer: SaintsPropertyDrawer
    {
        private class FieldTypeSelectWindow : SaintsObjectPickerWindowIMGUI
        {
            // private Type[] _expectedTypes;
            private Type _originType;
            private Type _swappedType;
            private Action<Object> _onSelected;
            private EPick _editorPick;
            private IReadOnlyList<GameObject> _rootGameObjects = Array.Empty<GameObject>();

            public static void Open(Object curValue, EPick editorPick, Type originType, Type swappedType,
                IReadOnlyList<GameObject> rootGameObjects, Action<Object> onSelected)
            {
                FieldTypeSelectWindow thisWindow = CreateInstance<FieldTypeSelectWindow>();
                thisWindow.titleContent = new GUIContent($"Select {swappedType.Name}");
                // thisWindow._expectedTypes = expectedTypes;
                thisWindow._originType = originType;
                thisWindow._swappedType = swappedType;
                thisWindow._rootGameObjects = rootGameObjects ?? Array.Empty<GameObject>();
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
                if (!itemObject)
                {
                    return false;
                }

#if UNITY_6000_4_OR_NEWER
                EntityId targetInstanceId = target.GetEntityId();
#else
                int targetInstanceId = target.GetInstanceID();
#endif
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
#if UNITY_6000_4_OR_NEWER
                return itemToOriginTypeValue.GetEntityId() == targetInstanceId;
#else
                return itemToOriginTypeValue.GetInstanceID() == targetInstanceId;
#endif
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

            protected override IEnumerable<ItemInfo> FetchAllSceneObject()
            {
                foreach (GameObject rootGameObject in _rootGameObjects)
                {
                    IEnumerable<(GameObject go, string path)> allGo =
                        Util.GetSubGoWithPath(rootGameObject, null).Prepend((rootGameObject, rootGameObject.name));
                    foreach ((GameObject eachSubGo, string eachSubPath) in allGo)
                    {
                        if (eachSubGo.GetComponent(_swappedType) == null)
                        {
                            continue;
                        }

                        IReadOnlyList<Object> targets = GetTargets(eachSubGo).ToArray();
                        foreach ((Object fieldTarget, int index) in targets.WithIndex())
                        {
                            yield return MakeItemInfo(fieldTarget,
                                $"{eachSubPath}{(targets.Count > 1 ? $"[{index}]" : "")}");
                        }
                    }
                }
            }

            protected override IEnumerable<ItemInfo> FetchAllAssets()
            {
                if (!typeof(Component).IsAssignableFrom(_swappedType))
                {
                    yield break;
                }

                foreach (string prefabGuid in AssetDatabase.FindAssets("t:prefab"))
                {
                    string path = AssetDatabase.GUIDToAssetPath(prefabGuid);
                    GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                    if (prefab == null || prefab.GetComponent(_swappedType) == null)
                    {
                        continue;
                    }

                    foreach (Object fieldTarget in GetTargets(prefab))
                    {
                        yield return MakeItemInfo(fieldTarget, path);
                    }
                }
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

            private IEnumerable<Object> GetTargets(GameObject gameObject)
            {
                if (typeof(Component).IsAssignableFrom(_originType))
                {
                    return gameObject.GetComponents(_originType).Cast<Object>();
                }

                return new Object[] { gameObject };
            }

            private static ItemInfo MakeItemInfo(Object target, string path)
            {
                return new ItemInfo
                {
                    Object = target,
                    Icon = null,
#if UNITY_6000_4_OR_NEWER
                    InstanceID = target.GetEntityId(),
#else
                    InstanceID = target.GetInstanceID(),
#endif
                    Label = target.name,
                    GuiLabel = new GUIContent(target.name),
                    TypeName = target.GetType().Name,
                    Path = path,
                };
            }
        }

        private static Object CanSign(Object target, Type type)
        {
            if(type.IsInstanceOfType(target))
            {
                return target;
            }

            bool expectedIsGameObject = type == typeof(GameObject);
            switch (target)
            {
                case GameObject go:
                    if (expectedIsGameObject)
                    {
                        return go;
                    }

                    return go.GetComponent(type);
                case Component comp:
                    return comp.GetComponent(type);
                default:
                    return null;
            }
        }

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
    }
}
