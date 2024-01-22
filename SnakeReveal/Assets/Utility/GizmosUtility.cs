using UnityEditor;
using UnityEngine;

namespace Utility
{
    public static class GizmosUtility
    {
        private const float DefaultArrowAngle = 25.0f;
        private const float DefaultArrowHeadSize = 0.02f;

        public static void DrawArrow(Vector3 startWorldPosition, Vector3 endWorldPosition)
        {
            Gizmos.DrawLine(startWorldPosition, endWorldPosition);
            Vector3 direction = endWorldPosition - startWorldPosition;
            GizmosUtility.DrawArrowHead(endWorldPosition, direction);
        }

        private static void DrawArrowHead(Vector3 position, Vector3 direction, float angle = DefaultArrowAngle)
        {
            // Handle Utility is only available in editor
            #if UNITY_EDITOR
            float size = HandleUtility.GetHandleSize(position) * 0.2f;
            if (direction != Vector3.zero)
            {
                Vector3 right = Quaternion.LookRotation(direction) * Quaternion.Euler(angle, 0, 0) * Vector3.back;
                Vector3 left = Quaternion.LookRotation(direction) * Quaternion.Euler(-angle, 0, 0) * Vector3.back;
                Vector3 up = Quaternion.LookRotation(direction) * Quaternion.Euler(0, angle, 0) * Vector3.back;
                Vector3 down = Quaternion.LookRotation(direction) * Quaternion.Euler(0, -angle, 0) * Vector3.back;
                Gizmos.DrawLine(position, position + right * size);
                Gizmos.DrawLine(position, position + left * size);
                Gizmos.DrawLine(position, position + up * size);
                Gizmos.DrawLine(position, position + down * size);
            }
            #endif
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