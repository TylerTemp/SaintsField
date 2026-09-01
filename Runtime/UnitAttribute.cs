using System.Diagnostics;
using SaintsField.Interfaces;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace SaintsField
{
    [Conditional("UNITY_EDITOR")]
    [System.AttributeUsage(System.AttributeTargets.Field | System.AttributeTargets.Property)]
    public class UnitAttribute: PropertyAttribute, ISaintsAttribute
    {
        public virtual SaintsAttributeType AttributeType => SaintsAttributeType.Field;
        public virtual string GroupBy => "__LABEL_FIELD__";

        public readonly EUnit? Base;
        public readonly EUnit? Display;
        public readonly string BaseName;
        public readonly string DisplayName;

        public UnitAttribute(EUnit unit): this(unit, unit)
        {
        }

        public UnitAttribute(EUnit baseUnit, EUnit displayUnit)
            : this(baseUnit, null, displayUnit, null)
        {
        }

        public UnitAttribute(string unit): this(unit, unit)
        {
        }

        public UnitAttribute(string baseUnit, string displayUnit)
            : this(null, baseUnit, null, displayUnit)
        {
        }

        public UnitAttribute(EUnit baseUnit, string displayUnit)
            : this(baseUnit, null, null, displayUnit)
        {
        }

        public UnitAttribute(string baseUnit, EUnit displayUnit)
            : this(null, baseUnit, displayUnit, null)
        {
        }

        private UnitAttribute(EUnit? baseUnit, string baseName, EUnit? displayUnit, string displayName)
        {
            Debug.Assert(baseUnit.HasValue || !string.IsNullOrEmpty(baseName), "Custom base unit name is required");
            Debug.Assert(displayUnit.HasValue || !string.IsNullOrEmpty(displayName), "Custom display unit name is required");

            Base = baseUnit;
            Display = displayUnit;
            BaseName = baseName;
            DisplayName = displayName;
        }
    }
}
