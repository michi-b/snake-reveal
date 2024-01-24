using UnityEditor;
using UnityEngine;

namespace TextDisplay.Abstractions
{
    public abstract class IntDisplay : ValueDisplay<int>
    {
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