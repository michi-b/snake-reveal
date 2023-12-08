using UnityEngine;

namespace Extensions
{
    public static class TransformExtensions
    {
        public static void SetLocalPosition(this Transform transform, Vector2 xy)
        {
            transform.localPosition = new Vector3(xy.x, xy.y, transform.localPosition.z);
        }
    }
}