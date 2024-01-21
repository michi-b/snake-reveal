using UnityEditor;
using UnityEngine;

namespace Game.Editor
{
    [CustomEditor(typeof(DrawnShape))]
    public class DrawnShapeEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            var shape = (DrawnShape)target;

            if (GUILayout.Button("Regenerate Quads"))
            {
                shape.EditModeRegenerateQuads();
            }
        }
    }
}