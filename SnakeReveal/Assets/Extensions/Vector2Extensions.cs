using Unity.Mathematics;
using UnityEngine;

namespace Extensions
{
    public static class Vector2Extensions
    {
        public static Vector3 ToVector3(this Vector2 vector2, float z)
        {
            return new Vector3(vector2.x, vector2.y, z);
        }

        public static int2 ToInt2(this Vector2 vector2)
        {
            return new int2((int)vector2.x, (int)vector2.y);
        }
    }
}