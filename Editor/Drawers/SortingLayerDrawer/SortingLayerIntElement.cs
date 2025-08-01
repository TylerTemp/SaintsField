#if UNITY_2021_3_OR_NEWER
using SaintsField.Editor.UIToolkitElements;
using UnityEngine;

namespace SaintsField.Editor.Drawers.SortingLayerDrawer
{
    public class SortingLayerIntElement: IntDropdownElement
    {
        public override void SetValueWithoutNotify(int newValue)
        {
            CachedValue = newValue;

            foreach (SortingLayer layer in SortingLayer.layers)
            {
                if (layer.value == newValue)
                {
                    Label.text = $"{layer.name} <color=#808080>({layer.id})</color>";
                    return;
                }
            }

            Label.text = $"<color=red>?</color> ({newValue})";
        }
    }
}
#endif
