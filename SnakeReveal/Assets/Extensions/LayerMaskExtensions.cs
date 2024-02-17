using UnityEngine;

namespace Extensions
{
    public static class LayerMaskExtensions
    {
        public static bool Contains(this LayerMask target, int layer) => (target & (1 << layer)) != 0;
    }
}