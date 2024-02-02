using Game.Enums.Extensions;

namespace Game.Enums
{
    /// <summary>
    /// 2D side on a grid, like of a quad.
    /// </summary>
    /// <remarks>
    /// Integer values of members are essential, as they represent the clockwise turn count from the bottom side,
    /// and this is made use of in <see cref="SideExtensions.GetClockwiseTurnWeight"/>.
    /// </remarks>
    public enum GridSide
    {
        None = -1,
        Bottom = 0,
        Left = 1,
        Top = 2,
        Right = 3
    }
}