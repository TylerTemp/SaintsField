using System;
using System.Collections.Generic;

namespace SaintsField.Editor.Units
{
    public class UnitInfo
    {
        public readonly string Name;
        public readonly IReadOnlyList<string> Symbols;
        public readonly EUnitCategory Category;

        public string PrimarySymbol => Symbols.Count == 0 ? Name : Symbols[0];

        public readonly Func<decimal, decimal> ToCategoryBase;
        public readonly Func<decimal, decimal> FromCategoryBase;

        public UnitInfo(string name, IReadOnlyList<string> symbols, EUnitCategory category,
            Func<decimal, decimal> toCategoryBase, Func<decimal, decimal> fromCategoryBase)
        {
            Name = name;
            Symbols = symbols;
            Category = category;
            ToCategoryBase = toCategoryBase;
            FromCategoryBase = fromCategoryBase;
        }
    }
}
