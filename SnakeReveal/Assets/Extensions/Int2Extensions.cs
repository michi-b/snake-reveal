using Game.Enums;
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

        public static GridDirection GetDirection(this int2 start, int2 end)
        {
            if (start.x != end.x && start.y != end.y)
            {
                return GridDirection.None;
            }

            int2 delta = end - start;

            return delta.x switch
            {
                > 0 => GridDirection.Right,
                < 0 => GridDirection.Left,
                _ => delta.y switch
                {
                    > 0 => GridDirection.Up,
                    < 0 => GridDirection.Down,
                    _ => GridDirection.None
                }
            };
        }
    }
}