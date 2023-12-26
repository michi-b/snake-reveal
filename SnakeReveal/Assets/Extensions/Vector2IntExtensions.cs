using Unity.Mathematics;
using UnityEngine;

namespace Extensions
{
    public static class Vector2IntExtensions
    {
        public static int2 ToInt2(this Vector2Int vector2Int)
        {
            return new int2(vector2Int.x, vector2Int.y);
        }
    }
}