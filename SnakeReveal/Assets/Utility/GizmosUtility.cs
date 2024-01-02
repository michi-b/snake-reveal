using UnityEngine;

namespace Utility
{
    public static class GizmosUtility
    {
        private const float DefaultArrowAngle = 20.0f;
        private const float DefaultArrowHeadSize = 0.02f;

        public static void DrawArrow(Vector3 start, Vector3 end, float arrowHeadSize = DefaultArrowHeadSize, float arrowHeadAngle = DefaultArrowAngle)
        {
            Gizmos.DrawLine(start, end);
            DrawArrowHead(end, end - start, arrowHeadSize, arrowHeadAngle);
        }

        public static void DrawArrowHead(Vector3 end, Vector3 direction, float size = DefaultArrowHeadSize, float angle = DefaultArrowAngle)
        {
            Vector3 right = Quaternion.LookRotation(direction) * Quaternion.Euler(angle, 0, 0) * Vector3.back;
            Vector3 left = Quaternion.LookRotation(direction) * Quaternion.Euler(-angle, 0, 0) * Vector3.back;
            Vector3 up = Quaternion.LookRotation(direction) * Quaternion.Euler(0, angle, 0) * Vector3.back;
            Vector3 down = Quaternion.LookRotation(direction) * Quaternion.Euler(0, -angle, 0) * Vector3.back;
            Gizmos.DrawLine(end, end + right * size);
            Gizmos.DrawLine(end, end + left * size);
            Gizmos.DrawLine(end, end + up * size);
            Gizmos.DrawLine(end, end + down * size);
        }
    }
}