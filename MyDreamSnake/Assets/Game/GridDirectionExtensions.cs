using System;
using Unity.Mathematics;
using UnityEngine;

namespace Game
{
    public static class GridDirectionExtensions
    {
        private static readonly Quaternion RightRotation = Quaternion.Euler(0f, 0f, 0f);
        private static readonly Quaternion UpRotation = Quaternion.Euler(0f, 0f, 90f);
        private static readonly Quaternion LeftRotation = Quaternion.Euler(0f, 0f, 180f);
        private static readonly Quaternion DownRotation = Quaternion.Euler(0f, 0f, 270f);

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

        public static Quaternion ToRotation(this GridDirection target)
        {
            return target switch
            {
                GridDirection.None => throw new ArgumentOutOfRangeException(nameof(target), target,
                    "\"None\" direction has no rotation"),
                GridDirection.Right => RightRotation,
                GridDirection.Up => UpRotation,
                GridDirection.Left => LeftRotation,
                GridDirection.Down => DownRotation,
                _ => throw new ArgumentOutOfRangeException(nameof(target), target, null)
            };
        }
    }
}