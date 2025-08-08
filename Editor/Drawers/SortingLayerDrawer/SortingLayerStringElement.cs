#if UNITY_2021_3_OR_NEWER
using SaintsField.Editor.UIToolkitElements;
using UnityEngine;

namespace SaintsField.Editor.Drawers.SortingLayerDrawer
{
    public class SortingLayerStringElement: StringDropdownElement
    {
        public override void SetValueWithoutNotify(string newValue)
        {
            CachedValue = newValue;

            foreach (SortingLayer layer in SortingLayer.layers)
            {
                // ReSharper disable once InvertIf
                if (layer.name == newValue)
                {
                    Label.text = $"{layer.name} <color=#808080>({layer.id})</color>";
                    return;
                }
            }

            Label.text = $"<color=red>?</color> {(string.IsNullOrEmpty(newValue)? "": $"({newValue})")}";
        }
    }
}
#endif
