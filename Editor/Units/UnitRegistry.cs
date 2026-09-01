using System;
using System.Collections.Generic;
using System.Linq;

namespace SaintsField.Editor.Units
{
    public static class UnitRegistry
    {
        private static readonly Dictionary<EUnit, UnitInfo> UnitToInfo =
            new Dictionary<EUnit, UnitInfo>();
        private static readonly Dictionary<string, UnitInfo> NameToInfo =
            new Dictionary<string, UnitInfo>(StringComparer.OrdinalIgnoreCase);
        private static readonly List<UnitInfo> AllUnits = new List<UnitInfo>();

        static UnitRegistry()
        {
            AddBuiltIn(EUnit.Ratio, "Ratio", new[] { "ratio" }, EUnitCategory.Percent, 1m);
            AddBuiltIn(EUnit.Nanometer, "Nanometer", new[] { "nm" }, EUnitCategory.Distance, 1000000000.0m);
            AddBuiltIn(EUnit.Micrometer, "Micrometer", new[] { "µm", "um" }, EUnitCategory.Distance, 1000000.0m);
            AddBuiltIn(EUnit.Millimeter, "Millimeter", new[] { "mm" }, EUnitCategory.Distance, 1000.0m);
            AddBuiltIn(EUnit.Centimeter, "Centimeter", new[] { "cm" }, EUnitCategory.Distance, 100.0m);
            AddBuiltIn(EUnit.Meter, "Meter", new[] { "m" }, EUnitCategory.Distance, 1.0m);
            AddBuiltIn(EUnit.Kilometer, "Kilometer", new[] { "km" }, EUnitCategory.Distance, 0.001m);
            AddBuiltIn(EUnit.Inch, "Inch", new[] { "\"", "in" }, EUnitCategory.Distance, 39.37007874m);
            AddBuiltIn(EUnit.Feet, "Feet", new[] { "'", "ft" }, EUnitCategory.Distance, 3.280839895m);
            AddBuiltIn(EUnit.Mile, "Mile", new[] { "mi" }, EUnitCategory.Distance, 0.0006213712m);
            AddBuiltIn(EUnit.Yard, "Yard", new[] { "yd" }, EUnitCategory.Distance, 1.0936132983m);
            AddBuiltIn(EUnit.NauticalMile, "Nautical Mile", new[] { "nmi" }, EUnitCategory.Distance, 0.0005399568m);
            AddBuiltIn(EUnit.LightYear, "Light Year", new[] { "ly" }, EUnitCategory.Distance, 0.0000000000000001057000834m);
            AddBuiltIn(EUnit.Parsec, "Parsec", new[] { "pc" }, EUnitCategory.Distance, 0.00000000000000003240779289m);
            AddBuiltIn(EUnit.AstronomicalUnit, "Astronomical Unit", new[] { "AU" }, EUnitCategory.Distance, 0.000000000006684587122m);
            AddBuiltIn(EUnit.CubicMeter, "Cubic Meter", new[] { "m³" }, EUnitCategory.Volume, 1.0m);
            AddBuiltIn(EUnit.CubicKilometer, "Cubic Kilometer", new[] { "km³" }, EUnitCategory.Volume, 0.000000001m);
            AddBuiltIn(EUnit.CubicCentimeter, "Cubic Centimeter", new[] { "cm³" }, EUnitCategory.Volume, 1000000m);
            AddBuiltIn(EUnit.CubicMillimeter, "Cubic Millimeter", new[] { "mm³" }, EUnitCategory.Volume, 1000000000m);
            AddBuiltIn(EUnit.Liter, "Liter", new[] { "L" }, EUnitCategory.Volume, 1000m);
            AddBuiltIn(EUnit.Milliliter, "Milliliter", new[] { "ml" }, EUnitCategory.Volume, 1000000m);
            AddBuiltIn(EUnit.Centiliter, "Centiliter", new[] { "cl" }, EUnitCategory.Volume, 100000.0m);
            AddBuiltIn(EUnit.Deciliter, "Deciliter", new[] { "dl" }, EUnitCategory.Volume, 10000.0m);
            AddBuiltIn(EUnit.Hectoliter, "Hectoliter", new[] { "hl" }, EUnitCategory.Volume, 10.0m);
            AddBuiltIn(EUnit.CubicInch, "Cubic Inch", new[] { "in³" }, EUnitCategory.Volume, 61023.744095m);
            AddBuiltIn(EUnit.CubicFeet, "Cubic Feet", new[] { "ft³" }, EUnitCategory.Volume, 35.314666721m);
            AddBuiltIn(EUnit.CubicYard, "Cubic Yard", new[] { "yd³" }, EUnitCategory.Volume, 1.3079506193m);
            AddBuiltIn(EUnit.AcreFeet, "Acre Feet", new[] { "acre ft" }, EUnitCategory.Volume, 0.0008107132m);
            AddBuiltIn(EUnit.BarrelOil, "Barrel Oil", new[] { "bbl (oil)" }, EUnitCategory.Volume, 6.2898107704m);
            AddBuiltIn(EUnit.TeaspoonUS, "Teaspoon (US)", new[] { "tsp (US)" }, EUnitCategory.Volume, 202884.13621m);
            AddBuiltIn(EUnit.TablespoonUS, "Tablespoon (US)", new[] { "tbsp (US)" }, EUnitCategory.Volume, 67628.045404m);
            AddBuiltIn(EUnit.CupUS, "Cup (US)", new[] { "cup (US)" }, EUnitCategory.Volume, 4226.7528377m);
            AddBuiltIn(EUnit.GillUS, "Gill (US)", new[] { "gill (US)" }, EUnitCategory.Volume, 8453.5056755m);
            AddBuiltIn(EUnit.PintUS, "Pint (US)", new[] { "pt (US)" }, EUnitCategory.Volume, 2113.3764189m);
            AddBuiltIn(EUnit.QuartUS, "Quart (US)", new[] { "qt (US)" }, EUnitCategory.Volume, 1056.6882094m);
            AddBuiltIn(EUnit.GallonUS, "Gallon (US)", new[] { "gal (US)" }, EUnitCategory.Volume, 264.17205236m);
            AddBuiltIn(EUnit.BarrelUS, "Barrel (US)", new[] { "bbl (US)" }, EUnitCategory.Volume, 8.3864143606m);
            AddBuiltIn(EUnit.FluidOunceUS, "Fluid Ounce (US)", new[] { "fl oz (US)" }, EUnitCategory.Volume, 33814.022702m);
            AddBuiltIn(EUnit.BarrelUK, "Barrel (UK)", new[] { "bbl (UK)" }, EUnitCategory.Volume, 6.1102568972m);
            AddBuiltIn(EUnit.FluidOunceUK, "Fluid Ounce (UK)", new[] { "fl oz (UK)" }, EUnitCategory.Volume, 35195.079728m);
            AddBuiltIn(EUnit.TeaspoonUK, "Teaspoon (UK)", new[] { "tsp (UK)" }, EUnitCategory.Volume, 168936.38269m);
            AddBuiltIn(EUnit.TablespoonUK, "Tablespoon (UK)", new[] { "tbsp (UK)" }, EUnitCategory.Volume, 56312.127565m);
            AddBuiltIn(EUnit.CupUK, "Cup (UK)", new[] { "cup (UK)" }, EUnitCategory.Volume, 3519.5079728m);
            AddBuiltIn(EUnit.GillUK, "Gill (UK)", new[] { "gill (UK)" }, EUnitCategory.Volume, 7039.0159456m);
            AddBuiltIn(EUnit.PintUK, "Pint (UK)", new[] { "pt (UK)" }, EUnitCategory.Volume, 1759.7539864m);
            AddBuiltIn(EUnit.QuartUK, "Quart (UK)", new[] { "qt (UK)" }, EUnitCategory.Volume, 879.8769932m);
            AddBuiltIn(EUnit.GallonUK, "Gallon (UK)", new[] { "gal (UK)" }, EUnitCategory.Volume, 219.9692483m);
            AddBuiltIn(EUnit.SquareMeter, "Square Meter", new[] { "m²" }, EUnitCategory.Area, 1.0m);
            AddBuiltIn(EUnit.SquareKilometer, "Square Kilometer", new[] { "km²" }, EUnitCategory.Area, 0.000001m);
            AddBuiltIn(EUnit.SquareCentimeter, "Square Centimeter", new[] { "cm²" }, EUnitCategory.Area, 10000m);
            AddBuiltIn(EUnit.SquareMillimeter, "Square Millimeter", new[] { "mm²" }, EUnitCategory.Area, 1000000m);
            AddBuiltIn(EUnit.SquareMicrometer, "Square Micrometer", new[] { "µm²", "um²" }, EUnitCategory.Area, 1000000000000m);
            AddBuiltIn(EUnit.SquareInch, "Square Inch", new[] { "in²" }, EUnitCategory.Area, 1550.0031m);
            AddBuiltIn(EUnit.SquareFeet, "Square Feet", new[] { "ft²" }, EUnitCategory.Area, 10.763910417m);
            AddBuiltIn(EUnit.SquareYard, "Square Yard", new[] { "yd²" }, EUnitCategory.Area, 1.1959900463m);
            AddBuiltIn(EUnit.SquareMile, "Square Mile", new[] { "mi²" }, EUnitCategory.Area, 0.0000003861021585m);
            AddBuiltIn(EUnit.Hectare, "Hectare", new[] { "ha" }, EUnitCategory.Area, 0.0001m);
            AddBuiltIn(EUnit.Acre, "Acre", new[] { "ac" }, EUnitCategory.Area, 0.0002471054m);
            AddBuiltIn(EUnit.Are, "Are", new[] { "a" }, EUnitCategory.Area, 0.01m);
            AddBuiltIn(EUnit.Joule, "Joule", new[] { "J" }, EUnitCategory.Energy, 1m);
            AddBuiltIn(EUnit.Kilojoule, "Kilojoule", new[] { "kJ" }, EUnitCategory.Energy, 0.001m);
            AddBuiltIn(EUnit.WattHour, "Watt-hour", new[] { "W*h" }, EUnitCategory.Energy, 0.0002777778m);
            AddBuiltIn(EUnit.KilowattHour, "Kilowatt-Hour", new[] { "kW*h" }, EUnitCategory.Energy, 0.0000002777777777m);
            AddBuiltIn(EUnit.HorsepowerHour, "Horsepower-Hour", new[] { "hp*h" }, EUnitCategory.Energy, 0.0000003725061361m);
            AddBuiltIn(EUnit.Newton, "Newton", new[] { "N" }, EUnitCategory.Force, 1.0m);
            AddBuiltIn(EUnit.Kilonewton, "Kilonewton", new[] { "kN" }, EUnitCategory.Force, 0.001m);
            AddBuiltIn(EUnit.Meganewton, "Meganewton", new[] { "MN" }, EUnitCategory.Force, 0.000001m);
            AddBuiltIn(EUnit.Giganewton, "Giganewton", new[] { "GN" }, EUnitCategory.Force, 0.000000001m);
            AddBuiltIn(EUnit.Teranewton, "Teranewton", new[] { "TN" }, EUnitCategory.Force, 0.000000000001m);
            AddBuiltIn(EUnit.Centinewton, "Centinewton", new[] { "cN" }, EUnitCategory.Force, 100m);
            AddBuiltIn(EUnit.Millinewton, "Millinewton", new[] { "mN" }, EUnitCategory.Force, 1000m);
            AddBuiltIn(EUnit.JouleMeter, "Joule/Meter", new[] { "J/m" }, EUnitCategory.Force, 1m);
            AddBuiltIn(EUnit.JouleCentimeter, "Joule/Centimeter", new[] { "J/cm" }, EUnitCategory.Force, 100m);
            AddBuiltIn(EUnit.GramForce, "Gram-Force", new[] { "gf" }, EUnitCategory.Force, 101.9716213m);
            AddBuiltIn(EUnit.KilogramForce, "Kilogram-Force", new[] { "kgf" }, EUnitCategory.Force, 0.1019716213m);
            AddBuiltIn(EUnit.TonForce, "Ton-Force", new[] { "tf" }, EUnitCategory.Force, 0.0001019716m);
            AddBuiltIn(EUnit.PoundForce, "Pound-Force", new[] { "lbf" }, EUnitCategory.Force, 0.2248089431m);
            AddBuiltIn(EUnit.KilopoundForce, "Kilopound-Force", new[] { "klbf" }, EUnitCategory.Force, 0.0002248089m);
            AddBuiltIn(EUnit.OunceForce, "Ounce-Force", new[] { "ozf" }, EUnitCategory.Force, 3.5969430896m);
            AddBuiltIn(EUnit.MetersPerSecond, "Meters per Second", new[] { "m/s" }, EUnitCategory.Speed, 1.0m);
            AddBuiltIn(EUnit.MetersPerMinute, "Meters per Minute", new[] { "m/min" }, EUnitCategory.Speed, 60m);
            AddBuiltIn(EUnit.MetersPerHour, "Meters per Hour", new[] { "m/h" }, EUnitCategory.Speed, 3600m);
            AddBuiltIn(EUnit.KilometersPerSecond, "Kilometers per Second", new[] { "km/s" }, EUnitCategory.Speed, 0.001m);
            AddBuiltIn(EUnit.KilometersPerMinute, "Kilometers per Minute", new[] { "km/min" }, EUnitCategory.Speed, 0.06m);
            AddBuiltIn(EUnit.KilometersPerHour, "Kilometers per Hour", new[] { "km/h" }, EUnitCategory.Speed, 3.6m);
            AddBuiltIn(EUnit.CentimetersPerSecond, "Centimeters per Second", new[] { "cm/s" }, EUnitCategory.Speed, 100m);
            AddBuiltIn(EUnit.CentimetersPerMinute, "Centimeters per Minute", new[] { "cm/min" }, EUnitCategory.Speed, 6000m);
            AddBuiltIn(EUnit.CentimetersPerHour, "Centimeters per Hour", new[] { "cm/h" }, EUnitCategory.Speed, 360000m);
            AddBuiltIn(EUnit.MillimetersPerSecond, "Millimeters per Second", new[] { "mm/s" }, EUnitCategory.Speed, 1000m);
            AddBuiltIn(EUnit.MillimetersPerMinute, "Millimeters per Minute", new[] { "mm/min" }, EUnitCategory.Speed, 60000m);
            AddBuiltIn(EUnit.MillimetersPerHour, "Millimeters per Hour", new[] { "mm/h" }, EUnitCategory.Speed, 3600000m);
            AddBuiltIn(EUnit.FeetPerSecond, "Feet per Second", new[] { "ft/s", "\"/s" }, EUnitCategory.Speed, 3.280839895m);
            AddBuiltIn(EUnit.FeetPerMinute, "Feet per Minute", new[] { "ft/min", "\"/min" }, EUnitCategory.Speed, 196.8503937m);
            AddBuiltIn(EUnit.FeetPerHour, "Feet per Hour", new[] { "ft/h", "\"/h" }, EUnitCategory.Speed, 11811.023622m);
            AddBuiltIn(EUnit.YardsPerSecond, "Yards per Second", new[] { "yd/s" }, EUnitCategory.Speed, 1.0936132983m);
            AddBuiltIn(EUnit.YardsPerMinute, "Yards per Minute", new[] { "yd/min" }, EUnitCategory.Speed, 65.616797m);
            AddBuiltIn(EUnit.YardsPerHour, "Yards per Hour", new[] { "yd/h" }, EUnitCategory.Speed, 3937.007874m);
            AddBuiltIn(EUnit.MilesPerSecond, "Miles per Second", new[] { "mi/s" }, EUnitCategory.Speed, 0.0006213712m);
            AddBuiltIn(EUnit.MilesPerMinute, "Miles per Minute", new[] { "mi/min" }, EUnitCategory.Speed, 0.0372822715m);
            AddBuiltIn(EUnit.MilesPerHour, "Miles per Hour", new[] { "mi/h" }, EUnitCategory.Speed, 2.2369362921m);
            AddBuiltIn(EUnit.Knot, "Knots", new[] { "kn" }, EUnitCategory.Speed, 1.9438444924m);
            AddBuiltIn(EUnit.KnotUK, "Knots (UK)", new[] { "kt (UK)" }, EUnitCategory.Speed, 1.9426025694m);
            AddBuiltIn(EUnit.SpeedOfLight, "Speed of light (Vacuum)", new[] { "c" }, EUnitCategory.Speed, 0.000000003335640951m);
            AddBuiltIn(EUnit.Bit, "Bit", new[] { "bit" }, EUnitCategory.DataStorage, 8000000m);
            AddBuiltIn(EUnit.Kilobit, "Kilobit", new[] { "kbit" }, EUnitCategory.DataStorage, 8000m);
            AddBuiltIn(EUnit.Megabit, "Megabit", new[] { "Mbit" }, EUnitCategory.DataStorage, 8m);
            AddBuiltIn(EUnit.Gigabit, "Gigabit", new[] { "Gbit" }, EUnitCategory.DataStorage, 0.008m);
            AddBuiltIn(EUnit.Terabit, "Terabit", new[] { "Tbit" }, EUnitCategory.DataStorage, 0.000008m);
            AddBuiltIn(EUnit.Petabit, "Petabit", new[] { "Pbit" }, EUnitCategory.DataStorage, 0.000000008m);
            AddBuiltIn(EUnit.Byte, "Byte", new[] { "B" }, EUnitCategory.DataStorage, 1000000m);
            AddBuiltIn(EUnit.Kilobyte, "Kilobyte", new[] { "kB" }, EUnitCategory.DataStorage, 1000m);
            AddBuiltIn(EUnit.Kibibyte, "Kibibyte", new[] { "kiB" }, EUnitCategory.DataStorage, 976.5625m);
            AddBuiltIn(EUnit.Megabyte, "Megabyte", new[] { "MB" }, EUnitCategory.DataStorage, 1m);
            AddBuiltIn(EUnit.Mebibyte, "Mebibyte", new[] { "MiB" }, EUnitCategory.DataStorage, 0.9536743164m);
            AddBuiltIn(EUnit.Gigabyte, "Gigabyte", new[] { "GB" }, EUnitCategory.DataStorage, 0.001m);
            AddBuiltIn(EUnit.Gibibyte, "Gibibyte", new[] { "GiB" }, EUnitCategory.DataStorage, 0.0009313226m);
            AddBuiltIn(EUnit.Terabyte, "Terabyte", new[] { "TB" }, EUnitCategory.DataStorage, 0.000001m);
            AddBuiltIn(EUnit.Tebibyte, "Tebibyte", new[] { "TiB" }, EUnitCategory.DataStorage, 0.0000009094947017m);
            AddBuiltIn(EUnit.Petabyte, "Petabyte", new[] { "PB" }, EUnitCategory.DataStorage, 0.000000001m);
            AddBuiltIn(EUnit.Pebibyte, "Pebibyte", new[] { "PiB" }, EUnitCategory.DataStorage, 0.0000000008881784197m);
            AddBuiltIn(EUnit.Kilogram, "Kilogram", new[] { "kg" }, EUnitCategory.Weight, 1.0m);
            AddBuiltIn(EUnit.Hectogram, "Hectogram", new[] { "hg" }, EUnitCategory.Weight, 10m);
            AddBuiltIn(EUnit.Dekagram, "Dekagram", new[] { "dag" }, EUnitCategory.Weight, 100m);
            AddBuiltIn(EUnit.Gram, "Gram", new[] { "g" }, EUnitCategory.Weight, 1000m);
            AddBuiltIn(EUnit.Decigram, "Decigram", new[] { "dg" }, EUnitCategory.Weight, 10000m);
            AddBuiltIn(EUnit.Centigram, "Centigram", new[] { "cg" }, EUnitCategory.Weight, 100000m);
            AddBuiltIn(EUnit.Milligram, "Milligram", new[] { "mg" }, EUnitCategory.Weight, 1000000m);
            AddBuiltIn(EUnit.MetricTon, "Metric Ton", new[] { "t", "Mg" }, EUnitCategory.Weight, 0.001m);
            AddBuiltIn(EUnit.Pounds, "Pounds", new[] { "lbs" }, EUnitCategory.Weight, 2.20462m);
            AddBuiltIn(EUnit.ShortTon, "Short Ton", new[] { "sh.tn.", "sh.t." }, EUnitCategory.Weight, 0.00110231m);
            AddBuiltIn(EUnit.LongTon, "Long Ton", new[] { "l.tn.", "l.t." }, EUnitCategory.Weight, 0.000984207m);
            AddBuiltIn(EUnit.Ounce, "Ounce", new[] { "oz" }, EUnitCategory.Weight, 35.27396195m);
            AddBuiltIn(EUnit.StoneUS, "Stone (US)", new[] { "stone (US)" }, EUnitCategory.Weight, 0.1763698097m);
            AddBuiltIn(EUnit.StoneUK, "Stone (UK)", new[] { "stone (UK)" }, EUnitCategory.Weight, 0.1574730444m);
            AddBuiltIn(EUnit.QuarterUS, "Quarter (US)", new[] { "qr (US)" }, EUnitCategory.Weight, 0.0881849049m);
            AddBuiltIn(EUnit.QuarterUK, "Quarter (UK)", new[] { "qr (UK)" }, EUnitCategory.Weight, 0.0787365222m);
            AddBuiltIn(EUnit.Slug, "Slug", new[] { "slug" }, EUnitCategory.Weight, 0.0685217659m);
            AddBuiltIn(EUnit.Grain, "Grain", new[] { "gr" }, EUnitCategory.Weight, 15432.358353m);
            AddBuiltIn(EUnit.Celsius, "Celsius", new[] { "°C", "C" }, EUnitCategory.Temperature,
                value => value + 273.15m, value => value - 273.15m);
            AddBuiltIn(EUnit.Fahrenheit, "Fahrenheit", new[] { "°F", "F" }, EUnitCategory.Temperature,
                value => (value + 459.67m) * 5m / 9m, value => value * 9m / 5m - 459.67m);
            AddBuiltIn(EUnit.Kelvin, "Kelvin", new[] { "°K", "K" }, EUnitCategory.Temperature, 1m);
            AddBuiltIn(EUnit.Pascal, "Pascal", new[] { "Pa" }, EUnitCategory.Pressure, 1m);
            AddBuiltIn(EUnit.Decipascal, "Decipascal", new[] { "dPa" }, EUnitCategory.Pressure, 10m);
            AddBuiltIn(EUnit.Centipascal, "Centipascal", new[] { "cPa" }, EUnitCategory.Pressure, 100m);
            AddBuiltIn(EUnit.Millipascal, "Millipascal", new[] { "mPa" }, EUnitCategory.Pressure, 1000m);
            AddBuiltIn(EUnit.Micropascal, "Micropascal", new[] { "µPa", "uPa" }, EUnitCategory.Pressure, 1000000m);
            AddBuiltIn(EUnit.Kilopascal, "Kilopascal", new[] { "kPa" }, EUnitCategory.Pressure, 0.001m);
            AddBuiltIn(EUnit.Megapascal, "Megapascal", new[] { "MPa" }, EUnitCategory.Pressure, 0.000001m);
            AddBuiltIn(EUnit.Gigapascal, "Gigapascal", new[] { "GPa" }, EUnitCategory.Pressure, 0.000000001m);
            AddBuiltIn(EUnit.Bar, "Bar", new[] { "bar" }, EUnitCategory.Pressure, 0.00001m);
            AddBuiltIn(EUnit.Millibar, "Millibar", new[] { "mbar" }, EUnitCategory.Pressure, 0.01m);
            AddBuiltIn(EUnit.Microbar, "Microbar", new[] { "µbar", "ubar" }, EUnitCategory.Pressure, 10m);
            AddBuiltIn(EUnit.PSI, "PSI", new[] { "psi" }, EUnitCategory.Pressure, 0.0001450377m);
            AddBuiltIn(EUnit.KSI, "KSI", new[] { "ksi" }, EUnitCategory.Pressure, 0.0000001450377377m);
            AddBuiltIn(EUnit.StandardAtmosphere, "Standard Atmosphere", new[] { "atm" }, EUnitCategory.Pressure, 0.0000098692m);
            AddBuiltIn(EUnit.Watt, "Watt", new[] { "W" }, EUnitCategory.Power, 1m);
            AddBuiltIn(EUnit.Kilowatt, "Kilowatt", new[] { "kW" }, EUnitCategory.Power, 0.001m);
            AddBuiltIn(EUnit.Megawatt, "Megawatt", new[] { "MW" }, EUnitCategory.Power, 0.000001m);
            AddBuiltIn(EUnit.Gigawatt, "Gigawatt", new[] { "GW" }, EUnitCategory.Power, 0.000000001m);
            AddBuiltIn(EUnit.Terawatt, "Terawatt", new[] { "TW" }, EUnitCategory.Power, 0.000000000001m);
            AddBuiltIn(EUnit.Horsepower, "Horsepower", new[] { "hp", "ft*lbf/s" }, EUnitCategory.Power, 0.0013410221m);
            AddBuiltIn(EUnit.JouleSecond, "Joule/Second", new[] { "J/s" }, EUnitCategory.Power, 1m);
            AddBuiltIn(EUnit.JouleMinute, "Joule/Minute", new[] { "J/min" }, EUnitCategory.Power, 60m);
            AddBuiltIn(EUnit.JouleHour, "Joule/Hour", new[] { "J/h" }, EUnitCategory.Power, 3600m);
            AddBuiltIn(EUnit.KilojouleSecond, "Kilojoule/Second", new[] { "kJ/s" }, EUnitCategory.Power, 0.001m);
            AddBuiltIn(EUnit.KilojouleMinute, "Kilojoule/Minute", new[] { "kJ/min" }, EUnitCategory.Power, 0.06m);
            AddBuiltIn(EUnit.KilojouleHour, "Kilojoule/Hour", new[] { "kJ/h" }, EUnitCategory.Power, 3.6m);
            AddBuiltIn(EUnit.Second, "Second", new[] { "s" }, EUnitCategory.Time, 1.0m);
            AddBuiltIn(EUnit.Millisecond, "Millisecond", new[] { "ms" }, EUnitCategory.Time, 1000m);
            AddBuiltIn(EUnit.Microsecond, "Microsecond", new[] { "µs", "us" }, EUnitCategory.Time, 1000000m);
            AddBuiltIn(EUnit.Nanosecond, "Nanosecond", new[] { "ns" }, EUnitCategory.Time, 1000000000m);
            AddBuiltIn(EUnit.Minute, "Minute", new[] { "min" }, EUnitCategory.Time, 0.0166666666666666666666666667m);
            AddBuiltIn(EUnit.Hour, "Hour", new[] { "h" }, EUnitCategory.Time, 0.0002777777777777777777777778m);
            AddBuiltIn(EUnit.Day, "Day", new[] { "d" }, EUnitCategory.Time, 0.0000115740740740740740740741m);
            AddBuiltIn(EUnit.Week, "Week", new[] { "week" }, EUnitCategory.Time, 0.0000016534m);
            AddBuiltIn(EUnit.Radian, "Radian", new[] { "rad" }, EUnitCategory.Angle, 0.0174532925199432777777777778m);
            AddBuiltIn(EUnit.Degree, "Degree", new[] { "°", "d" }, EUnitCategory.Angle, 1.0m);
            AddBuiltIn(EUnit.Turn, "Turn", new[] { "turns" }, EUnitCategory.Angle, 0.0027777777777777777777777778m);
            AddBuiltIn(EUnit.Grad, "Grad", new[] { "^g" }, EUnitCategory.Angle, 1.1111111111m);
            AddBuiltIn(EUnit.SecondsOfAngle, "Seconds of Angle", new[] { "\"" }, EUnitCategory.Angle, 3600m);
            AddBuiltIn(EUnit.MinutesOfAngle, "Minutes of Angle", new[] { "'", "MOA" }, EUnitCategory.Angle, 60m);
            AddBuiltIn(EUnit.Mil, "Mil", new[] { "mil" }, EUnitCategory.Angle, 17.777777778m);
            AddBuiltIn(EUnit.MetersPerSecondSquared, "Meters per second squared", new[] { "m/s²", "m/s/s" }, EUnitCategory.Acceleration, 1.0m);
            AddBuiltIn(EUnit.DecimetersPerSecondSquared, "Decimeters per second squared", new[] { "dm/s²", "dm/s/s" }, EUnitCategory.Acceleration, 10m);
            AddBuiltIn(EUnit.CentimetersPerSecondSquared, "Centimeters per second squared", new[] { "cm/s²", "cm/s/s" }, EUnitCategory.Acceleration, 100m);
            AddBuiltIn(EUnit.MillimetersPerSecondSquared, "Millimeters per second squared", new[] { "mm/s²", "mm/s/s" }, EUnitCategory.Acceleration, 1000m);
            AddBuiltIn(EUnit.MicrometersPerSecondSquared, "Micrometers per second squared", new[] { "µm/s²", "µm/s/s" }, EUnitCategory.Acceleration, 1000000m);
            AddBuiltIn(EUnit.DekametersPerSecondSquared, "Dekameters per second squared", new[] { "Dm/s²", "Dm/s/s" }, EUnitCategory.Acceleration, 0.1m);
            AddBuiltIn(EUnit.HectometersPerSecondSquared, "Hectometers per second squared", new[] { "hm/s²", "hm/s/s" }, EUnitCategory.Acceleration, 0.01m);
            AddBuiltIn(EUnit.KilometersPerSecondSquared, "Kilometers per second squared", new[] { "km/s²", "km/s/s" }, EUnitCategory.Acceleration, 0.001m);
            AddBuiltIn(EUnit.MilePerSecondSquared, "Mile per second squared", new[] { "mi/s²", "mi/s/s" }, EUnitCategory.Acceleration, 0.0006213712m);
            AddBuiltIn(EUnit.YardPerSecondSquared, "Yard per second squared", new[] { "yd/s²", "yd/s/s" }, EUnitCategory.Acceleration, 1.0936132983m);
            AddBuiltIn(EUnit.FeetPerSecondSquared, "Feet per second squared", new[] { "ft/s²", "ft/s/s" }, EUnitCategory.Acceleration, 3.280839895m);
            AddBuiltIn(EUnit.InchPerSecondSquared, "Inch per second squared", new[] { "in/s²", "in/s/s" }, EUnitCategory.Acceleration, 39.37007874m);
            AddBuiltIn(EUnit.GForce, "G-Force", new[] { "g" }, EUnitCategory.Acceleration, 0.1019716213m);
            AddBuiltIn(EUnit.NewtonMeter, "Newton Meter", new[] { "N⋅m" }, EUnitCategory.Torque, 1m);
            AddBuiltIn(EUnit.NewtonCentimeter, "Newton Centimeter", new[] { "N⋅cm" }, EUnitCategory.Torque, 100m);
            AddBuiltIn(EUnit.NewtonMillimeter, "Newton Millimeter", new[] { "N⋅mm" }, EUnitCategory.Torque, 1000m);
            AddBuiltIn(EUnit.KilonewtonMeter, "Kilonewton Meter", new[] { "kN⋅m" }, EUnitCategory.Torque, 0.001m);
            AddBuiltIn(EUnit.KilogramForceMeter, "Kilogram-force Meter", new[] { "kgf⋅m" }, EUnitCategory.Torque, 0.1019716213m);
            AddBuiltIn(EUnit.KilogramForceCentimeter, "Kilogram-force Centimeter", new[] { "kgf⋅cm" }, EUnitCategory.Torque, 10.19716213m);
            AddBuiltIn(EUnit.KilogramForceMillimeter, "Kilogram-force Millimeter", new[] { "kgf⋅mm" }, EUnitCategory.Torque, 101.9716213m);
            AddBuiltIn(EUnit.GramForceMeter, "Gram-force Meter", new[] { "gf⋅m" }, EUnitCategory.Torque, 101.9716213m);
            AddBuiltIn(EUnit.GramForceCentimeter, "Gram-force Centimeter", new[] { "gf⋅cm" }, EUnitCategory.Torque, 10197.16213m);
            AddBuiltIn(EUnit.GramForceMillimeter, "Gram-force Millimeter", new[] { "gf⋅mm" }, EUnitCategory.Torque, 101971.6213m);
            AddBuiltIn(EUnit.PoundFeet, "Pound-force Feet", new[] { "lb⋅ft", "lbf-ft" }, EUnitCategory.Torque, 0.7375621212m);
            AddBuiltIn(EUnit.PoundInch, "Pound-force Inch", new[] { "lb⋅in", "lbf-in" }, EUnitCategory.Torque, 8.850745454m);
            AddBuiltIn(EUnit.OuncecFeet, "Ounce-force Feet", new[] { "oz⋅ft" }, EUnitCategory.Torque, 11.800994078m);
            AddBuiltIn(EUnit.OuncecInch, "Ounce-force Inch", new[] { "oz⋅in" }, EUnitCategory.Torque, 141.61192894m);
            AddBuiltIn(EUnit.RadiansPerSecond, "Radians per Second", new[] { "rad/s", "r/s" }, EUnitCategory.AngularVelocity, 1m);
            AddBuiltIn(EUnit.RadiansPerMinute, "Radians per Minute", new[] { "rad/min", "r/min" }, EUnitCategory.AngularVelocity, 60m);
            AddBuiltIn(EUnit.RadiansPerHour, "Radians per Hour", new[] { "rad/h", "r/h" }, EUnitCategory.AngularVelocity, 3600m);
            AddBuiltIn(EUnit.RadiansPerDay, "Radians per Day", new[] { "rad/d", "r/d" }, EUnitCategory.AngularVelocity, 86400m);
            AddBuiltIn(EUnit.DegreesPerSecond, "Degrees per Second", new[] { "°/s", "d/s" }, EUnitCategory.AngularVelocity, 57.295779513m);
            AddBuiltIn(EUnit.DegreesPerMinute, "Degrees per Minute", new[] { "°/min", "d/min" }, EUnitCategory.AngularVelocity, 3437.7467708m);
            AddBuiltIn(EUnit.DegreesPerHour, "Degrees per Hour", new[] { "°/h", "d/h" }, EUnitCategory.AngularVelocity, 206264.80625m);
            AddBuiltIn(EUnit.DegreesPerDay, "Degrees per Day", new[] { "°/d", "d/d" }, EUnitCategory.AngularVelocity, 4950355.3499m);
            AddBuiltIn(EUnit.RevolutionsPerSecond, "Revolutions per Second", new[] { "rps" }, EUnitCategory.AngularVelocity, 0.1591549431m);
            AddBuiltIn(EUnit.RevolutionsPerMinute, "Revolutions per Minute", new[] { "rpm" }, EUnitCategory.AngularVelocity, 9.5492965855m);
            AddBuiltIn(EUnit.RevolutionsPerHour, "Revolutions per Hour", new[] { "rph" }, EUnitCategory.AngularVelocity, 572.95779513m);
            AddBuiltIn(EUnit.RevolutionsPerDay, "Revolutions per Day", new[] { "rpd" }, EUnitCategory.AngularVelocity, 13750.987083m);
            AddBuiltIn(EUnit.Hertz, "Hertz", new[] { "Hz" }, EUnitCategory.Frequency, 1.0m);
            AddBuiltIn(EUnit.Kilohertz, "Kilohertz", new[] { "kHz" }, EUnitCategory.Frequency, 0.001m);
            AddBuiltIn(EUnit.Megahertz, "Megahertz", new[] { "MHz" }, EUnitCategory.Frequency, 0.000001m);
            AddBuiltIn(EUnit.Gigahertz, "Gigahertz", new[] { "GHz" }, EUnitCategory.Frequency, 0.000000001m);
            AddBuiltIn(EUnit.PercentMultiplier, "Percent Multiplier", new[] { "%m" }, EUnitCategory.Percent, 1.0m);
            AddBuiltIn(EUnit.Percent, "Percent", new[] { "%" }, EUnitCategory.Percent, 100.0m);
            AddBuiltIn(EUnit.Permille, "Permille", new[] { "‰" }, EUnitCategory.Percent, 1000.0m);
            AddBuiltIn(EUnit.Permyriad, "Permyriad", new[] { "‱" }, EUnitCategory.Percent, 10000.0m);
        }

        public static UnitInfo AddCustomUnit(string name, IEnumerable<string> symbols,
            EUnitCategory category, decimal categoryBaseToUnitMultiplier)
        {
            if (categoryBaseToUnitMultiplier == 0)
            {
                throw new ArgumentOutOfRangeException(nameof(categoryBaseToUnitMultiplier),
                    "Unit multiplier cannot be zero.");
            }

            return AddCustomUnit(name, symbols, category,
                value => value / categoryBaseToUnitMultiplier,
                value => value * categoryBaseToUnitMultiplier);
        }

        // ReSharper disable once MemberCanBePrivate.Global
        public static UnitInfo AddCustomUnit(string name, IEnumerable<string> symbols,
            EUnitCategory category, Func<decimal, decimal> convertToCategoryBase,
            Func<decimal, decimal> convertFromCategoryBase)
        {
            if (convertToCategoryBase == null)
            {
                throw new ArgumentNullException(nameof(convertToCategoryBase));
            }

            if (convertFromCategoryBase == null)
            {
                throw new ArgumentNullException(nameof(convertFromCategoryBase));
            }

            string[] symbolArray = ValidateNameAndSymbols(name, symbols);
            UnitInfo unitInfo = new UnitInfo(name, symbolArray, category,
                convertToCategoryBase, convertFromCategoryBase);
            Add(unitInfo);
            return unitInfo;
        }

        public static (bool found, UnitInfo result) GetUnitInfo(EUnit unit) => UnitToInfo.ContainsKey(unit)
            ? (true, UnitToInfo[unit])
            : (false, null);

        public static (bool found, UnitInfo result) GetUnitInfo(string name)
        {
            string key = name ?? "";
            return NameToInfo.ContainsKey(key)
                ? (true, NameToInfo[key])
                : (false, null);
        }

        public static IReadOnlyList<UnitInfo> GetAllUnitInfos(EUnitCategory category) =>
            AllUnits.Where(each => each.Category == category).ToArray();

        public static (bool success, decimal result, string error) Convert(decimal value, UnitInfo from, UnitInfo to)
        {
            if (from == null || to == null)
            {
                return (false, value, "Unit is not registered.");
            }

            if (from.Category != to.Category)
            {
                return (false, value,
                    $"Cannot convert {from.Name} ({from.Category}) to {to.Name} ({to.Category}).");
            }

            try
            {
                return (true, to.FromCategoryBase(from.ToCategoryBase(value)), "");
            }
            catch (Exception exception)
            {
                return (false, value, $"Failed to convert {from.Name} to {to.Name}: {exception.Message}");
            }
        }

        private static void AddBuiltIn(EUnit unit, string name, string[] symbols, EUnitCategory category,
            decimal categoryBaseToUnitMultiplier)
        {
            UnitInfo unitInfo = new UnitInfo(name, symbols, category,
                value => value / categoryBaseToUnitMultiplier,
                value => value * categoryBaseToUnitMultiplier);
            UnitToInfo.Add(unit, unitInfo);
            Add(unitInfo);
        }

        private static void AddBuiltIn(EUnit unit, string name, string[] symbols, EUnitCategory category,
            Func<decimal, decimal> convertToCategoryBase, Func<decimal, decimal> convertFromCategoryBase)
        {
            UnitInfo unitInfo = new UnitInfo(name, symbols, category,
                convertToCategoryBase, convertFromCategoryBase);
            UnitToInfo.Add(unit, unitInfo);
            Add(unitInfo);
        }

        private static void Add(UnitInfo unitInfo)
        {
            if (NameToInfo.ContainsKey(unitInfo.Name))
            {
                throw new ArgumentException($"A unit named '{unitInfo.Name}' is already registered.", nameof(unitInfo));
            }

            foreach (string symbol in unitInfo.Symbols)
            {
                if (AllUnits.Any(each => each.Category == unitInfo.Category &&
                                         each.Symbols.Any(existing => string.Equals(existing, symbol,
                                             StringComparison.Ordinal))))
                {
                    throw new ArgumentException(
                        $"The symbol '{symbol}' is already registered in {unitInfo.Category}.", nameof(unitInfo));
                }
            }

            NameToInfo.Add(unitInfo.Name, unitInfo);
            AllUnits.Add(unitInfo);
        }

        private static string[] ValidateNameAndSymbols(string name, IEnumerable<string> symbols)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException("Unit name cannot be empty.", nameof(name));
            }

            string[] result = symbols?.ToArray() ?? Array.Empty<string>();
            if (result.Any(string.IsNullOrWhiteSpace))
            {
                throw new ArgumentException("Unit symbols cannot be empty.", nameof(symbols));
            }

            return result;
        }
    }
}
