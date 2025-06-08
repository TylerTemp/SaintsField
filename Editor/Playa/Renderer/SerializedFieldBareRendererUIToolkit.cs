#if UNITY_2021_3_OR_NEWER //&& !SAINTSFIELD_UI_TOOLKIT_DISABLE
using SaintsField.Editor.Utils;
using UnityEditor;
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

            string labelName = FieldWithInfo.SerializedProperty.displayName;

            _onSearchFieldUIToolkit.AddListener(Search);
            result.RegisterCallback<DetachFromPanelEvent>(_ => _onSearchFieldUIToolkit.RemoveListener(Search));

            return (result, false);

            void Search(string search)
            {
                DisplayStyle display = Util.UnityDefaultSimpleSearch(labelName, search)
                    ? DisplayStyle.Flex
                    : DisplayStyle.None;

                if (result.style.display != display)
                {
                    result.style.display = display;
                }
            }
        }
    }
}
#endif
