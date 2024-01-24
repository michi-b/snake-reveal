using UnityEditor;
using UnityEngine;

namespace TextDisplay.Abstractions
{
    public abstract class FloatDisplay : ValueDisplay<float>
    {
        [ContextMenu("Apply")]
        private void ContextMenuApply()
        {
#if UNITY_EDITOR
            Undo.RecordObject(Renderer, "Apply Float Display");
#endif
            Apply();
        }
    }
}