using System;
using System.Linq;
using System.Reflection;
using SaintsField.Editor.Utils;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace SaintsField.Editor.Drawers.SaintsInterfacePropertyDrawer
{
#if ODIN_INSPECTOR
    [Sirenix.OdinInspector.Editor.DrawerPriority(Sirenix.OdinInspector.Editor.DrawerPriorityLevel.SuperPriority)]
#endif
    [CustomPropertyDrawer(typeof(SaintsObjInterface<>), true)]
    [CustomPropertyDrawer(typeof(SaintsInterface<,>), true)]
    public partial class SaintsInterfaceDrawer: PropertyDrawer
    {
        private class FieldInterfaceSelectWindow : ObjectSelectWindow
        {
            private Type _fieldType;
            private Type _interfaceType;
            private Action<Object> _onSelected;

            public static void Open(Object curValue, Type originType, Type interfaceType, Action<Object> onSelected)
            {
                FieldInterfaceSelectWindow thisWindow = CreateInstance<FieldInterfaceSelectWindow>();
                thisWindow.titleContent = new GUIContent($"Select {originType.Name} with {interfaceType}");
                thisWindow._fieldType = originType;
                thisWindow._interfaceType = interfaceType;
                thisWindow._onSelected = onSelected;
                thisWindow.SetDefaultActive(curValue);
                thisWindow.ShowAuxWindow();
            }

            protected override bool AllowScene => true;

            protected override bool AllowAssets => true;

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

                Object itemToOriginTypeValue = Util.GetTypeFromObj(itemObject, _fieldType);

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
                return GetSerializedObject(itemInfo.Object, _fieldType, _interfaceType).isMatch;
            }
        }

        private static (bool isMatch, Object result) GetSerializedObject(Object selectedObject, Type fieldType,
            Type interfaceType)
        {
            bool fieldTypeIsComponent = typeof(Component).IsAssignableFrom(fieldType);

            switch (selectedObject)
            {
                case GameObject go:
                {
                    // Debug.Log($"go={go}, fieldType={_fieldType}, interfaceType={_interfaceType}");
                    if (fieldTypeIsComponent)
                    {
                        Component compResult = go.GetComponents(fieldType)
                            .FirstOrDefault(interfaceType.IsInstanceOfType);
                        return compResult == null
                            ? (false, null)
                            : (true, compResult);
                    }

                    if (!fieldType.IsInstanceOfType(go))
                    {
                        return (false, null);
                    }

                    Component result = go.GetComponents(typeof(Component))
                        .FirstOrDefault(interfaceType.IsInstanceOfType);
                    return result == null
                        ? (false, null)
                        : (true, result);
                }
                case Component comp:
                {
                    if (fieldTypeIsComponent)
                    {
                        Component compResult = comp.GetComponents(fieldType)
                            .FirstOrDefault(interfaceType.IsInstanceOfType);
                        return compResult == null
                            ? (false, null)
                            : (true, compResult);
                    }

                    if (!fieldType.IsInstanceOfType(comp))
                    {
                        return (false, null);
                    }

                    Component result = comp.GetComponents(typeof(Component))
                        .FirstOrDefault(interfaceType.IsInstanceOfType);
                    return result == null
                        ? (false, null)
                        : (true, result);
                }
                case ScriptableObject so:
                    // Debug.Log(fieldType);
                    return (fieldType == typeof(ScriptableObject) || fieldType.IsSubclassOf(typeof(ScriptableObject)) || typeof(ScriptableObject).IsSubclassOf(fieldType))
                           && interfaceType.IsInstanceOfType(so)
                           ? (true, so)
                           : (false, null);
                default:
                    return new[]
                    {
                        fieldType,
                        interfaceType,
                    }.All(requiredType => requiredType.IsInstanceOfType(selectedObject))
                        ? (true, selectedObject)
                        : (false, null);

                    // Type itemType = itemInfo.Object.GetType();
                    // return checkTypes.All(requiredType => itemType.IsInstanceOfType(requiredType));

            }
        }

        private static (string error, IWrapProp propInfo, int index, object parent) GetSerName(SerializedProperty property, FieldInfo fieldInfo)
        {
            (SerializedUtils.FieldOrProp _, object parent) = SerializedUtils.GetFieldInfoAndDirectParent(property);

            (string error, int arrayIndex, object value) = Util.GetValue(property, fieldInfo, parent);

            if (error != "")
            {
                return (error, null, -1, null);
            }

            IWrapProp curValue = (IWrapProp) value;
            return ("", curValue, arrayIndex, parent);
        }

    }
}
