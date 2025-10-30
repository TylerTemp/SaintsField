using System;
using System.Collections.Generic;
using System.Reflection;
using SaintsField.Editor.Drawers.AdvancedDropdownDrawer;
using SaintsField.Editor.Drawers.TreeDropdownDrawer;
using SaintsField.Editor.Utils;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.UIElements;

namespace SaintsField.Editor.Drawers.LayerDrawer
{
    public static class LayerUtils
    {
        public readonly struct LayerInfo: IEquatable<LayerInfo>
        {
            public readonly string Name;
            public readonly int Value;
            public readonly int Mask;

            public LayerInfo(string name, int value)
            {
                Name = name;
                Value = value;
                Mask = 1 << value;
            }

            public bool Equals(LayerInfo other)
            {
                return Value == other.Value;
            }

            public override bool Equals(object obj)
            {
                return obj is LayerInfo other && Equals(other);
            }

            public override int GetHashCode()
            {
                return Value;
            }
        }

        // Copy from: LayerField.GetAllLayers
        public static IReadOnlyList<LayerInfo> GetAllLayers()
        {
            List<LayerInfo> resultList = new List<LayerInfo>();
            for (int layer = 0; layer < 32; ++layer)
            {
                string layerName = InternalEditorUtility.GetLayerName(layer);
                if (layerName.Length != 0)
                {
                    resultList.Add(new LayerInfo(layerName, layer));
                }
            }
            return resultList;
        }

        public static string LayerInfoLabelUIToolkit(LayerInfo layerInfo) => $"{layerInfo.Name} <color=#808080>({layerInfo.Value})</color>";

        public static void MakeDropdown(bool allowEmpty, int curMask, VisualElement root, Action<LayerInfo> onValueChangedCallback)
        {
            AdvancedDropdownList<LayerInfo> dropdown = new AdvancedDropdownList<LayerInfo>();
            if (allowEmpty)
            {
                dropdown.Add("[Empty String]", new LayerInfo(string.Empty, -1));
                dropdown.AddSeparator();
            }

            bool hasSelected = false;
            LayerInfo selected = new LayerInfo("", -9999);
            foreach (LayerInfo layerInfo in GetAllLayers())
            {
                dropdown.Add(LayerInfoLabelUIToolkit(layerInfo), layerInfo);

                // ReSharper disable once InvertIf
                if (layerInfo.Mask == curMask)
                {
                    hasSelected = true;
                    selected = layerInfo;
                }
            }

            dropdown.AddSeparator();
            dropdown.Add("Edit Layers...", new LayerInfo("", -2), false, "d_editicon.sml");

            AdvancedDropdownMetaInfo metaInfo = new AdvancedDropdownMetaInfo
            {
                CurValues = hasSelected ? new object[] { selected } : Array.Empty<object>(),
                DropdownListValue = dropdown,
                SelectStacks = Array.Empty<AdvancedDropdownAttributeDrawer.SelectStack>(),
            };

            (Rect worldBound, float maxHeight) = SaintsAdvancedDropdownUIToolkit.GetProperPos(root.worldBound);

            SaintsTreeDropdownUIToolkit sa = new SaintsTreeDropdownUIToolkit(
                metaInfo,
                root.worldBound.width,
                maxHeight,
                false,
                (curItem, _) =>
                {
                    LayerInfo layerInfo = (LayerInfo)curItem;
                    if (layerInfo.Value == -2)
                    {
                        Selection.activeObject = AssetDatabase.LoadMainAssetAtPath("ProjectSettings/TagManager.asset");
                    }
                    else
                    {
                        onValueChangedCallback(layerInfo);
                    }

                    return null;
                }
            );

            // DebugPopupExample.SaintsAdvancedDropdownUIToolkit = sa;
            // var editorWindow = EditorWindow.GetWindow<DebugPopupExample>();
            // editorWindow.Show();

            UnityEditor.PopupWindow.Show(worldBound, sa);
        }
    }
}
