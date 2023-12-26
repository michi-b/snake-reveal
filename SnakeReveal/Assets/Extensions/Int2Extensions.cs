using Game.Enums;
using Unity.Mathematics;
using UnityEngine;

namespace Extensions
{
    public static class Int2Extensions
    {
        public static Vector2 ToVector2(this int2 target)
        {
            return new Vector2(target.x, target.y);
        }
        
        public static Vector2Int ToVector2Int(this int2 target)
        {
            return new Vector2Int(target.x, target.y);
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