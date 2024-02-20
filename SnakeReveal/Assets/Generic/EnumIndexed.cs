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

        public TItem this[TEnum index]
        {
            get => _items[Convert.ToInt32(index)];
            set => _items[Convert.ToInt32(index)] = value;
        }

        [SerializeField] private TEnum _enum;
    }
}