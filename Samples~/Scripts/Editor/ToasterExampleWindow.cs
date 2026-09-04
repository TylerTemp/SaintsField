#if UNITY_EDITOR && UNITY_2021_3_OR_NEWER && !SAINTSFIELD_UI_TOOLKIT_DISABLE
using SaintsField.Utils;
using SaintsField.Editor;
using SaintsField.Editor.UIToolkitElements.ToasterDrawer;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace SaintsField.Samples.Scripts.Editor
{
    public class ToasterExampleWindow : SaintsEditorWindow
    {
        [MenuItem(RuntimeUtil.MenuRoot + "Example/Toaster")]
        private static void OpenWindow()
        {
            ToasterExampleWindow window = GetWindow<ToasterExampleWindow>(false, "Toaster");
            window.minSize = new Vector2(400, 470);
            window.Show();
        }

        protected override void EditorRelinkRootUIToolkit()
        {
            VisualElement root = rootVisualElement;
            // root.Clear();

            ScrollView scrollView = new ScrollView();
            scrollView.style.paddingTop = 16;
            scrollView.style.paddingRight = 16;
            scrollView.style.paddingBottom = 16;
            scrollView.style.paddingLeft = 16;
            scrollView.contentContainer.style.alignItems = Align.Center;
            root.Add(scrollView);

            Add(scrollView, new ToasterElement().Default("Default notification"));
            Add(scrollView, new ToasterElement().Success("Operation completed successfully"));
            Add(scrollView, new ToasterElement().Info("Here is some useful information"));
            Add(scrollView, new ToasterElement().Warning("This action may need attention"));
            Add(scrollView, new ToasterElement().Error("Something went wrong"));
            Add(scrollView, new ToasterElement().Loading("Loading data..."));
            // Add(scrollView, Toast.Show("My toast", new Toast.Options
            // {
            //     Description = "My description",
            //     Duration = double.PositiveInfinity,
            //     Icon = "eye.png",  // or: Assets/my/pic.png
            //     CloseButton = true,
            // }));


            ToasterElement action = new ToasterElement();
            action.Action("Event created", "Undo", () => action.Info("Action clicked"));
            Add(scrollView, action);

            VisualElement buttonRow = new VisualElement();
            buttonRow.style.marginTop = 16;
            buttonRow.style.flexDirection = FlexDirection.Row;
            buttonRow.style.flexWrap = Wrap.Wrap;
            buttonRow.style.alignSelf = Align.Stretch;
            scrollView.Add(buttonRow);

            AddButton(buttonRow, "Default", () => EditorToast("Default notification"));
            AddButton(buttonRow, "Success", () => EditorToastSuccess("Operation completed successfully"));
            AddButton(buttonRow, "Info", () => EditorToastInfo("Here is some useful information"));
            AddButton(buttonRow, "Warning", () => EditorToastWarning("This action may need attention"));
            AddButton(buttonRow, "Error", () => EditorToastError("Something went wrong"));
            AddButton(buttonRow, "Loading", () => EditorToastLoading("Loading data...", new Toast.Options
            {
                CloseButton = true,
                Duration = double.PositiveInfinity,
            }));
            AddButton(buttonRow, "Action", () => EditorToast("Event created", new Toast.Options
            {
                Description = "This toast uses the standard window API.",
                Action = new Toast.ActionOptions
                {
                    Label = "Close",
                },
            }));
        }

        private static void Add(VisualElement parent, ToasterElement toasterElement)
        {
            toasterElement.style.marginBottom = 8;
            toasterElement.style.alignSelf = Align.FlexEnd;
            parent.Add(toasterElement);
        }

        private static void AddButton(VisualElement parent, string label, System.Action onClick)
        {
            Button button = new Button(onClick)
            {
                text = label,
            };
            button.style.marginRight = 4;
            button.style.marginBottom = 4;
            parent.Add(button);
        }
    }
}
#endif
