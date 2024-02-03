using System;
using UnityEngine;

namespace Game.Enums.Extensions
{
    public static class GridDirectionExtensions
    {
        private static readonly Quaternion RightRotation = Quaternion.Euler(0f, 0f, 0f);
        private static readonly Quaternion UpRotation = Quaternion.Euler(0f, 0f, 90f);
        private static readonly Quaternion LeftRotation = Quaternion.Euler(0f, 0f, 180f);
        private static readonly Quaternion DownRotation = Quaternion.Euler(0f, 0f, 270f);

        public static Vector2Int AsVector(this GridDirection target)
        {
            return target switch
            {
                GridDirection.None => new Vector2Int(0, 0),
                GridDirection.Right => new Vector2Int(1, 0),
                GridDirection.Up => new Vector2Int(0, 1),
                GridDirection.Left => new Vector2Int(-1, 0),
                GridDirection.Down => new Vector2Int(0, -1),
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

        public static GridAxis GetAxis(this GridDirection target)
        {
            return target switch
            {
                GridDirection.None => throw new ArgumentOutOfRangeException(nameof(target), target, "\"None\" direction has no axis orientation"),
                GridDirection.Right => GridAxis.Horizontal,
                GridDirection.Up => GridAxis.Vertical,
                GridDirection.Left => GridAxis.Horizontal,
                GridDirection.Down => GridAxis.Vertical,
                _ => throw new ArgumentOutOfRangeException(nameof(target), target, null)
            };
        }

        public static Turn GetTurn(this GridDirection direction, GridDirection to)
        {
            return direction switch
            {
                GridDirection.None => Enums.Turn.None,
                GridDirection.Right => to switch
                {
                    GridDirection.None => Enums.Turn.None,
                    GridDirection.Right => Enums.Turn.None,
                    GridDirection.Up => Enums.Turn.Left,
                    GridDirection.Left => Enums.Turn.None,
                    GridDirection.Down => Enums.Turn.Right,
                    _ => throw new ArgumentOutOfRangeException(nameof(to), to, null)
                },
                GridDirection.Up => to switch
                {
                    GridDirection.None => Enums.Turn.None,
                    GridDirection.Right => Enums.Turn.Right,
                    GridDirection.Up => Enums.Turn.None,
                    GridDirection.Left => Enums.Turn.Left,
                    GridDirection.Down => Enums.Turn.None,
                    _ => throw new ArgumentOutOfRangeException(nameof(to), to, null)
                },
                GridDirection.Left => to switch
                {
                    GridDirection.None => Enums.Turn.None,
                    GridDirection.Right => Enums.Turn.None,
                    GridDirection.Up => Enums.Turn.Right,
                    GridDirection.Left => Enums.Turn.None,
                    GridDirection.Down => Enums.Turn.Left,
                    _ => throw new ArgumentOutOfRangeException(nameof(to), to, null)
                },
                GridDirection.Down => to switch
                {
                    GridDirection.None => Enums.Turn.None,
                    GridDirection.Right => Enums.Turn.Left,
                    GridDirection.Up => Enums.Turn.None,
                    GridDirection.Left => Enums.Turn.Right,
                    GridDirection.Down => Enums.Turn.None,
                    _ => throw new ArgumentOutOfRangeException(nameof(to), to, null)
                },
                _ => throw new ArgumentOutOfRangeException(nameof(direction), direction, null)
            };
        }

        public static GridDirection Reverse(this GridDirection target)
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

        public static GridDirection Turn(this GridDirection target, Turn turn)
        {
            return turn switch
            {
                Enums.Turn.None => target,
                Enums.Turn.Right => target switch
                {
                    GridDirection.None => GridDirection.None,
                    GridDirection.Right => GridDirection.Down,
                    GridDirection.Up => GridDirection.Right,
                    GridDirection.Left => GridDirection.Up,
                    GridDirection.Down => GridDirection.Left,
                    _ => throw new ArgumentOutOfRangeException(nameof(target), target, null)
                },
                Enums.Turn.Left => target switch
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

        public static GridDirection TurnInsideCorner(this GridDirection direction, GridCorner corner)
        {
            return direction switch
            {
                GridDirection.None => throw new ArgumentOutOfRangeException(nameof(direction), direction, null),
                GridDirection.Right => corner switch
                {
                    GridCorner.None => throw new ArgumentOutOfRangeException(nameof(corner), corner, null),
                    GridCorner.BottomLeft => throw new ArgumentOutOfRangeException(nameof(corner), corner, null),
                    GridCorner.TopLeft => throw new ArgumentOutOfRangeException(nameof(corner), corner, null),
                    GridCorner.TopRight => GridDirection.Down,
                    GridCorner.BottomRight => GridDirection.Up,
                    _ => throw new ArgumentOutOfRangeException(nameof(corner), corner, null)
                },
                GridDirection.Up => corner switch
                {
                    GridCorner.None => throw new ArgumentOutOfRangeException(nameof(corner), corner, null),
                    GridCorner.BottomLeft => throw new ArgumentOutOfRangeException(nameof(corner), corner, null),
                    GridCorner.TopLeft => GridDirection.Right,
                    GridCorner.TopRight => GridDirection.Left,
                    GridCorner.BottomRight => throw new ArgumentOutOfRangeException(nameof(corner), corner, null),
                    _ => throw new ArgumentOutOfRangeException(nameof(corner), corner, null)
                },
                GridDirection.Left => corner switch
                {
                    GridCorner.None => throw new ArgumentOutOfRangeException(nameof(corner), corner, null),
                    GridCorner.BottomLeft => GridDirection.Up,
                    GridCorner.TopLeft => GridDirection.Down,
                    GridCorner.TopRight => throw new ArgumentOutOfRangeException(nameof(corner), corner, null),
                    GridCorner.BottomRight => throw new ArgumentOutOfRangeException(nameof(corner), corner, null),
                    _ => throw new ArgumentOutOfRangeException(nameof(corner), corner, null)
                },
                GridDirection.Down => corner switch
                {
                    GridCorner.None => throw new ArgumentOutOfRangeException(nameof(corner), corner, null),
                    GridCorner.BottomLeft => GridDirection.Right,
                    GridCorner.TopLeft => throw new ArgumentOutOfRangeException(nameof(corner), corner, null),
                    GridCorner.TopRight => throw new ArgumentOutOfRangeException(nameof(corner), corner, null),
                    GridCorner.BottomRight => GridDirection.Left,
                    _ => throw new ArgumentOutOfRangeException(nameof(corner), corner, null)
                },
                _ => throw new ArgumentOutOfRangeException(nameof(direction), direction, null)
            };
        }
    }
}