using Editor;
using UnityEditor;
using UnityEngine;

namespace Game.Grid.Editor
{
    [CustomEditor(typeof(GridPlacement))]
    public class SimulationGridTransformEditor : UnityEditor.Editor
    {
        protected virtual void OnEnable()
        {
            serializedObject.FindDirectChild(GridPlacement.PositionPropertyName);
            Tools.hidden = true;
        }

        protected virtual void OnDisable()
        {
            Tools.hidden = false;
        }

        protected virtual void OnSceneGUI()
        {
            var placement = (GridPlacement)target;
            SimulationGrid grid = placement.Grid;
            if (grid == null)
            {
                return;
            }

            if (HandlesUtility.TryGridHandleMove(placement.Position, placement.transform.position.z, grid, out Vector2Int newGridPosition))
            {
                placement.RecordUndo("Move Grid Transform Handle");
                placement.Position = newGridPosition;
            }
        }

        public override void OnInspectorGUI()
        {
            EditorGUI.BeginChangeCheck();
            base.OnInspectorGUI();
            bool defaultEditorChanged = EditorGUI.EndChangeCheck();

            var transform = (GridPlacement)target;

            if (defaultEditorChanged)
            {
                if (transform.Grid != null)
                {
                    transform.RecordUndo("Apply Grid Transform");
                    transform.Apply();
                }
            }

            if (GUILayout.Button("Move To Grid Center"))
            {
                transform.RecordUndo("Move To Grid Center");
                transform.Position = transform.Grid.CenterPosition;
            }
        }
    }
}