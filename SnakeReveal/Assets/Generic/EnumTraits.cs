using System;
using System.Collections.Generic;

namespace Generic
{
    public static class EnumTraits<TEnum> where TEnum : struct, Enum
    {
        static EnumTraits()
        {
            Values = (TEnum[])Enum.GetValues(typeof(TEnum));
            for (int i = 0; i < Values.Length; i++)
            {
                IndexByValue[Values[i]] = i;
#if DEBUG
                if (Convert.ToInt32(Values[i]) < 0)
                {
                    HasNegativeValues = true;
                }
#endif
            }
        }

#if DEBUG
        // ReSharper disable once StaticMemberInGenericType
        private static readonly bool HasNegativeValues;
#endif

        private static readonly Dictionary<TEnum, int> IndexByValue = new();
        private static TEnum[] Values { get; }

#if DEBUG
        public static int GetIndex(TEnum value)
        {
            if (HasNegativeValues)
            {
                throw new InvalidOperationException($"Enum {typeof(TEnum).Name} has negative values, which makes the value indices nonsense for some reason." +
                                                    $"Only use enum index for enums with exclusively non-negative values.");
            }

            return IndexByValue[value];
        }

#else
        public static int GetIndex(TEnum value) => IndexByValue[value];
#endif
        public static int Count => Values.Length;
    }
}