using Unity.Mathematics;

namespace Game.Enums
{
    public static class GridDirectionUtility
    {
        public static GridDirection GetDirection(int2 start, int2 end)
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