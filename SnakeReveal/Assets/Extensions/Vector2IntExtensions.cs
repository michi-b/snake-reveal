using System;
using Game.Grid;
using UnityEngine;

namespace Extensions
{
    public static class Vector2IntExtensions
    {
        public static Vector2 GetScenePosition(this Vector2Int gridPosition, SimulationGrid grid)
        {
            return grid.GetScenePosition(gridPosition);
        }

        public static int GetComponentMax(this Vector2Int target)
        {
            return Math.Max(target.x, target.y);
        }
    }
}