using System.Diagnostics;
using UnityEngine;
using Utility.Generic;

namespace Extensions
{
    public static class GameObjectExtensions
    {
        [Conditional("UNITY_EDITOR")]
        public static void SetVisibleInSceneView(this GameObject target, bool visible)
        {
#if UNITY_EDITOR
            target.hideFlags = EnumUtility.SetFlagEnabled(target.hideFlags, HideFlags.HideInHierarchy, !visible);
#endif
        }
    }
}