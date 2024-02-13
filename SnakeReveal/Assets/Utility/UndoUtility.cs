using System.Diagnostics;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Utility
{
    public class UndoUtility
    {
        [Conditional("UNITY_EDITOR")]
        public static void RecordFullGameObjectHierarchy(Component target, string operationName)
        {
            RecordFullGameObjectHierarchy(target.gameObject, operationName);
        }

        [Conditional("UNITY_EDITOR")]
        public static void RecordFullGameObjectHierarchy(GameObject target, string operationName)
        {
#if UNITY_EDITOR
            Undo.RegisterFullObjectHierarchyUndo(target, operationName);
#endif
        }
    }
}