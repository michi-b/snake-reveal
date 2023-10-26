using System;
using Unity.Mathematics;
using UnityEngine;

namespace Game.Simulation.Grid
{
    public static class GridDirectionExtensions
    {
        public static Vector2 Vector2(this GridDirection target)
        {
            return target switch
            {
                GridDirection.None => new Vector2(0f, 0f),
                GridDirection.Right => new Vector2(1f, 0f),
                GridDirection.Up => new Vector2(0f, 1f),
                GridDirection.Left => new Vector2(-1f, 0f),
                GridDirection.Down => new Vector2(0f, -1f),
                _ => throw new ArgumentOutOfRangeException(nameof(target), target, null)
            };
        }

        public static int2 ToInt2(this GridDirection target)
        {
            return target switch
            {
                GridDirection.None => new int2(0, 0),
                GridDirection.Right => new int2(1, 0),
                GridDirection.Up => new int2(0, 1),
                GridDirection.Left => new int2(-1, 0),
                GridDirection.Down => new int2(0, -1),
                _ => throw new ArgumentOutOfRangeException(nameof(target), target, null)
            };
        }
    }
}