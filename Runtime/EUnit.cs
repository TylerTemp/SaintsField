namespace SaintsField
{
    public enum EUnit
    {
        /// <summary>
        /// A unitless ratio where 1 represents 100%. <see cref="EUnitCategory.Percent"/>.
        /// </summary>
        Ratio = 1,
        /// <summary>
        /// <see cref="EUnitCategory.Percent"/>.
        /// </summary>
        Percent,
        /// <summary>
        /// <see cref="EUnitCategory.Distance"/>.
        /// </summary>
        Millimeter,
        /// <summary>
        /// <see cref="EUnitCategory.Distance"/>.
        /// </summary>
        Centimeter,
        /// <summary>
        /// <see cref="EUnitCategory.Distance"/>.
        /// </summary>
        Meter,
        /// <summary>
        /// <see cref="EUnitCategory.Distance"/>.
        /// </summary>
        Kilometer,
        /// <summary>
        /// <see cref="EUnitCategory.Speed"/>.
        /// </summary>
        MetersPerSecond,
        /// <summary>
        /// <see cref="EUnitCategory.Speed"/>.
        /// </summary>
        KilometersPerHour,
        /// <summary>
        /// <see cref="EUnitCategory.Speed"/>.
        /// </summary>
        MilesPerHour,
        /// <summary>
        /// <see cref="EUnitCategory.Distance"/>.
        /// </summary>
        Nanometer,
        /// <summary>
        /// <see cref="EUnitCategory.Distance"/>.
        /// </summary>
        Micrometer,
        /// <summary>
        /// <see cref="EUnitCategory.Distance"/>.
        /// </summary>
        Inch,
        /// <summary>
        /// <see cref="EUnitCategory.Distance"/>.
        /// </summary>
        Feet,
        /// <summary>
        /// <see cref="EUnitCategory.Distance"/>.
        /// </summary>
        Mile,
        /// <summary>
        /// <see cref="EUnitCategory.Distance"/>.
        /// </summary>
        Yard,
        /// <summary>
        /// <see cref="EUnitCategory.Distance"/>.
        /// </summary>
        NauticalMile,
        /// <summary>
        /// <see cref="EUnitCategory.Distance"/>.
        /// </summary>
        LightYear,
        /// <summary>
        /// <see cref="EUnitCategory.Distance"/>.
        /// </summary>
        Parsec,
        /// <summary>
        /// <see cref="EUnitCategory.Distance"/>.
        /// </summary>
        AstronomicalUnit,
        /// <summary>
        /// <see cref="EUnitCategory.Volume"/>.
        /// </summary>
        CubicMeter,
        /// <summary>
        /// <see cref="EUnitCategory.Volume"/>.
        /// </summary>
        CubicKilometer,
        /// <summary>
        /// <see cref="EUnitCategory.Volume"/>.
        /// </summary>
        CubicCentimeter,
        /// <summary>
        /// <see cref="EUnitCategory.Volume"/>.
        /// </summary>
        CubicMillimeter,
        /// <summary>
        /// <see cref="EUnitCategory.Volume"/>.
        /// </summary>
        Liter,
        /// <summary>
        /// <see cref="EUnitCategory.Volume"/>.
        /// </summary>
        Milliliter,
        /// <summary>
        /// <see cref="EUnitCategory.Volume"/>.
        /// </summary>
        Centiliter,
        /// <summary>
        /// <see cref="EUnitCategory.Volume"/>.
        /// </summary>
        Deciliter,
        /// <summary>
        /// <see cref="EUnitCategory.Volume"/>.
        /// </summary>
        Hectoliter,
        /// <summary>
        /// <see cref="EUnitCategory.Volume"/>.
        /// </summary>
        CubicInch,
        /// <summary>
        /// <see cref="EUnitCategory.Volume"/>.
        /// </summary>
        CubicFeet,
        /// <summary>
        /// <see cref="EUnitCategory.Volume"/>.
        /// </summary>
        CubicYard,
        /// <summary>
        /// The volume of one acre covered to a depth of one foot. <see cref="EUnitCategory.Volume"/>.
        /// </summary>
        AcreFeet,
        /// <summary>
        /// A standard oil-barrel volume. <see cref="EUnitCategory.Volume"/>.
        /// </summary>
        BarrelOil,
        /// <summary>
        /// <see cref="EUnitCategory.Volume"/>.
        /// </summary>
        TeaspoonUS,
        /// <summary>
        /// <see cref="EUnitCategory.Volume"/>.
        /// </summary>
        TablespoonUS,
        /// <summary>
        /// <see cref="EUnitCategory.Volume"/>.
        /// </summary>
        CupUS,
        /// <summary>
        /// The US customary gill. <see cref="EUnitCategory.Volume"/>.
        /// </summary>
        GillUS,
        /// <summary>
        /// <see cref="EUnitCategory.Volume"/>.
        /// </summary>
        PintUS,
        /// <summary>
        /// <see cref="EUnitCategory.Volume"/>.
        /// </summary>
        QuartUS,
        /// <summary>
        /// <see cref="EUnitCategory.Volume"/>.
        /// </summary>
        GallonUS,
        /// <summary>
        /// <see cref="EUnitCategory.Volume"/>.
        /// </summary>
        BarrelUS,
        /// <summary>
        /// <see cref="EUnitCategory.Volume"/>.
        /// </summary>
        FluidOunceUS,
        /// <summary>
        /// <see cref="EUnitCategory.Volume"/>.
        /// </summary>
        BarrelUK,
        /// <summary>
        /// <see cref="EUnitCategory.Volume"/>.
        /// </summary>
        FluidOunceUK,
        /// <summary>
        /// <see cref="EUnitCategory.Volume"/>.
        /// </summary>
        TeaspoonUK,
        /// <summary>
        /// <see cref="EUnitCategory.Volume"/>.
        /// </summary>
        TablespoonUK,
        /// <summary>
        /// <see cref="EUnitCategory.Volume"/>.
        /// </summary>
        CupUK,
        /// <summary>
        /// The imperial gill. <see cref="EUnitCategory.Volume"/>.
        /// </summary>
        GillUK,
        /// <summary>
        /// <see cref="EUnitCategory.Volume"/>.
        /// </summary>
        PintUK,
        /// <summary>
        /// <see cref="EUnitCategory.Volume"/>.
        /// </summary>
        QuartUK,
        /// <summary>
        /// <see cref="EUnitCategory.Volume"/>.
        /// </summary>
        GallonUK,
        /// <summary>
        /// <see cref="EUnitCategory.Area"/>.
        /// </summary>
        SquareMeter,
        /// <summary>
        /// <see cref="EUnitCategory.Area"/>.
        /// </summary>
        SquareKilometer,
        /// <summary>
        /// <see cref="EUnitCategory.Area"/>.
        /// </summary>
        SquareCentimeter,
        /// <summary>
        /// <see cref="EUnitCategory.Area"/>.
        /// </summary>
        SquareMillimeter,
        /// <summary>
        /// <see cref="EUnitCategory.Area"/>.
        /// </summary>
        SquareMicrometer,
        /// <summary>
        /// <see cref="EUnitCategory.Area"/>.
        /// </summary>
        SquareInch,
        /// <summary>
        /// <see cref="EUnitCategory.Area"/>.
        /// </summary>
        SquareFeet,
        /// <summary>
        /// <see cref="EUnitCategory.Area"/>.
        /// </summary>
        SquareYard,
        /// <summary>
        /// <see cref="EUnitCategory.Area"/>.
        /// </summary>
        SquareMile,
        /// <summary>
        /// <see cref="EUnitCategory.Area"/>.
        /// </summary>
        Hectare,
        /// <summary>
        /// <see cref="EUnitCategory.Area"/>.
        /// </summary>
        Acre,
        /// <summary>
        /// An area of 100 square meters. <see cref="EUnitCategory.Area"/>.
        /// </summary>
        Are,
        /// <summary>
        /// <see cref="EUnitCategory.Energy"/>.
        /// </summary>
        Joule,
        /// <summary>
        /// <see cref="EUnitCategory.Energy"/>.
        /// </summary>
        Kilojoule,
        /// <summary>
        /// <see cref="EUnitCategory.Energy"/>.
        /// </summary>
        WattHour,
        /// <summary>
        /// <see cref="EUnitCategory.Energy"/>.
        /// </summary>
        KilowattHour,
        /// <summary>
        /// <see cref="EUnitCategory.Energy"/>.
        /// </summary>
        HorsepowerHour,
        /// <summary>
        /// <see cref="EUnitCategory.Force"/>.
        /// </summary>
        Newton,
        /// <summary>
        /// <see cref="EUnitCategory.Force"/>.
        /// </summary>
        Kilonewton,
        /// <summary>
        /// <see cref="EUnitCategory.Force"/>.
        /// </summary>
        Meganewton,
        /// <summary>
        /// <see cref="EUnitCategory.Force"/>.
        /// </summary>
        Giganewton,
        /// <summary>
        /// <see cref="EUnitCategory.Force"/>.
        /// </summary>
        Teranewton,
        /// <summary>
        /// <see cref="EUnitCategory.Force"/>.
        /// </summary>
        Centinewton,
        /// <summary>
        /// <see cref="EUnitCategory.Force"/>.
        /// </summary>
        Millinewton,
        /// <summary>
        /// Joules per meter. <see cref="EUnitCategory.Force"/>.
        /// </summary>
        JouleMeter,
        /// <summary>
        /// Joules per centimeter. <see cref="EUnitCategory.Force"/>.
        /// </summary>
        JouleCentimeter,
        /// <summary>
        /// <see cref="EUnitCategory.Force"/>.
        /// </summary>
        GramForce,
        /// <summary>
        /// <see cref="EUnitCategory.Force"/>.
        /// </summary>
        KilogramForce,
        /// <summary>
        /// <see cref="EUnitCategory.Force"/>.
        /// </summary>
        TonForce,
        /// <summary>
        /// <see cref="EUnitCategory.Force"/>.
        /// </summary>
        PoundForce,
        /// <summary>
        /// <see cref="EUnitCategory.Force"/>.
        /// </summary>
        KilopoundForce,
        /// <summary>
        /// <see cref="EUnitCategory.Force"/>.
        /// </summary>
        OunceForce,
        /// <summary>
        /// <see cref="EUnitCategory.Speed"/>.
        /// </summary>
        MetersPerMinute,
        /// <summary>
        /// <see cref="EUnitCategory.Speed"/>.
        /// </summary>
        MetersPerHour,
        /// <summary>
        /// <see cref="EUnitCategory.Speed"/>.
        /// </summary>
        KilometersPerSecond,
        /// <summary>
        /// <see cref="EUnitCategory.Speed"/>.
        /// </summary>
        KilometersPerMinute,
        /// <summary>
        /// <see cref="EUnitCategory.Speed"/>.
        /// </summary>
        CentimetersPerSecond,
        /// <summary>
        /// <see cref="EUnitCategory.Speed"/>.
        /// </summary>
        CentimetersPerMinute,
        /// <summary>
        /// <see cref="EUnitCategory.Speed"/>.
        /// </summary>
        CentimetersPerHour,
        /// <summary>
        /// <see cref="EUnitCategory.Speed"/>.
        /// </summary>
        MillimetersPerSecond,
        /// <summary>
        /// <see cref="EUnitCategory.Speed"/>.
        /// </summary>
        MillimetersPerMinute,
        /// <summary>
        /// <see cref="EUnitCategory.Speed"/>.
        /// </summary>
        MillimetersPerHour,
        /// <summary>
        /// <see cref="EUnitCategory.Speed"/>.
        /// </summary>
        FeetPerSecond,
        /// <summary>
        /// <see cref="EUnitCategory.Speed"/>.
        /// </summary>
        FeetPerMinute,
        /// <summary>
        /// <see cref="EUnitCategory.Speed"/>.
        /// </summary>
        FeetPerHour,
        /// <summary>
        /// <see cref="EUnitCategory.Speed"/>.
        /// </summary>
        YardsPerSecond,
        /// <summary>
        /// <see cref="EUnitCategory.Speed"/>.
        /// </summary>
        YardsPerMinute,
        /// <summary>
        /// <see cref="EUnitCategory.Speed"/>.
        /// </summary>
        YardsPerHour,
        /// <summary>
        /// <see cref="EUnitCategory.Speed"/>.
        /// </summary>
        MilesPerSecond,
        /// <summary>
        /// <see cref="EUnitCategory.Speed"/>.
        /// </summary>
        MilesPerMinute,
        /// <summary>
        /// International nautical miles per hour. <see cref="EUnitCategory.Speed"/>.
        /// </summary>
        Knot,
        /// <summary>
        /// The UK knot. <see cref="EUnitCategory.Speed"/>.
        /// </summary>
        KnotUK,
        /// <summary>
        /// The speed of light in vacuum. <see cref="EUnitCategory.Speed"/>.
        /// </summary>
        SpeedOfLight,
        /// <summary>
        /// <see cref="EUnitCategory.DataStorage"/>.
        /// </summary>
        Bit,
        /// <summary>
        /// <see cref="EUnitCategory.DataStorage"/>.
        /// </summary>
        Kilobit,
        /// <summary>
        /// <see cref="EUnitCategory.DataStorage"/>.
        /// </summary>
        Megabit,
        /// <summary>
        /// <see cref="EUnitCategory.DataStorage"/>.
        /// </summary>
        Gigabit,
        /// <summary>
        /// <see cref="EUnitCategory.DataStorage"/>.
        /// </summary>
        Terabit,
        /// <summary>
        /// <see cref="EUnitCategory.DataStorage"/>.
        /// </summary>
        Petabit,
        /// <summary>
        /// <see cref="EUnitCategory.DataStorage"/>.
        /// </summary>
        Byte,
        /// <summary>
        /// <see cref="EUnitCategory.DataStorage"/>.
        /// </summary>
        Kilobyte,
        /// <summary>
        /// <see cref="EUnitCategory.DataStorage"/>.
        /// </summary>
        Kibibyte,
        /// <summary>
        /// <see cref="EUnitCategory.DataStorage"/>.
        /// </summary>
        Megabyte,
        /// <summary>
        /// <see cref="EUnitCategory.DataStorage"/>.
        /// </summary>
        Mebibyte,
        /// <summary>
        /// <see cref="EUnitCategory.DataStorage"/>.
        /// </summary>
        Gigabyte,
        /// <summary>
        /// <see cref="EUnitCategory.DataStorage"/>.
        /// </summary>
        Gibibyte,
        /// <summary>
        /// <see cref="EUnitCategory.DataStorage"/>.
        /// </summary>
        Terabyte,
        /// <summary>
        /// <see cref="EUnitCategory.DataStorage"/>.
        /// </summary>
        Tebibyte,
        /// <summary>
        /// <see cref="EUnitCategory.DataStorage"/>.
        /// </summary>
        Petabyte,
        /// <summary>
        /// <see cref="EUnitCategory.DataStorage"/>.
        /// </summary>
        Pebibyte,
        /// <summary>
        /// <see cref="EUnitCategory.Weight"/>.
        /// </summary>
        Kilogram,
        /// <summary>
        /// <see cref="EUnitCategory.Weight"/>.
        /// </summary>
        Hectogram,
        /// <summary>
        /// <see cref="EUnitCategory.Weight"/>.
        /// </summary>
        Dekagram,
        /// <summary>
        /// <see cref="EUnitCategory.Weight"/>.
        /// </summary>
        Gram,
        /// <summary>
        /// <see cref="EUnitCategory.Weight"/>.
        /// </summary>
        Decigram,
        /// <summary>
        /// <see cref="EUnitCategory.Weight"/>.
        /// </summary>
        Centigram,
        /// <summary>
        /// <see cref="EUnitCategory.Weight"/>.
        /// </summary>
        Milligram,
        /// <summary>
        /// <see cref="EUnitCategory.Weight"/>.
        /// </summary>
        MetricTon,
        /// <summary>
        /// <see cref="EUnitCategory.Weight"/>.
        /// </summary>
        Pounds,
        /// <summary>
        /// The US short ton. <see cref="EUnitCategory.Weight"/>.
        /// </summary>
        ShortTon,
        /// <summary>
        /// The imperial long ton. <see cref="EUnitCategory.Weight"/>.
        /// </summary>
        LongTon,
        /// <summary>
        /// <see cref="EUnitCategory.Weight"/>.
        /// </summary>
        Ounce,
        /// <summary>
        /// The US stone. <see cref="EUnitCategory.Weight"/>.
        /// </summary>
        StoneUS,
        /// <summary>
        /// The imperial stone. <see cref="EUnitCategory.Weight"/>.
        /// </summary>
        StoneUK,
        /// <summary>
        /// The US quarter-weight. <see cref="EUnitCategory.Weight"/>.
        /// </summary>
        QuarterUS,
        /// <summary>
        /// The imperial quarter-weight. <see cref="EUnitCategory.Weight"/>.
        /// </summary>
        QuarterUK,
        /// <summary>
        /// The imperial mass unit known as a slug. <see cref="EUnitCategory.Weight"/>.
        /// </summary>
        Slug,
        /// <summary>
        /// The grain mass unit. <see cref="EUnitCategory.Weight"/>.
        /// </summary>
        Grain,
        /// <summary>
        /// <see cref="EUnitCategory.Temperature"/>.
        /// </summary>
        Celsius,
        /// <summary>
        /// <see cref="EUnitCategory.Temperature"/>.
        /// </summary>
        Fahrenheit,
        /// <summary>
        /// <see cref="EUnitCategory.Temperature"/>.
        /// </summary>
        Kelvin,
        /// <summary>
        /// <see cref="EUnitCategory.Pressure"/>.
        /// </summary>
        Pascal,
        /// <summary>
        /// <see cref="EUnitCategory.Pressure"/>.
        /// </summary>
        Decipascal,
        /// <summary>
        /// <see cref="EUnitCategory.Pressure"/>.
        /// </summary>
        Centipascal,
        /// <summary>
        /// <see cref="EUnitCategory.Pressure"/>.
        /// </summary>
        Millipascal,
        /// <summary>
        /// <see cref="EUnitCategory.Pressure"/>.
        /// </summary>
        Micropascal,
        /// <summary>
        /// <see cref="EUnitCategory.Pressure"/>.
        /// </summary>
        Kilopascal,
        /// <summary>
        /// <see cref="EUnitCategory.Pressure"/>.
        /// </summary>
        Megapascal,
        /// <summary>
        /// <see cref="EUnitCategory.Pressure"/>.
        /// </summary>
        Gigapascal,
        /// <summary>
        /// <see cref="EUnitCategory.Pressure"/>.
        /// </summary>
        Bar,
        /// <summary>
        /// <see cref="EUnitCategory.Pressure"/>.
        /// </summary>
        Millibar,
        /// <summary>
        /// <see cref="EUnitCategory.Pressure"/>.
        /// </summary>
        Microbar,
        /// <summary>
        /// Pounds per square inch. <see cref="EUnitCategory.Pressure"/>.
        /// </summary>
        PSI,
        /// <summary>
        /// Kips per square inch. <see cref="EUnitCategory.Pressure"/>.
        /// </summary>
        KSI,
        /// <summary>
        /// <see cref="EUnitCategory.Pressure"/>.
        /// </summary>
        StandardAtmosphere,
        /// <summary>
        /// <see cref="EUnitCategory.Power"/>.
        /// </summary>
        Watt,
        /// <summary>
        /// <see cref="EUnitCategory.Power"/>.
        /// </summary>
        Kilowatt,
        /// <summary>
        /// <see cref="EUnitCategory.Power"/>.
        /// </summary>
        Megawatt,
        /// <summary>
        /// <see cref="EUnitCategory.Power"/>.
        /// </summary>
        Gigawatt,
        /// <summary>
        /// <see cref="EUnitCategory.Power"/>.
        /// </summary>
        Terawatt,
        /// <summary>
        /// <see cref="EUnitCategory.Power"/>.
        /// </summary>
        Horsepower,
        /// <summary>
        /// Joules per second. <see cref="EUnitCategory.Power"/>.
        /// </summary>
        JouleSecond,
        /// <summary>
        /// Joules per minute. <see cref="EUnitCategory.Power"/>.
        /// </summary>
        JouleMinute,
        /// <summary>
        /// Joules per hour. <see cref="EUnitCategory.Power"/>.
        /// </summary>
        JouleHour,
        /// <summary>
        /// Kilojoules per second. <see cref="EUnitCategory.Power"/>.
        /// </summary>
        KilojouleSecond,
        /// <summary>
        /// Kilojoules per minute. <see cref="EUnitCategory.Power"/>.
        /// </summary>
        KilojouleMinute,
        /// <summary>
        /// Kilojoules per hour. <see cref="EUnitCategory.Power"/>.
        /// </summary>
        KilojouleHour,
        /// <summary>
        /// <see cref="EUnitCategory.Time"/>.
        /// </summary>
        Second,
        /// <summary>
        /// <see cref="EUnitCategory.Time"/>.
        /// </summary>
        Millisecond,
        /// <summary>
        /// <see cref="EUnitCategory.Time"/>.
        /// </summary>
        Microsecond,
        /// <summary>
        /// <see cref="EUnitCategory.Time"/>.
        /// </summary>
        Nanosecond,
        /// <summary>
        /// <see cref="EUnitCategory.Time"/>.
        /// </summary>
        Minute,
        /// <summary>
        /// <see cref="EUnitCategory.Time"/>.
        /// </summary>
        Hour,
        /// <summary>
        /// <see cref="EUnitCategory.Time"/>.
        /// </summary>
        Day,
        /// <summary>
        /// <see cref="EUnitCategory.Time"/>.
        /// </summary>
        Week,
        /// <summary>
        /// <see cref="EUnitCategory.Angle"/>.
        /// </summary>
        Radian,
        /// <summary>
        /// <see cref="EUnitCategory.Angle"/>.
        /// </summary>
        Degree,
        /// <summary>
        /// One complete revolution. <see cref="EUnitCategory.Angle"/>.
        /// </summary>
        Turn,
        /// <summary>
        /// The gradian angle unit. <see cref="EUnitCategory.Angle"/>.
        /// </summary>
        Grad,
        /// <summary>
        /// Arcseconds. <see cref="EUnitCategory.Angle"/>.
        /// </summary>
        SecondsOfAngle,
        /// <summary>
        /// Arcminutes (MOA). <see cref="EUnitCategory.Angle"/>.
        /// </summary>
        MinutesOfAngle,
        /// <summary>
        /// The angular mil. <see cref="EUnitCategory.Angle"/>.
        /// </summary>
        Mil,
        /// <summary>
        /// <see cref="EUnitCategory.Acceleration"/>.
        /// </summary>
        MetersPerSecondSquared,
        /// <summary>
        /// <see cref="EUnitCategory.Acceleration"/>.
        /// </summary>
        DecimetersPerSecondSquared,
        /// <summary>
        /// <see cref="EUnitCategory.Acceleration"/>.
        /// </summary>
        CentimetersPerSecondSquared,
        /// <summary>
        /// <see cref="EUnitCategory.Acceleration"/>.
        /// </summary>
        MillimetersPerSecondSquared,
        /// <summary>
        /// <see cref="EUnitCategory.Acceleration"/>.
        /// </summary>
        MicrometersPerSecondSquared,
        /// <summary>
        /// <see cref="EUnitCategory.Acceleration"/>.
        /// </summary>
        DekametersPerSecondSquared,
        /// <summary>
        /// <see cref="EUnitCategory.Acceleration"/>.
        /// </summary>
        HectometersPerSecondSquared,
        /// <summary>
        /// <see cref="EUnitCategory.Acceleration"/>.
        /// </summary>
        KilometersPerSecondSquared,
        /// <summary>
        /// <see cref="EUnitCategory.Acceleration"/>.
        /// </summary>
        MilePerSecondSquared,
        /// <summary>
        /// <see cref="EUnitCategory.Acceleration"/>.
        /// </summary>
        YardPerSecondSquared,
        /// <summary>
        /// <see cref="EUnitCategory.Acceleration"/>.
        /// </summary>
        FeetPerSecondSquared,
        /// <summary>
        /// <see cref="EUnitCategory.Acceleration"/>.
        /// </summary>
        InchPerSecondSquared,
        /// <summary>
        /// Acceleration expressed in standard gravity. <see cref="EUnitCategory.Acceleration"/>.
        /// </summary>
        GForce,
        /// <summary>
        /// <see cref="EUnitCategory.Torque"/>.
        /// </summary>
        NewtonMeter,
        /// <summary>
        /// <see cref="EUnitCategory.Torque"/>.
        /// </summary>
        NewtonCentimeter,
        /// <summary>
        /// <see cref="EUnitCategory.Torque"/>.
        /// </summary>
        NewtonMillimeter,
        /// <summary>
        /// <see cref="EUnitCategory.Torque"/>.
        /// </summary>
        KilonewtonMeter,
        /// <summary>
        /// <see cref="EUnitCategory.Torque"/>.
        /// </summary>
        KilogramForceMeter,
        /// <summary>
        /// <see cref="EUnitCategory.Torque"/>.
        /// </summary>
        KilogramForceCentimeter,
        /// <summary>
        /// <see cref="EUnitCategory.Torque"/>.
        /// </summary>
        KilogramForceMillimeter,
        /// <summary>
        /// <see cref="EUnitCategory.Torque"/>.
        /// </summary>
        GramForceMeter,
        /// <summary>
        /// <see cref="EUnitCategory.Torque"/>.
        /// </summary>
        GramForceCentimeter,
        /// <summary>
        /// <see cref="EUnitCategory.Torque"/>.
        /// </summary>
        GramForceMillimeter,
        /// <summary>
        /// Pound-force feet. <see cref="EUnitCategory.Torque"/>.
        /// </summary>
        PoundFeet,
        /// <summary>
        /// Pound-force inches. <see cref="EUnitCategory.Torque"/>.
        /// </summary>
        PoundInch,
        /// <summary>
        /// Ounce-force feet. <see cref="EUnitCategory.Torque"/>.
        /// </summary>
        OuncecFeet,
        /// <summary>
        /// Ounce-force inches. <see cref="EUnitCategory.Torque"/>.
        /// </summary>
        OuncecInch,
        /// <summary>
        /// <see cref="EUnitCategory.AngularVelocity"/>.
        /// </summary>
        RadiansPerSecond,
        /// <summary>
        /// <see cref="EUnitCategory.AngularVelocity"/>.
        /// </summary>
        RadiansPerMinute,
        /// <summary>
        /// <see cref="EUnitCategory.AngularVelocity"/>.
        /// </summary>
        RadiansPerHour,
        /// <summary>
        /// <see cref="EUnitCategory.AngularVelocity"/>.
        /// </summary>
        RadiansPerDay,
        /// <summary>
        /// <see cref="EUnitCategory.AngularVelocity"/>.
        /// </summary>
        DegreesPerSecond,
        /// <summary>
        /// <see cref="EUnitCategory.AngularVelocity"/>.
        /// </summary>
        DegreesPerMinute,
        /// <summary>
        /// <see cref="EUnitCategory.AngularVelocity"/>.
        /// </summary>
        DegreesPerHour,
        /// <summary>
        /// <see cref="EUnitCategory.AngularVelocity"/>.
        /// </summary>
        DegreesPerDay,
        /// <summary>
        /// <see cref="EUnitCategory.AngularVelocity"/>.
        /// </summary>
        RevolutionsPerSecond,
        /// <summary>
        /// <see cref="EUnitCategory.AngularVelocity"/>.
        /// </summary>
        RevolutionsPerMinute,
        /// <summary>
        /// <see cref="EUnitCategory.AngularVelocity"/>.
        /// </summary>
        RevolutionsPerHour,
        /// <summary>
        /// <see cref="EUnitCategory.AngularVelocity"/>.
        /// </summary>
        RevolutionsPerDay,
        /// <summary>
        /// <see cref="EUnitCategory.Frequency"/>.
        /// </summary>
        Hertz,
        /// <summary>
        /// <see cref="EUnitCategory.Frequency"/>.
        /// </summary>
        Kilohertz,
        /// <summary>
        /// <see cref="EUnitCategory.Frequency"/>.
        /// </summary>
        Megahertz,
        /// <summary>
        /// <see cref="EUnitCategory.Frequency"/>.
        /// </summary>
        Gigahertz,
        /// <summary>
        /// A percent multiplier using ratio scale, where 1 represents 100%. <see cref="EUnitCategory.Percent"/>.
        /// </summary>
        PercentMultiplier,
        /// <summary>
        /// Parts per thousand. <see cref="EUnitCategory.Percent"/>.
        /// </summary>
        Permille,
        /// <summary>
        /// Parts per ten thousand. <see cref="EUnitCategory.Percent"/>.
        /// </summary>
        Permyriad,
    }
}
