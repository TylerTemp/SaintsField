using SaintsField.Playa;

namespace SaintsField.Samples.Scripts.SaintsEditor.LayoutGroupInherent
{
    public class Parent : SaintsMonoBehavior
    {
        [LayoutGroup("Parent", ELayout.TitleOut  | ELayout.Background)]
        public string parent1;
        public string parent2;
    }
}
