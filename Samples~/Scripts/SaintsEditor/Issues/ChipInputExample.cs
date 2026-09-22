using System;
using UnityEngine;

namespace SaintsField.Samples.Scripts.SaintsEditor.Issues
{
    public class ChipInputExample : SaintsMonoBehaviour
    {
        [Serializable]
        public enum MyEnumFlags
        {
            [InspectorName("Special")]
            None,

            [InspectorName("Weapon/1")]
            One,

            [InspectorName("Armor/2")]
            Two,

            [InspectorName("Armor/3")]
            Three,
        }

        public MyEnumFlags[] defaultList;

        [Chips] public MyEnumFlags[] enumList;
        [Chips(EUnique.Disable)] public MyEnumFlags[] disableList;
        [Chips(EUnique.Remove)] public MyEnumFlags[] removeList;
    }
}
