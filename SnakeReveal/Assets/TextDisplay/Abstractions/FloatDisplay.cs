using UnityEditor;
using UnityEngine;

namespace TextDisplay.Abstractions
{
    public abstract class FloatDisplay : ValueDisplay<float>
    {
        // note that context menu methods cannot be in generic classes
        [ContextMenu("Apply")]
        private void ContextMenuApply()
        {
#if UNITY_EDITOR
            Undo.RegisterFullObjectHierarchyUndo(gameObject, "Apply Float Display");
#endif
            Apply();
        }
    }
}