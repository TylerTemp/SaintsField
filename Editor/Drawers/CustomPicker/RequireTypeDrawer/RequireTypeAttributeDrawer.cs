using System;
using System.Collections.Generic;
using System.Linq;
using SaintsField.Editor.Core;
using SaintsField.Editor.Utils;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace SaintsField.Editor.Drawers.CustomPicker.RequireTypeDrawer
{
    [CustomPropertyDrawer(typeof(RequireTypeAttribute))]
    public partial class RequireTypeAttributeDrawer: SaintsPropertyDrawer
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
        }

    }
}
