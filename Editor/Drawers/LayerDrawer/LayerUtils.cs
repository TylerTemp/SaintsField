using System;
using System.Collections.Generic;
using UnityEditorInternal;

namespace SaintsField.Editor.Drawers.LayerDrawer
{
    public static class LayerUtils
    {
        public readonly struct LayerInfo: IEquatable<LayerInfo>
        {
            public readonly string Name;
            public readonly int Value;

            public LayerInfo(string name, int value)
            {
                Name = name;
                Value = value;
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
    }
}
