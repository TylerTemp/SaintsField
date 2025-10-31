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

            Add(EnableButton = new Button(EnableClicked.Invoke)
            {
                text = "Enable",
                style =
                {
                    display = DisplayStyle.Flex,
                },
            });

            Add(AddButton = new Button(AddClicked.Invoke)
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