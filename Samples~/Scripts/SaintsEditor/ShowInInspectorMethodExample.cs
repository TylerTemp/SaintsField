using SaintsField.Playa;

namespace SaintsField.Samples.Scripts.SaintsEditor
{
    public class ShowInInspectorMethodExample : SaintsMonoBehaviour
    {
        [ShowInInspector]
        private int AddCalculator(int a, int b)
        {
            return a + b;
        }
    }
}
