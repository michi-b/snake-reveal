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
                _indexByValue[Values[i]] = i;
            }
        }

        private static readonly Dictionary<TEnum, int> _indexByValue = new Dictionary<TEnum, int>();
        public static TEnum[] Values { get; }
        public static int GetIndex(TEnum value) => _indexByValue[value];
        public static int Count => Values.Length;
    }
}