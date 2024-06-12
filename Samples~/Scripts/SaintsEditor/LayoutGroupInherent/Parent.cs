using SaintsField.Playa;

namespace SaintsField.Samples.Scripts.SaintsEditor.LayoutGroupInherent
{
    public class Parent : SaintsMonoBehavior
    {
        [LayoutGroup("Parent", ELayout.Collapse)]
        public string parent1;
        public string parent2;
    }
}
