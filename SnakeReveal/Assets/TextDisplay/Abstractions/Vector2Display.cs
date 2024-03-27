using UnityEditor;
using UnityEngine;

namespace TextDisplay.Abstractions
{
    public abstract class Vector2Display : ValueDisplay<Vector2>
    {
        // note that context menu methods cannot be in generic classes
        [ContextMenu("Apply")]
        private void ContextMenuApply()
        {
#if UNITY_EDITOR
            Undo.RegisterFullObjectHierarchyUndo(gameObject, "Apply Vector2 Display");
#endif
            Apply();
        }
    }
}