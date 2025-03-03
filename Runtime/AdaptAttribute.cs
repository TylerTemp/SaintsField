using SaintsField.Interfaces;
using UnityEngine;

namespace SaintsField
{
    public class AdaptAttribute: PropertyAttribute, ISaintsAttribute
    {
        public SaintsAttributeType AttributeType => SaintsAttributeType.Other;
        public string GroupBy => "";

        public readonly EUnit EUnit;

        public AdaptAttribute(EUnit eUnit)
        {
            Debug.Assert(eUnit != EUnit.Custom, "Custom is not allowed");
            EUnit = eUnit;
        }
    }
}
