using System.Collections.Generic;
using SaintsField.Playa;
using UnityEngine;

namespace SaintsField.Samples.Scripts.SaintsEditor.Issues
{
    public class Issue127ListDrop: SaintsMonoBehaviour
    {
        [Required, ListDrawerSettings] public GameObject[] goArrayRequired;
        [Required] public GameObject[] goArrayDefault;
        [Required] public GameObject[] goArrayNothing;
    }
}
