#if UNITY_2021_3_OR_NEWER
using System;
using System.Reflection;
using SaintsField.Editor.Drawers.TreeDropdownDrawer;
using SaintsField.Editor.Utils;
using UnityEditor;
using UnityEngine.Events;

namespace SaintsField.Editor.Drawers.EnumFlagsDrawers.EnumToggleButtonsDrawer
{
    public class ButtonsLongElement: ButtonsGenElement<long>
    {
        public ButtonsLongElement(EnumMetaInfo metaInfo, SerializedProperty property, MemberInfo info, object container, Action<object> setValue, IRichTextTagProvider richTextProvider) : base(metaInfo, property, info, container, setValue, richTextProvider)
        {
        }
    }

    public class ButtonsLongField : ButtonsGenField<ulong>
    {
        public ButtonsLongField(string label, ExpandableButtonsElement visualInput) : base(label, visualInput)
        {
        }
    }
}
#endif
