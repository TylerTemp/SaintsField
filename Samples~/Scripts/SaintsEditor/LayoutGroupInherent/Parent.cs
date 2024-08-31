using SaintsField.Playa;

namespace SaintsField.Samples.Scripts.SaintsEditor.LayoutGroupInherent
{
    public class Parent : SaintsMonoBehaviour
    {
        [LayoutStart("Parent", ELayout.Collapse)]
        public string parent1;
        public string parent2;
    }
}
