#if UNITY_2021_3_OR_NEWER
using System;
using System.Reflection;
using SaintsField.Editor.Drawers.TreeDropdownDrawer;
using UnityEditor;
using UnityEngine.Events;

namespace SaintsField.Editor.Drawers.EnumFlagsDrawers.EnumToggleButtonsDrawer
{
    public class ButtonsULongElement: ButtonsGenElement<ulong>
    {
        public ButtonsULongElement(EnumMetaInfo metaInfo, SerializedProperty property, MemberInfo info, object container, Action<object> setValue) : base(metaInfo, property, info, container, setValue)
        {
        }
    }

    public class ButtonsULongField : ButtonsGenField<ulong>
    {
        public ButtonsULongField(string label, ButtonsGenElement<ulong> visualInput, UnityEvent<bool> onExpandChanged) : base(label, visualInput, onExpandChanged)
        {
        }
    }
}
#endif
