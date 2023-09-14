using System.Linq;
using UnityEngine;
using System.Collections.Generic;

namespace ExtInspector.GizmosDraw
{
    public static class Arc
    {
        public static void Draw(Vector3 center, float radius, float fromArc, float toArc, Vector3 upward, int numSegments = 40)
        {
#if UNITY_EDITOR
            Vector3 vertexStart = new Vector3(-radius, 0.0f, 0);

            List<Vector3> points = new List<Vector3>(FloatRange(fromArc, toArc, numSegments)
                .Select(angleOffset => center + Quaternion.AngleAxis(angleOffset, upward) * vertexStart));

            for (int index = 0; index < points.Count - 1; index++)
            {
                Vector3 curPoint = points[index];
                Vector3 nextPoint = points[index + 1];
                Gizmos.DrawLine(curPoint, nextPoint);
            }
#endif
        }

#if UNITY_EDITOR
        private static IEnumerable<float> FloatRange(float min, float max, int sep)
        {
            float step = (max - min) / sep;

            foreach (int curSep in Enumerable.Range(0, sep + 1))
            {
                yield return min + step * curSep;
            }
        }
#endif
    }
}
