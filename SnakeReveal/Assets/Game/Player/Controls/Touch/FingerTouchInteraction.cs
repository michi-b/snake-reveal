using System;

namespace Game.Player.Controls.Touch
{
    [Flags]
    public enum FingerTouchInteraction
    {
        None = 0,
        IsTouching = 1 << 0,
        HasTouched = 1 << 1,
        HasTouchedAndIsTouching = IsTouching | HasTouched
    }
}