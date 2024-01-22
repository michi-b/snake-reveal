using UnityEngine;

namespace Utility
{
    public static class GizmosUtility
    {
        private const float DefaultArrowAngle = 25.0f;
        private const float DefaultArrowHeadSize = 0.02f;

        public static void DrawArrow(Vector3 start, Vector3 end, float arrowHeadSize = DefaultArrowHeadSize, float arrowHeadAngle = DefaultArrowAngle)
        {
            Gizmos.DrawLine(start, end);
            DrawArrowHead(end, end - start, arrowHeadSize, arrowHeadAngle);
        }

        public static void DrawArrowHead(Vector3 end, Vector3 direction, float size = DefaultArrowHeadSize, float angle = DefaultArrowAngle)
        {
            if (direction != Vector3.zero)
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

        public static void DrawRect(Vector2 bottomLeft, Vector2 topRight, float z, Color color)
        {
            Color gizmosColor = Gizmos.color;
            Gizmos.color = color;
            {
                var topLeft = new Vector2(bottomLeft.x, topRight.y);
                var bottomRight = new Vector2(topRight.x, bottomLeft.y);
                Gizmos.DrawLine(bottomLeft, topLeft);
                Gizmos.DrawLine(topLeft, topRight);
                Gizmos.DrawLine(topRight, bottomRight);
                Gizmos.DrawLine(bottomRight, bottomLeft);
            }
            Gizmos.color = gizmosColor;
        }
    }
}