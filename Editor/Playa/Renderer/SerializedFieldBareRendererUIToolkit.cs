#if UNITY_2021_3_OR_NEWER && !SAINTSFIELD_UI_TOOLKIT_DISABLE
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace SaintsField.Editor.Playa.Renderer
{
    public partial class SerializedFieldBareRenderer
    {
        protected override (VisualElement target, bool needUpdate) CreateTargetUIToolkit(VisualElement container)
        {
            PropertyField result = new PropertyField(FieldWithInfo.SerializedProperty)
            {
                style =
                {
                    flexGrow = 1,
                    width = new StyleLength(Length.Percent(100)),
                },
                name = FieldWithInfo.SerializedProperty.propertyPath,
            };
            result.Bind(FieldWithInfo.SerializedProperty.serializedObject);
            return (result, false);
        }
    }
}
#endif
