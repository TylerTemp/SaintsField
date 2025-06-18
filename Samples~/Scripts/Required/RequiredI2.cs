#if SAINTSFIELD_I2_LOC
using I2.Loc;
#endif
using UnityEngine;

namespace SaintsField.Samples.Scripts.Required
{
    public class RequiredI2 : MonoBehaviour
    {
#if SAINTSFIELD_I2_LOC
        [Required] public LocalizedString ls1;
        [Required] public LocalizedString ls2;
#endif
    }
}
