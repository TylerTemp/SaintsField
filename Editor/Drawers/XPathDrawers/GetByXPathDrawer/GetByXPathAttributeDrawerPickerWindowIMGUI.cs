using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using SaintsField.Editor.Utils.SaintsObjectPickerWindow;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;


namespace SaintsField.Editor.Drawers.XPathDrawers.GetByXPathDrawer
{
    public partial class GetByXPathAttributeDrawer
    {
        private void OpenPicker(SerializedProperty property, FieldInfo info,
            IReadOnlyList<GetByXPathAttribute> getByXPathAttributes, Type expectedType, Type interfaceType,
            Action<object> onValueChanged, object updatedParent)
        {
            (string getValueError, object curValue) = GetCurValue(property, info, updatedParent);

            if (getValueError != "")
            {
                Debug.LogError(getValueError);
                return;
            }

            if (!(curValue is Object) && curValue != null)
            {
                Debug.LogError($"targetValue is not Object: {curValue}");
                return;
            }

            Object curValueObj = (Object)curValue;

            GetXPathValuesResult result = GetXPathValues(getByXPathAttributes
                    .Select(xPathAttribute => new XPathResourceInfo
                    {
                        OptimizationPayload = xPathAttribute.OptimizationPayload,
                        OrXPathInfoList = xPathAttribute.XPathInfoAndList.SelectMany(each => each).ToArray(),
                    })
                    .ToArray(),
                expectedType, interfaceType, property, info, updatedParent);
            if (result.XPathError != "")
            {
                Debug.LogError(result.XPathError);
                return;
            }

            IReadOnlyList<SaintsObjectPickerWindowIMGUI.ItemInfo> assetItems;
            IReadOnlyList<SaintsObjectPickerWindowIMGUI.ItemInfo> sceneItems;
            SplitObjectResults(result.Results.OfType<Object>(), out assetItems, out sceneItems);

            SaintsObjectPickerWindowIMGUI pickerWindow =
                ScriptableObject.CreateInstance<SaintsObjectPickerWindowIMGUI>();
            pickerWindow.ConfigAllowAssets = true;
            pickerWindow.ConfigAllowScene = true;
            pickerWindow.titleContent = new GUIContent(
                $"Select {expectedType}" + (interfaceType == null ? "" : $"({interfaceType})"));
            pickerWindow.FetchAllAssetsCallback = () => assetItems;
            pickerWindow.FetchAllSceneObjectCallback = () => sceneItems;
            pickerWindow.IsEqualCallback = (itemInfo, target) => ReferenceEquals(itemInfo.Object, target);
            pickerWindow.OnSelectCallback = itemInfo =>
            {
                pickerWindow.ErrorMessage = "";
                onValueChanged.Invoke(itemInfo.Object);
            };
            pickerWindow.SetDefaultActive(curValueObj);
            pickerWindow.ShowAuxWindow();
        }

        private static void SplitObjectResults(IEnumerable<Object> results,
            out IReadOnlyList<SaintsObjectPickerWindowIMGUI.ItemInfo> assetItems,
            out IReadOnlyList<SaintsObjectPickerWindowIMGUI.ItemInfo> sceneItems)
        {
            List<SaintsObjectPickerWindowIMGUI.ItemInfo> assets =
                new List<SaintsObjectPickerWindowIMGUI.ItemInfo>();
            List<SaintsObjectPickerWindowIMGUI.ItemInfo> scenes =
                new List<SaintsObjectPickerWindowIMGUI.ItemInfo>();

            foreach (Object objResult in results)
            {
                string assetPath = AssetDatabase.GetAssetPath(objResult);
                SaintsObjectPickerWindowIMGUI.ItemInfo itemInfo = MakePickerItemInfo(objResult, assetPath);
                if (assetPath != "")
                {
                    assets.Add(itemInfo);
                }
                else
                {
                    scenes.Add(itemInfo);
                }
            }

            assetItems = assets;
            sceneItems = scenes;
        }

        private static SaintsObjectPickerWindowIMGUI.ItemInfo MakePickerItemInfo(Object objResult, string assetPath)
        {
            return new SaintsObjectPickerWindowIMGUI.ItemInfo
            {
                Object = objResult,
                Label = objResult.name,
                GuiLabel = new GUIContent(objResult.name),
                TypeName = objResult.GetType().Name,
                Path = assetPath,
#if UNITY_6000_4_OR_NEWER
                InstanceID = objResult.GetEntityId(),
#else
                InstanceID = objResult.GetInstanceID(),
#endif
            };
        }

    }
}
