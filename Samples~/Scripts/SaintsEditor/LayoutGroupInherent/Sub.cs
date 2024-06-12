using SaintsField.Playa;

namespace SaintsField.Samples.Scripts.SaintsEditor.LayoutGroupInherent
{
    public class Sub : Parent
    {
        public string sub1;
        public string sub2;
        [LayoutGroup("Parent")]
        public string subPumpToParent;
    }
}
