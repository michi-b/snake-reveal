using Game.Enums;
using UnityEngine;

namespace Utility
{
    public static class Vector2IntUtility
    {
        public static GridDirection GetDirection(this Vector2Int start, Vector2Int end)
        {
            if ((start.x == end.x && start.y == end.y)
                || (start.x != end.x && start.y != end.y))
            {
                return GridDirection.None;
            }

            Vector2Int delta = end - start;

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