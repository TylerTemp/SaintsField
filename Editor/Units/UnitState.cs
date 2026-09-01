using System;

namespace SaintsField.Editor.Units
{
    public class UnitState
    {
        public readonly UnitInfo BaseUnit;
        public UnitInfo DisplayUnit { get; private set; }
        public readonly string Error;

        public event Action DisplayUnitChanged;

        public UnitState(UnitAttribute attribute)
        {
            (bool baseFound, UnitInfo baseUnit) = Resolve(attribute.Base, attribute.BaseName);
            if (!baseFound)
            {
                Error = $"Unknown base unit '{attribute.BaseName ?? attribute.Base?.ToString()}'.";
                return;
            }

            (bool displayFound, UnitInfo displayUnit) = Resolve(attribute.Display, attribute.DisplayName);
            if (!displayFound)
            {
                Error = $"Unknown display unit '{attribute.DisplayName ?? attribute.Display?.ToString()}'.";
                return;
            }

            if (baseUnit.Category != displayUnit.Category)
            {
                Error = $"Base unit {baseUnit.Name} and display unit {displayUnit.Name} must share a category.";
                return;
            }

            BaseUnit = baseUnit;
            DisplayUnit = displayUnit;
            Error = "";
        }

        public void SetDisplayUnit(UnitInfo unit)
        {
            if (unit == null || BaseUnit == null || unit.Category != BaseUnit.Category ||
                ReferenceEquals(DisplayUnit, unit))
            {
                return;
            }

            DisplayUnit = unit;
            DisplayUnitChanged?.Invoke();
        }

        private static (bool found, UnitInfo result) Resolve(EUnit? unit, string name) => unit.HasValue
            ? UnitRegistry.GetUnitInfo(unit.Value)
            : UnitRegistry.GetUnitInfo(name);
    }
}
