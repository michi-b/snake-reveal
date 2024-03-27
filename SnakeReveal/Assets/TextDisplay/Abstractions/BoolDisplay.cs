using UnityEditor;
using UnityEngine;

namespace TextDisplay.Abstractions
{
    public abstract class BoolDisplay : ValueDisplay<bool>
    {
        // note that context menu methods cannot be in generic classes
        [ContextMenu("Apply")]
        private void ContextMenuApply()
        {
#if UNITY_EDITOR
            Undo.RegisterFullObjectHierarchyUndo(gameObject, "Apply Bool Display");
#endif
            Apply();
        }
    }
}