using SaintsField.Playa;
using UnityEngine;

namespace SaintsField.Samples.Scripts.SaintsEditor.Testing
{
    public class PropRangeFix : SaintsMonoBehaviour
    {
        [PropRange(1, 3)] public int serializedInt = 2;
        [PropRange(1, 3)] public uint serializedUInt = 2;
        [PropRange(1, 3)] public long serializedLong = 2;
        [PropRange(1, 3)] public ulong serializedULong = 2;
        [PropRange(1, 3)] public float serializedFloat = 2;
        [PropRange(1, 3)] public double serializedDouble = 2;

        [Button]
        private void CheckButtonParameters(
            [PropRange(1, 3)] int intValue = 2,
            [PropRange(1, 3)] uint uintValue = 2,
            [PropRange(1, 3)] long longValue = 2,
            [PropRange(1, 3)] ulong ulongValue = 2,
            [PropRange(1, 3)] float floatValue = 2,
            [PropRange(1, 3)] double doubleValue = 2)
        {
            Debug.Log($"PropRange button values: int={intValue}, uint={uintValue}, long={longValue}, " +
                      $"ulong={ulongValue}, float={floatValue}, double={doubleValue}", this);
        }
    }
}
