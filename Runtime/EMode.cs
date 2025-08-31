using System;

namespace SaintsField
{
    [Flags]
    public enum EMode
    {
        Edit = 1,
        Play = 1 << 1,
        InstanceInScene = 1 << 2,
        InstanceInPrefab = 1 << 3,
        Regular = 1 << 4,
        Variant = 1 << 5,
        NonPrefabInstance = 1 << 6,

        PrefabInstance = InstanceInPrefab | InstanceInScene,
        PrefabAsset = Variant | Regular,
    }

    // public static class EModeExtensions
    // {
    //     public static bool HasFlagFast(this EMode lhs, EMode rhs) => (lhs & rhs) != 0;
    // }
}
