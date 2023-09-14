using UnityEngine;

namespace ExtInspector.GizmosDraw
{
    public static class Arrow
    {
        public static void Draw(Vector3 from, Vector3 to, float arrowHeadLength = 1f, float arrowHeadAngle = 20.0f, Vector3? forward = null)
        {
#if UNITY_EDITOR
            if (Mathf.Approximately(Vector3.Distance(from, to), 0f))
            {
                return;
            }

            Vector3 forwardDirection = forward ?? Vector3.forward;

            Vector3 direction = to - from;
            Gizmos.DrawRay(from, direction);

            Vector3 right = Quaternion.LookRotation(direction) * Quaternion.Euler(0,180+arrowHeadAngle,0) * forwardDirection;
            Vector3 left = Quaternion.LookRotation(direction) * Quaternion.Euler(0,180-arrowHeadAngle,0) * forwardDirection;
            Gizmos.DrawRay(to, right * arrowHeadLength);
            Gizmos.DrawRay(to, left * arrowHeadLength);
#endif
        }
    }
}
