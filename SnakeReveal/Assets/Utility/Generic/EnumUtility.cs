using System;

namespace Utility.Generic
{
    public static class EnumUtility
    {
        public static TEnum SetFlagEnabled<TEnum>(TEnum flags, TEnum flag, bool enableFlag)
            where TEnum : struct, Enum
        {
            if (enableFlag)
            {
                flags = (TEnum)(object)((int)(object)flags | (int)(object)flag);
            }
            else
            {
                flags = (TEnum)(object)((int)(object)flags & ~(int)(object)flag);
            }

            return flags;
        }
    }
}