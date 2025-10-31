using UnityEngine.Events;
using UnityEngine.UIElements;

namespace SaintsField.Editor.Drawers.SceneDrawer
{
    public class SceneHelpBox : HelpBox
    {
        public readonly UnityEvent EnableClicked = new UnityEvent();
        public readonly UnityEvent AddClicked = new UnityEvent();

        public readonly Button EnableButton;
        public readonly Button AddButton;

        public SceneHelpBox()
        {
            messageType = HelpBoxMessageType.Error;

            VisualElement buttonContainer = new VisualElement
            {
                style =
                {
                    flexDirection = FlexDirection.Row,
                    justifyContent = Justify.FlexEnd,
                    flexGrow = 1,
                    flexShrink = 0,
                }
            };
            Add(buttonContainer);

            buttonContainer.Add(EnableButton = new Button(EnableClicked.Invoke)
            {
                text = "Enable",
                style =
                {
                    display = DisplayStyle.Flex,
                },
            });

            buttonContainer.Add(AddButton = new Button(AddClicked.Invoke)
            {
                text = "Add",
                style =
                {
                    display = DisplayStyle.Flex,
                },
            });
            style.display = DisplayStyle.None;
            style.flexGrow = 1;
        }
    }
}
