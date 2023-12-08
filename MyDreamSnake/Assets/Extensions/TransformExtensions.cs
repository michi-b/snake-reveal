using UnityEngine;

namespace Extensions
{
    public static class TransformExtensions
    {
        public static void SetLocalPositionXY(this Transform transform, Vector2 xy)
        {
            transform.localPosition = new Vector3(xy.x, xy.y, transform.localPosition.z);
        }

        public static void SetLocalPositionZ(this Transform transform, float z)
        {
            Vector3 localPosition = transform.localPosition;
            transform.localPosition = new Vector3(localPosition.x, localPosition.y, z);
        }
    }
}