using System;
using Spine;

namespace SaintsField.Editor.Drawers.Spine.SpineSlotPickerDrawer
{
    public static class SpineSlotUtils
    {
        public readonly struct SlotInfo: IEquatable<SlotInfo>
        {
            public readonly SlotData SlotData;
            public readonly bool Disabled;
            public readonly string Label;

            public SlotInfo(SlotData slotData, bool disabled, string label)
            {
                SlotData = slotData;
                Disabled = disabled;
                Label = label;
            }

            public bool Equals(SlotInfo other)
            {
                return Equals(SlotData, other.SlotData);
            }

            public override bool Equals(object obj)
            {
                return obj is SlotInfo other && Equals(other);
            }

            public override int GetHashCode()
            {
                return SlotData != null ? SlotData.GetHashCode() : 0;
            }
        }

        public const string IconPath = "Spine/icon-slot.png";
    }
}
