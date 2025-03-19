using System;
using System.Linq;
using SaintsField.Editor.Core;
using SaintsField.Editor.Utils;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace SaintsField.Editor.Drawers.CustomPicker.FieldTypeDrawer
{
#if ODIN_INSPECTOR
    [Sirenix.OdinInspector.Editor.DrawerPriority(Sirenix.OdinInspector.Editor.DrawerPriorityLevel.SuperPriority)]
#endif
    [CustomPropertyDrawer(typeof(FieldTypeAttribute), true)]
    public partial class FieldTypeAttributeDrawer: SaintsPropertyDrawer
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
