using UnityEngine;

namespace SaintsField.Samples.Scripts.Interface
{
    public class InterfaceMultiple : MonoBehaviour
    {
        [BelowRichLabel("$" + nameof(this1))]
        public SaintsObjInterface<IMultiple> this1;
    }
}
