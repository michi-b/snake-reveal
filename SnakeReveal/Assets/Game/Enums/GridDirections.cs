using System;

namespace Game.Enums
{
    [Flags]
    public enum GridDirections
    {
        None = 0,
        Right = 1 << 0,
        Up = 1 << 1,
        Left = 1 << 2,
        Down = 1 << 3
    }
}