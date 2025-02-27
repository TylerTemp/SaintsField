#if UNITY_2021_3_OR_NEWER && !SAINTSFIELD_UI_TOOLKIT_DISABLE
using SaintsField.Editor.Core;
using SaintsField.Editor.Utils;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace SaintsField.Editor.Playa.Renderer
{
    public partial class SerializedFieldRenderer
    {
        private PropertyField _result;

        private VisualElement _fieldElement;
        // private bool _arraySizeCondition;
        // private bool _richLabelCondition;

        protected override (VisualElement target, bool needUpdate) CreateSerializedUIToolkit()
        {
            VisualElement result = new PropertyField(FieldWithInfo.SerializedProperty)
            {
                style =
                {
                    flexGrow = 1,
                },
                name = FieldWithInfo.SerializedProperty.propertyPath,
            };
            return (result, false);
        }
    }
}
#endif
