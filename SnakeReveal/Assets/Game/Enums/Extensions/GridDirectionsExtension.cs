using Game.Enums.Utility;
using Game.Simulation.Grid;
using UnityEngine;

namespace Game.Enums.Extensions
{
    public static class GridDirectionsExtension
    {
        public static bool Contains(this GridDirections directions, GridDirection direction) => (directions & direction.AsFlag()) != 0;

        public static GridDirections WithDirection(this GridDirections directions, GridDirection direction) => directions | direction.AsFlag();

        public static GridDirections WithoutDirection(this GridDirections directions, GridDirection direction) => directions & ~direction.AsFlag();

        public static GridDirections RestrictInBounds(this GridDirections directions, SimulationGrid grid, Vector2Int position)
        {
            // ReSharper disable once LoopCanBeConvertedToQuery
            // No LINQ to avoid closure allocation
            foreach (GridDirection direction in GridDirectionUtility.ActualDirections)
            {
                if (!grid.GetCanMoveInDirectionInsideBounds(position, direction))
                {
                    directions = directions.WithoutDirection(direction);
                }
            }

            return directions;
        }

        public static GridDirections WithDirectionsBetween(this GridDirections directions, GridDirection start, GridDirection end, Turn turn)
        {
            if (start == end)
            {
                return directions;
            }

            for (GridDirection current = start.Turn(turn); current != end; current = current.Turn(turn))
            {
                directions = directions.WithDirection(current);
            }

            return directions;
        }
    }
}