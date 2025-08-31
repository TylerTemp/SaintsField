
using System;
#if SAINTSFIELD_AI_NAVIGATION && !SAINTSFIELD_AI_NAVIGATION_DISABLED
using System.Linq;
using System.Collections.Generic;
#endif

namespace SaintsField.Editor.Utils
{
    public static class AiNavigationUtils
    {
#if SAINTSFIELD_AI_NAVIGATION && !SAINTSFIELD_AI_NAVIGATION_DISABLED
        public struct NavMeshArea : IEquatable<NavMeshArea>
        {
            public string Name;
            public int Value;
            public int Mask;

            public bool Equals(NavMeshArea other)
            {
                return Name == other.Name && Value == other.Value && Mask == other.Mask;
            }

            public override bool Equals(object obj)
            {
                return obj is NavMeshArea other && Equals(other);
            }

            public override int GetHashCode()
            {
                return Util.CombineHashCode(Name, Value, Mask);
            }
        }

        public static IEnumerable<NavMeshArea> GetNavMeshAreas() => GetNavMeshAreaNames()
            .Select(name =>
            {
                int areaValue = UnityEngine.AI.NavMesh.GetAreaFromName(name);
                return new NavMeshArea
                {
                    Name = name,
                    Value = areaValue,
                    Mask = 1 << areaValue,
                };
            });

        private static string[] GetNavMeshAreaNames() =>
#if UNITY_6000_0_OR_NEWER
            UnityEngine.AI.NavMesh.GetAreaNames()
#else
            UnityEditor.GameObjectUtility.GetNavMeshAreaNames()
#endif
        ;

        public static int GetAreaFromName(string areaName) =>
#if UNITY_6000_0_OR_NEWER
            UnityEngine.AI.NavMesh.GetAreaFromName(areaName)
#else
            UnityEditor.GameObjectUtility.GetNavMeshAreaFromName(areaName)
#endif
        ;

#endif
    }

}
