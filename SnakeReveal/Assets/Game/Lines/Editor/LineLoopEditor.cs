using UnityEditor;
using UnityEngine;

namespace Game.Lines.Editor
{
    [CustomEditor(typeof(LineLoop))]
    public class LineLoopEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            
            var lineLoop = (LineLoop) target;

            EditorGUILayout.Space();
            
            EditorGUILayout.LabelField("Initialization Tools");
            
            _grid = (SimulationGrid)EditorGUILayout.ObjectField(_grid, typeof(SimulationGrid), true);

            if (_grid == null)
            {
                return;
            }

            if (GUILayout.Button("Initialize Quad"))
            {
                // todo: initialize quad
            }
        }

        private SimulationGrid _grid;
    }
}
