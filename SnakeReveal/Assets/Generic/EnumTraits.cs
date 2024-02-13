using System;

namespace Generic
{
    public static class EnumTraits<TEnum> where TEnum : struct, Enum
    {
        static EnumTraits()
        {
            Values = (TEnum[])Enum.GetValues(typeof(TEnum));
        }

        public static TEnum[] Values { get; }
    }
}