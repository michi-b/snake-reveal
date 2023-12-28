using UnityEngine;

namespace Extensions
{
    public static class Vector2Extensions
    {
        public static Vector3 ToVector3(this Vector2 vector2, float z)
        {
            return new Vector3(vector2.x, vector2.y, z);
        }
    }
}