using UnityEngine;

namespace Extensions
{
    public static class TransformExtensions
    {
        public static void SetLocalPositionXY(this Transform target, Vector2 xy)
        {
            target.localPosition = new Vector3(xy.x, xy.y, target.localPosition.z);
        }

        public static void ClearZ(this Transform target)
        {
            target.SetLocalPositionZ(0f);
        }

        public static void SetLocalPositionZ(this Transform target, float z)
        {
            Vector3 localPosition = target.localPosition;
            target.localPosition = new Vector3(localPosition.x, localPosition.y, z);
        }

        public static void SetWorldPositionZ(this Transform target, float z)
        {
            Vector3 position = target.position;
            target.position = new Vector3(position.x, position.y, z);
        }
    }
}