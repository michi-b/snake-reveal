using System;
using Unity.Mathematics;
using UnityEngine;

namespace Game.Enums
{
    public static class GridDirectionExtensions
    {
        private static readonly Quaternion RightRotation = Quaternion.Euler(0f, 0f, 0f);
        private static readonly Quaternion UpRotation = Quaternion.Euler(0f, 0f, 90f);
        private static readonly Quaternion LeftRotation = Quaternion.Euler(0f, 0f, 180f);
        private static readonly Quaternion DownRotation = Quaternion.Euler(0f, 0f, 270f);

        public static Vector2 ToVector2(this GridDirection target)
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

        public static Quaternion GetRotation(this GridDirection target)
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

        public static AxisOrientation GetOrientation(this GridDirection target)
        {
            return target switch
            {
                GridDirection.None => throw new ArgumentOutOfRangeException(nameof(target), target, "\"None\" direction has no axis orientation"),
                GridDirection.Right => AxisOrientation.Horizontal,
                GridDirection.Up => AxisOrientation.Vertical,
                GridDirection.Left => AxisOrientation.Horizontal,
                GridDirection.Down => AxisOrientation.Vertical,
                _ => throw new ArgumentOutOfRangeException(nameof(target), target, null)
            };
        }

        public static Turn GetTurn(this GridDirection target, GridDirection to)
        {
            return target switch
            {
                GridDirection.None => Turn.None,
                GridDirection.Right => to switch
                {
                    GridDirection.None => Turn.None,
                    GridDirection.Right => Turn.None,
                    GridDirection.Up => Turn.CounterClockwise,
                    GridDirection.Left => Turn.None,
                    GridDirection.Down => Turn.Clockwise,
                    _ => throw new ArgumentOutOfRangeException(nameof(to), to, null)
                },
                GridDirection.Up => to switch
                {
                    GridDirection.None => Turn.None,
                    GridDirection.Right => Turn.Clockwise,
                    GridDirection.Up => Turn.None,
                    GridDirection.Left => Turn.CounterClockwise,
                    GridDirection.Down => Turn.None,
                    _ => throw new ArgumentOutOfRangeException(nameof(to), to, null)
                },
                GridDirection.Left => to switch
                {
                    GridDirection.None => Turn.None,
                    GridDirection.Right => Turn.None,
                    GridDirection.Up => Turn.Clockwise,
                    GridDirection.Left => Turn.None,
                    GridDirection.Down => Turn.CounterClockwise,
                    _ => throw new ArgumentOutOfRangeException(nameof(to), to, null)
                },
                GridDirection.Down => to switch
                {
                    GridDirection.None => Turn.None,
                    GridDirection.Right => Turn.CounterClockwise,
                    GridDirection.Up => Turn.None,
                    GridDirection.Left => Turn.Clockwise,
                    GridDirection.Down => Turn.None,
                    _ => throw new ArgumentOutOfRangeException(nameof(to), to, null)
                },
                _ => throw new ArgumentOutOfRangeException(nameof(target), target, null)
            };
        }

        public static GridDirection GetOpposite(this GridDirection target)
        {
            return target switch
            {
                GridDirection.None => GridDirection.None,
                GridDirection.Right => GridDirection.Left,
                GridDirection.Up => GridDirection.Down,
                GridDirection.Left => GridDirection.Right,
                GridDirection.Down => GridDirection.Up,
                _ => throw new ArgumentOutOfRangeException(nameof(target), target, null)
            };
        }

        public static bool IsOpposite(this GridDirection target, GridDirection other)
        {
            return target.GetOpposite() == other;
        }

        public static bool IsSameOrOpposite(this GridDirection target, GridDirection other)
        {
            return target == other || target.IsOpposite(other);
        }

        public static GridDirection GetTurned(this GridDirection target, Turn turn)
        {
            return turn switch
            {
                Turn.None => target,
                Turn.Clockwise => target switch
                {
                    GridDirection.None => GridDirection.None,
                    GridDirection.Right => GridDirection.Down,
                    GridDirection.Up => GridDirection.Right,
                    GridDirection.Left => GridDirection.Up,
                    GridDirection.Down => GridDirection.Left,
                    _ => throw new ArgumentOutOfRangeException(nameof(target), target, null)
                },
                Turn.CounterClockwise => target switch
                {
                    GridDirection.None => GridDirection.None,
                    GridDirection.Right => GridDirection.Up,
                    GridDirection.Up => GridDirection.Left,
                    GridDirection.Left => GridDirection.Down,
                    GridDirection.Down => GridDirection.Right,
                    _ => throw new ArgumentOutOfRangeException(nameof(target), target, null)
                },
                _ => throw new ArgumentOutOfRangeException(nameof(turn), turn, null)
            };
        }
    }
}