using System;
using UnityEngine;

namespace SaintsField.Samples.Scripts
{
    public class FlagsDropdownExample : MonoBehaviour
    {
        [Serializable, Flags]
        public enum F
        {
            [RichLabel("Option0")]
            Zero,
            [RichLabel("Option1/1")]
            One = 1,
            [RichLabel("Option1/2")]
            Two = 1 << 1,
            [RichLabel("Option1/3")]
            Three = 1 << 2,
            Four = 1 << 3,
        }

        [FlagsDropdown]
        public F flags;
    }
}
