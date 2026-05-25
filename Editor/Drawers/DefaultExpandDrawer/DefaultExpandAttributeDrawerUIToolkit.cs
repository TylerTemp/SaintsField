#if UNITY_2021_3_OR_NEWER && !SAINTSFIELD_UI_TOOLKIT_DISABLE
using System.Reflection;
using SaintsField.Interfaces;
using UnityEditor;
using UnityEngine.UIElements;

namespace SaintsField.Editor.Drawers.DefaultExpandDrawer
{
    public partial class DefaultExpandAttributeDrawer
    {
        protected override VisualElement CreateAboveUIToolkit(SerializedProperty property, ISaintsAttribute saintsAttribute, int index,
            VisualElement container, FieldInfo info, object parent)
        {
            property.isExpanded = true;
            return null;
        }
    }
}
#endif
