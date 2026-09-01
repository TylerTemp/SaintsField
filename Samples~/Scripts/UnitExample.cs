#if UNITY_EDITOR
using SaintsField.Editor.Units;
using UnityEditor;
#endif

using UnityEngine;

namespace SaintsField.Samples.Scripts
{
    public class UnitExample : SaintsMonoBehaviour
    {
        // saved as meter
        [Unit(EUnit.Meter)] public float distanceInMeters;
        // saved as degree, default displayed as radian
        [Unit(EUnit.Degree, EUnit.Radian)] public float angle;

        [Header("Custom Unit")]

        [InfoBox("Game Tile is registered below in this file. The value remains serialized in meters.")]
        [Unit(EUnit.Meter, "Game Tile")]
        public float customDistanceInMeters = 1f;

#if UNITY_EDITOR && SAINTSFIELD_DEBUG
        [InitializeOnLoadMethod]
        public static void Register()
        {
            if (!UnitRegistry.GetUnitInfo("Game Tile").found)
            {
                UnitRegistry.AddCustomUnit(
                    "Game Tile",  // name
                    new[] { "tile" },  // symbols
                    EUnitCategory.Distance,  // category, see https://github.com/TylerTemp/SaintsField/Runtime/EUnitCategory.cs
                    2m  // multiplier: how the base unit convert to this
                );
            }
        }
#endif
    }
}
