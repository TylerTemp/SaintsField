using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using SaintsField.Editor.Utils;
using SaintsField.Editor.Utils.SaintsObjectPickerWindow;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;


namespace SaintsField.Editor.Drawers.XPathDrawers.GetByXPathDrawer
{
    public partial class GetByXPathAttributeDrawer
    {
        private class GetByPickerWindow : SaintsObjectPickerWindowIMGUI
        {
            private Action<Object> _onSelected;
            private EPick _editorPick;

            private IReadOnlyList<Object> _assetObjects;
            private IReadOnlyList<Object> _sceneObjects;

            public static void Open(Object curValue, EPick editorPick, IReadOnlyList<Object> assetObjects, IReadOnlyList<Object> sceneObjects, Action<Object> onSelected)
            {
                GetByPickerWindow thisWindow = CreateInstance<GetByPickerWindow>();
                thisWindow.titleContent = new GUIContent("Select");
                // thisWindow._expectedTypes = expectedTypes;
                thisWindow._assetObjects = assetObjects;
                thisWindow._sceneObjects = sceneObjects;
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

            protected override IEnumerable<ItemInfo> FetchAllAssets()
            {
                // HierarchyProperty property = new HierarchyProperty(HierarchyType.Assets, false);
                return _assetObjects.Select(each => new ItemInfo
                {
                    Object = each,
                    Label = each.name,
                    // Icon = property.icon,
                    InstanceID = each.GetInstanceID(),
                    GuiLabel = new GUIContent(each.name),
                });
            }

            protected override IEnumerable<ItemInfo> FetchAllSceneObject()
            {
                // HierarchyProperty property = new HierarchyProperty(HierarchyType.GameObjects, false);
                return _sceneObjects.Select(each => new ItemInfo
                {
                    Object = each,
                    Label = each.name,
                    // Icon = property.icon,
                    InstanceID = each.GetInstanceID(),
                    GuiLabel = new GUIContent(each.name),
                });
            }

            protected override string Error => "";

            protected override bool IsEqual(ItemInfo itemInfo, Object target)
            {
                return ReferenceEquals(itemInfo.Object, target);
            }

            protected override void OnSelect(ItemInfo itemInfo)
            {
                _onSelected(itemInfo.Object);
            }

            protected override bool FetchAllSceneObjectFilter(ItemInfo itemInfo) => true;

            protected override bool FetchAllAssetsFilter(ItemInfo itemInfo) => true;
        }

        private static void OpenPicker(SerializedProperty property, FieldInfo info, IReadOnlyList<GetByXPathAttribute> getByXPathAttributes, Type expectedType, Type interfaceType, Action<object> onValueChanged, object updatedParent)
        {
            (string getValueError, int _, object curValue) = Util.GetValue(property, info, updatedParent);

            // ReSharper disable once UseNegatedPatternInIsExpression
            if (!(curValue is Object) && curValue != null)
            {
                Debug.LogError($"targetValue is not Object: {curValue}");
                return;
            }

            Object curValueObj = (Object)curValue;

            if (getValueError != "")
            {
                Debug.LogError(getValueError);
                return;
            }

            GetXPathValuesResult r = CalcXPathValues(getByXPathAttributes
                    .Select(xPathAttribute => new XPathResourceInfo
                    {
                        OptimizationPayload = xPathAttribute.OptimizationPayload,
                        OrXPathInfoList = xPathAttribute.XPathInfoAndList.SelectMany(each => each).ToArray(),
                    })
                    .ToArray(),
                expectedType, interfaceType, property, info, updatedParent);
            if (r.XPathError != "")
            {
                Debug.LogError(r.XPathError);
                return;
            }

            Object[] objResults = r.Results.OfType<Object>().ToArray();
            List<Object> assetObj = new List<Object>();
            List<Object> sceneObj = new List<Object>();
            foreach (Object objResult in objResults)
            {
                if (AssetDatabase.GetAssetPath(objResult) != "")
                {
                    assetObj.Add(objResult);
                }
                else
                {
                    sceneObj.Add(objResult);
                }
            }

            GetByPickerWindow.Open(curValueObj, EPick.Assets | EPick.Scene, assetObj, sceneObj, onValueChanged.Invoke);
        }

    }
}
