using System.Collections.Generic;

namespace Game.Enums.Utility
{
    public static class GridDirectionUtility
    {
        static GridDirectionUtility()
        {
            ActualDirections = new List<GridDirection>
            {
                GridDirection.Right,
                GridDirection.Up,
                GridDirection.Left,
                GridDirection.Down
            };
        }

        // all defined directions without <see cref="GridDirection.None"/>
        public static readonly IReadOnlyList<GridDirection> ActualDirections;
    }
}