using UnityEngine;
using SaintsField.Playa;

#if UNITY_EDITOR
using SaintsField.Editor.Units;
using UnityEditor;
#endif

namespace SaintsField.Samples.Scripts.IssueAndTesting.Testing
{
    public partial class AdaptUnitDemo : SaintsMonoBehaviour
    {
        [Header("Unit Alone")]
        [Tooltip("Serialized and initially displayed in meters. The unit button can switch to any Distance unit.")]
        [Unit(EUnit.Meter)]
        public float distanceInMeters = 1.25f;

        [Tooltip("Serialized in meters per second; initially displayed in kilometers per hour.")]
        [Unit(EUnit.MetersPerSecond, EUnit.KilometersPerHour)]
        public float speedInMetersPerSecond = 10f;

        [Tooltip("Serialized in centimeters; initially displayed in meters.")]
        [Unit(EUnit.Centimeter, EUnit.Meter)]
        public float lengthInCentimeters = 250f;

        [Tooltip("Percent is a unit category: this is serialized as a 0-1 ratio and displayed as percent.")]
        [Unit(EUnit.Ratio, EUnit.Percent)]
        public float completionRatio = 0.5f;

        [Tooltip("Temperature uses affine conversion, not only a multiplier.")]
        [Unit(EUnit.Celsius, EUnit.Fahrenheit)]
        public float temperatureInCelsius = 20f;

        [Tooltip("The selector contains every registered Weight unit.")]
        [Unit(EUnit.Kilogram, EUnit.Pounds)]
        public double weightInKilograms = 2.5;

        [Tooltip("Data storage demonstrates another Odin-compatible unit category.")]
        [Unit(EUnit.Byte, EUnit.Mebibyte)]
        public long fileSizeInBytes = 1048576;

        [ShowInInspector, Unit(EUnit.Watt, EUnit.Horsepower)]
        public float InspectorPower
        {
            get => inspectorPowerInWatts;
            set => inspectorPowerInWatts = value;
        }

        [SerializeField] private float inspectorPowerInWatts = 745.6999f;

        [Header("Works With Other Drawers")]
        [Tooltip("PropRange bounds and storage use meters; the inspector initially displays centimeters.")]
        [PropRange(0f, 10f, 0.25f), Adapt(EUnit.Meter, EUnit.Centimeter)]
        public float rangedDistanceInMeters = 1.5f;

        [Tooltip("MinMaxSlider values are stored as ratios; the inspector initially displays percent.")]
        [MinMaxSlider(0f, 1f, 0.05f), Adapt(EUnit.Ratio, EUnit.Percent)]
        public Vector2 normalizedWindow = new Vector2(0.25f, 0.75f);

        [Tooltip("SaintsDecimal is serialized in meters and edited in centimeters.")]
        [Adapt(EUnit.Meter, EUnit.Centimeter)]
        public SaintsDecimal preciseDistanceInMeters = new SaintsDecimal(1.2345m);

        [SaintsSerialized, Adapt(EUnit.Kilogram, EUnit.Pounds)]
        private decimal extendedWeightInKilograms = 3.75m;
    }


}
