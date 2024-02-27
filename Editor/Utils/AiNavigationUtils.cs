#if SAINTSFIELD_AI_NAVIGATION
using System.Linq;
using System.Collections.Generic;
using UnityEditor;
#endif

namespace SaintsField.Editor.Utils
{
#if SAINTSFIELD_AI_NAVIGATION
    public static class AiNavigationUtils
    {
        public struct NavMeshArea
        {
            public string Name;
            public int Value;
            public int Mask;
        }

        public static IEnumerable<NavMeshArea> GetNavMeshAreas() => GameObjectUtility
            .GetNavMeshAreaNames()
            .Select((name, index) =>
            {
                int areaValue = GameObjectUtility.GetNavMeshAreaFromName(name);
                return new NavMeshArea
                {
                    Name = name,
                    Value = areaValue,
                    Mask = 1 << areaValue,
                };
            });
    }
#endif
}
