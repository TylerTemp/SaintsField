using UnityEngine;

namespace SaintsField.GizmosDraw
{
    public static class Circle
    {
        public static void Draw(Vector3 center, float radius, Vector3 upward, int numSegments = 40)
        {
#if UNITY_EDITOR
            Arc.Draw(center, radius, 0, 360, upward, numSegments);
#endif
        }
    }
}
