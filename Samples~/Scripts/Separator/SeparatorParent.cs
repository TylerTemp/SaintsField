using UnityEngine;

namespace SaintsField.Samples.Scripts.Separator
{
    public class SeparatorParent : MonoBehaviour
    {
        [FieldBelowSeparator("End Of <b><color=Aqua><container.Type/></color></b>", EAlign.Center, space: 10)]
        public string parent;
    }

}
