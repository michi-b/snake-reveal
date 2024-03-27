using UnityEditor;
using UnityEngine;

namespace TextDisplay.Abstractions
{
    public abstract class IntDisplay : ValueDisplay<int>
    {
        // note that context menu methods cannot be in generic classes
        [ContextMenu("Apply")]
        private void ContextMenuApply()
        {
#if UNITY_EDITOR
            Undo.RegisterCompleteObjectUndo(gameObject, "Apply Int Display");
#endif
            Apply();
        }
    }
}