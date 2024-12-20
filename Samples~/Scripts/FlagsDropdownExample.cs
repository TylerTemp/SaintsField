using System;
using UnityEngine;

namespace SaintsField.Samples.Scripts
{
    public class FlagsDropdownExample : MonoBehaviour
    {
        [Serializable, Flags]
        public enum F
        {
            [RichLabel("[Null]")]
            Zero,
            [RichLabel("Options/Value1")]
            One = 1,
            [RichLabel("Options/Value2")]
            Two = 1 << 1,
            [RichLabel("Options/Value3")]
            Three = 1 << 2,
            Four = 1 << 3,
        }

        [FlagsDropdown]
        public F flags;
    }
}
