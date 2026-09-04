#if UNITY_EDITOR
using SaintsField.Editor;
using SaintsField.Editor.UIToolkitElements.ToasterDrawer;
#endif

using SaintsField.Playa;

namespace SaintsField.Samples.Scripts.SaintsEditor
{
    public class ToastExample : SaintsMonoBehaviour
    {
#if UNITY_EDITOR
        [Button]
        private void ToastMe([PropRange(0, 10)] int num)
        {
            Toast.Info("My toast", new Toast.Options
            {
                Description = "My description",
                Duration = double.PositiveInfinity,
                Icon = "star.png",  // or: Assets/my/pic.png
                IconColor = EColor.Gold.GetColor(),
                CloseButton = true,
            });
        }

        private ToasterElement loading;
        [Button]
        private void CreatAll()
        {
            Toast.Show("Default notification");
            Toast.Success("Operation completed successfully");
            Toast.Info("Here is some useful information");
            Toast.Warning("This action may need attention");
            Toast.Error("Something went wrong");
            loading = Toast.Loading("Loading data", new Toast.Options
            {
                CloseButton = true,
            });
        }

        [Button]
        private void CleanLoading()
        {
            Toast.Dismiss(loading);
        }
#endif
    }
}
