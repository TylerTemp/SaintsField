#if UNITY_2021_3_OR_NEWER
using UnityEngine.UIElements;

namespace SaintsField.Editor.UIToolkitElements
{
#if UNITY_6000_0_OR_NEWER
    [UxmlElement]
#endif
    // ReSharper disable once PartialTypeWithSinglePart
    public partial class CleanableTextInputFullWidth: CleanableTextInput
    {
        public CleanableTextInputFullWidth()
        {
            Init();
        }

        public CleanableTextInputFullWidth(string label) : base(label)
        {
            Init();
        }

        private void Init()
        {
            // TextField.style.maxWidth = StyleKeyword.Null;
            TextField.style.maxWidth = StyleKeyword.None;
        }
    }
}
#endif
