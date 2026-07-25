#if UNITY_2021_3_OR_NEWER
using SaintsField.Editor.Utils;
using UnityEngine.UIElements;

namespace SaintsField.Editor.Drawers.LeftToggleDrawer
{
#if UNITY_6000_0_OR_NEWER
    [UxmlElement]
#endif
    public partial class LeftToggleField: Toggle
    {
#if !UNITY_6000_0_OR_NEWER
        public new class UxmlTraits : VisualElement.UxmlTraits { }
        public new class UxmlFactory : UxmlFactory<LeftToggleField, UxmlTraits> { }
#endif
        public LeftToggleField()
            : this(null)
        {
        }

        public LeftToggleField(string label)
            : base(label)
        {
            styleSheets.Add(Util.LoadResource<StyleSheet>("UIToolkit/LeftToggle.uss"));
        }
    }
}
#endif
