using Unity.Mathematics;
using UnityEngine;

namespace Extensions
{
    public static class Int2Extensions
    {
        public static Vector2 ToVector2(this int2 int2)
        {
            return new Vector2(int2.x, int2.y);
        }
    }
}