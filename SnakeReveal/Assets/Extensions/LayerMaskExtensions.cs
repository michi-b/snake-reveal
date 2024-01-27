using UnityEngine;

namespace Extensions
{
    public static class LayerMaskExtensions
    {
        public static bool Contains(this LayerMask target, int layer)
        {
            return (target & (1 << layer)) != 0;
        }
    }
}