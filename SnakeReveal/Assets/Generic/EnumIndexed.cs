using System;
using UnityEngine;

namespace Generic
{
    [Serializable]
    public class EnumIndexed<TEnum, TItem> : ISerializationCallbackReceiver where TEnum : struct, Enum
    {
        [SerializeField] private TItem[] _items = new TItem[EnumTraits<TEnum>.Count];

        public void OnBeforeSerialize()
        {
            int count = EnumTraits<TEnum>.Count;
            if (_items.Length != count)
            {
                Array.Resize(ref _items, count);
            }
        }

        public void OnAfterDeserialize()
        {
        }

        public TItem this[TEnum key]
        {
            get
            {
                int index = GetIndex(key);
                return _items[index];
            }
            set => _items[GetIndex(key)] = value;
        }

        private static int GetIndex(TEnum key) => EnumTraits<TEnum>.GetIndex(key);

#pragma warning disable CS0414 // Field is assigned but its value is never used
        // HACK: this unused field is "used" by the custom property drawer to get enum members via Unity GUI API
        [SerializeField] private TEnum _enum;
#pragma warning restore CS0414 // Field is assigned but its value is never used
    }
}