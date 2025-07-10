using SaintsField.ComponentHeader;
using SaintsField.Playa;
using SaintsField.Samples.Scripts.SaintsEditor;

namespace SaintsField.Samples.Scripts.HeaderComponentExample.Issues
{
    public class Issue258ColorOriginalName : SaintsMonoBehaviour
    {
        [ShowInInspector][HeaderLabel("<color=red><field/>")]
        private const int Num = 10;
    }
}
