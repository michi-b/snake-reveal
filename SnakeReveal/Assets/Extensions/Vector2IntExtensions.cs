using Game;
using UnityEngine;

namespace Extensions
{
    public static class Vector2IntExtensions
    {
        public static Vector2 GetScenePosition(this Vector2Int gridPosition, SimulationGrid grid)
        {
            return grid.GetScenePosition(gridPosition);
        }
    }
}