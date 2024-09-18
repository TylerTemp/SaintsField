namespace SaintsField.Editor.Utils
{
    public class IMGUILabelHelper
    {
        public const string ClassName = "saints-field-imgui-container";

        public bool NoLabel;
        public string RichLabel;

        public IMGUILabelHelper(string defaultName) => RichLabel = defaultName;
    }
}
