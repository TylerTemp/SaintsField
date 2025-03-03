#if UNITY_2021_3_OR_NEWER
using System.Reflection;
using SaintsField.Interfaces;
using UnityEditor;
using UnityEngine.UIElements;

namespace SaintsField.Editor.Drawers.ButtonDrawers.BelowButtonDrawer
{
    public partial class BelowButtonAttributeDrawer
    {
        protected override VisualElement CreateBelowUIToolkit(SerializedProperty property,
            ISaintsAttribute saintsAttribute, int index, VisualElement container, FieldInfo info, object parent)
        {
            VisualElement visualElement = new VisualElement
            {
                style =
                {
                    flexGrow = 1,
                },
            };
            visualElement.Add(DrawUIToolkit(property, saintsAttribute, index, info, parent, container));
            visualElement.Add(DrawLabelError(property, index));
            visualElement.Add(DrawExecError(property, index));
            return visualElement;
        }
    }
}
#endif
