namespace SaintsField
{
    public enum EUnitCategory
    {
        Percent,
        Distance,
        Speed,
        Volume,
        Area,
        Energy,
        Force,
        DataStorage,
        Weight,
        Temperature,
        Pressure,
        Power,
        Time,
        Angle,
        Torque,
        Acceleration,
        AngularVelocity,
        Frequency,

        // If you want to define completely custom unit that should not convert with built-in types
        // use these
        // pure custom category requires you to at least define one with categoryBaseToUnitMultiplier=1m
        Custom0,
        Custom1,
        Custom2,
        Custom3,
        Custom4,
        Custom5,
        Custom6,
        Custom7,
        Custom8,
        Custom9,
    }
}
