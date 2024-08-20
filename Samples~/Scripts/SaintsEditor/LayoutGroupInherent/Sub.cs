using SaintsField.Playa;

namespace SaintsField.Samples.Scripts.SaintsEditor.LayoutGroupInherent
{
    public class Sub : Parent
    {
        public string sub;

        [Layout("Parent", ELayout.TitleBox)]  // override config
        public string subPumpToParent;
    }
}
