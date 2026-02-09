using UnityEngine;

namespace SaintsField.Samples.Scripts.SaintsEditor.Testing
{
    public class ListLabelContext : SaintsMonoBehaviour
    {
        public int[] arrInts;

        [LabelText("Arr!")]
        public int[] arrIntsLabel;
    }
}
